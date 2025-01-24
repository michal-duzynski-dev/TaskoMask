# Deployment Guide

## Infrastructure Requirements

### 1. Core Services
- .NET 6.0 Runtime
- Redis 6.x or higher
- RabbitMQ 3.8 or higher
- MongoDB 4.4 or higher

### 2. Development Tools
- Docker Desktop
- .NET SDK 6.0
- Visual Studio 2022 or VS Code

## Docker Deployment

### 1. Service Containerization
Example Dockerfile for Tasks.Write.Api:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/2-Services/Tasks/Api/Tasks.Write.Api/Tasks.Write.Api.csproj", "Tasks.Write.Api/"]
RUN dotnet restore "Tasks.Write.Api/Tasks.Write.Api.csproj"
COPY . .
WORKDIR "/src/Tasks.Write.Api"
RUN dotnet build "Tasks.Write.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tasks.Write.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasks.Write.Api.dll"]
```

### 2. Docker Compose Configuration

```yaml
version: '3.8'

services:
  redis:
    image: redis:6.2-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
    
  rabbitmq:
    image: rabbitmq:3.8-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=taskomask
      - RABBITMQ_DEFAULT_PASS=your_secure_password
      
  mongodb:
    image: mongo:4.4
    ports:
      - "27017:27017"
    volumes:
      - mongodb-data:/data/db
    environment:
      - MONGO_INITDB_ROOT_USERNAME=taskomask
      - MONGO_INITDB_ROOT_PASSWORD=your_secure_password
      
  identity-api:
    build:
      context: .
      dockerfile: src/2-Services/Identity/Api/Identity.Api/Dockerfile
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      
  tasks-write-api:
    build:
      context: .
      dockerfile: src/2-Services/Tasks/Api/Tasks.Write.Api/Dockerfile
    ports:
      - "5002:80"
    depends_on:
      - redis
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - EventStore__ConnectionString=redis:6379
      - RabbitMQ__Host=rabbitmq
      
  tasks-read-api:
    build:
      context: .
      dockerfile: src/2-Services/Tasks/Api/Tasks.Read.Api/Dockerfile
    ports:
      - "5003:80"
    depends_on:
      - mongodb
      - rabbitmq
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoDB__ConnectionString=mongodb://taskomask:your_secure_password@mongodb:27017
      - RabbitMQ__Host=rabbitmq

volumes:
  redis-data:
  rabbitmq-data:
  mongodb-data:
```

## Kubernetes Deployment

### 1. Service Configuration

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: tasks-write-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: tasks-write-api
  template:
    metadata:
      labels:
        app: tasks-write-api
    spec:
      containers:
      - name: tasks-write-api
        image: taskomask/tasks-write-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: EventStore__ConnectionString
          valueFrom:
            secretKeyRef:
              name: redis-secret
              key: connection-string
        - name: RabbitMQ__Host
          valueFrom:
            configMapKeyRef:
              name: rabbitmq-config
              key: host
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
```

### 2. Service Discovery

```yaml
apiVersion: v1
kind: Service
metadata:
  name: tasks-write-api
spec:
  selector:
    app: tasks-write-api
  ports:
    - protocol: TCP
      port: 80
      targetPort: 80
  type: ClusterIP
```

### 3. Ingress Configuration

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: taskomask-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: api.taskomask.com
    http:
      paths:
      - path: /tasks/write
        pathType: Prefix
        backend:
          service:
            name: tasks-write-api
            port:
              number: 80
      - path: /tasks/read
        pathType: Prefix
        backend:
          service:
            name: tasks-read-api
            port:
              number: 80
```

## Configuration Management

### 1. Application Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "EventStore": {
    "ConnectionString": "localhost:6379",
    "Database": 0
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "JWT": {
    "Authority": "https://identity.taskomask.com",
    "Audience": "tasks-api",
    "RequireHttpsMetadata": true
  },
  "OpenTelemetry": {
    "ServiceName": "Tasks.Write.Api",
    "Endpoint": "http://otel-collector:4317"
  }
}
```

### 2. Secret Management

Using Azure Key Vault (example):

```csharp
public static class KeyVaultExtensions
{
    public static void AddAzureKeyVault(this IServiceCollection services, IConfiguration config)
    {
        var keyVaultUrl = config["KeyVault:Url"];
        var credential = new DefaultAzureCredential();
        
        services.Configure<AzureKeyVaultConfigurationOptions>(options =>
        {
            options.ReloadInterval = TimeSpan.FromHours(1);
        });
        
        services.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
    }
}
```

## Monitoring Setup

### 1. Health Checks

```csharp
public static class HealthCheckExtensions
{
    public static void AddHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddRedis(config["EventStore:ConnectionString"], name: "redis")
            .AddRabbitMQ(config["RabbitMQ:ConnectionString"], name: "rabbitmq")
            .AddMongoDb(config["MongoDB:ConnectionString"], name: "mongodb");
    }
}
```

### 2. Logging Configuration

```csharp
public static class LoggingExtensions
{
    public static void ConfigureLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.AddApplicationInsights();
        
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("System", LogLevel.Warning);
    }
}
```
