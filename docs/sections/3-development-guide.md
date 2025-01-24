# Development Guide

## Project Structure

```
TaskoMask/
├── src/
│   ├── 1-BuildingBlocks/           # Shared libraries and components
│   │   ├── Domain/                 # Core domain objects
│   │   ├── Application/            # Application services
│   │   ├── Infrastructure/         # Technical implementations
│   │   └── Contracts/              # Shared DTOs and events
│   │
│   ├── 2-Services/                 # Microservices
│   │   ├── Tasks/                  # Task management service
│   │   │   ├── Api/               
│   │   │   │   ├── Tasks.Read.Api/
│   │   │   │   └── Tasks.Write.Api/
│   │   │   └── Tests/
│   │   │
│   │   ├── Boards/                 # Board management service
│   │   │   ├── Api/
│   │   │   │   ├── Boards.Read.Api/
│   │   │   │   └── Boards.Write.Api/
│   │   │   └── Tests/
│   │   │
│   │   └── Identity/               # Identity service
│   │       ├── Api/
│   │       └── Tests/
│   │
│   └── 3-ApiGateways/             # API Gateways
│       └── UserPanel/
│
├── tests/                          # Solution-wide tests
│   ├── Integration.Tests/
│   └── Load.Tests/
│
└── tools/                          # Development tools and scripts
```

## Development Setup

### 1. Prerequisites
- .NET 6.0 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code
- Git

### 2. Initial Setup
```powershell
# Clone repository
git clone https://github.com/yourusername/TaskoMask.git
cd TaskoMask

# Restore dependencies
dotnet restore TaskoMask.sln

# Start infrastructure services
docker-compose -f docker-compose.infrastructure.yml up -d

# Build solution
dotnet build TaskoMask.sln
```

### 3. Development Workflow

#### Creating a New Feature
1. Create feature branch:
```bash
git checkout -b feature/your-feature-name
```

2. Implement the feature following the DDD and Clean Architecture principles:

```csharp
// Domain Entity
public class Task : Entity, IAggregateRoot
{
    public string Title { get; private set; }
    public TaskStatus Status { get; private set; }
    
    private Task() { } // For EF Core
    
    public Task(string title)
    {
        Title = title;
        Status = TaskStatus.ToDo;
        AddDomainEvent(new TaskCreatedEvent(this));
    }
    
    public void UpdateStatus(TaskStatus newStatus)
    {
        Status = newStatus;
        AddDomainEvent(new TaskStatusUpdatedEvent(this));
    }
}

// Application Command
public class CreateTaskCommand : IRequest<Result<string>>
{
    public string Title { get; }
    
    public CreateTaskCommand(string title)
    {
        Title = title;
    }
}

// Command Handler
public class CreateTaskCommandHandler 
    : IRequestHandler<CreateTaskCommand, Result<string>>
{
    private readonly ITaskRepository _repository;
    
    public async Task<Result<string>> Handle(
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var task = new Task(command.Title);
        await _repository.AddAsync(task);
        return Result.Success(task.Id);
    }
}
```

3. Add tests:

```csharp
public class CreateTaskTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        // Arrange
        var repository = new Mock<ITaskRepository>();
        var handler = new CreateTaskCommandHandler(repository.Object);
        var command = new CreateTaskCommand("Test Task");
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        repository.Verify(r => r.AddAsync(It.IsAny<Task>()), Times.Once);
    }
}
```

## Testing Strategy

### 1. Unit Tests
Focus on testing business logic in isolation:

```csharp
public class TaskTests
{
    [Fact]
    public void UpdateStatus_ChangesStatus_RaisesEvent()
    {
        // Arrange
        var task = new Task("Test Task");
        
        // Act
        task.UpdateStatus(TaskStatus.InProgress);
        
        // Assert
        Assert.Equal(TaskStatus.InProgress, task.Status);
        Assert.Contains(task.DomainEvents, 
            e => e is TaskStatusUpdatedEvent);
    }
}
```

### 2. Integration Tests
Test multiple components working together:

```csharp
public class TasksApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateTaskRequest { Title = "Test Task" };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/tasks", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### 3. Event Sourcing Tests
Verify event handling and replay:

```csharp
public class TaskEventSourceTests
{
    [Fact]
    public async Task ReplayEvents_RestoresTaskState()
    {
        // Arrange
        var events = new List<IDomainEvent>
        {
            new TaskCreatedEvent("Test Task"),
            new TaskStatusUpdatedEvent(TaskStatus.InProgress),
            new TaskStatusUpdatedEvent(TaskStatus.Done)
        };
        
        // Act
        var task = await TaskEventRebuilder.Replay(events);
        
        // Assert
        Assert.Equal(TaskStatus.Done, task.Status);
    }
}
```

## Coding Standards

### 1. Naming Conventions
- Use PascalCase for public members and types
- Use camelCase for private fields
- Prefix interfaces with 'I'
- Use meaningful, descriptive names

### 2. Code Organization
- One class per file
- Group related files in folders
- Keep classes focused and small
- Follow Clean Architecture layers

### 3. Error Handling
```csharp
public class ErrorHandling
{
    public async Task<Result<T>> TryOperation<T>(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return Result.Success(result);
        }
        catch (DomainException ex)
        {
            return Result.Failure<T>(ex.Message);
        }
        catch (Exception ex)
        {
            // Log unexpected error
            _logger.LogError(ex, "Unexpected error occurred");
            return Result.Failure<T>("An unexpected error occurred");
        }
    }
}
```

### 4. Documentation
Add XML comments for public APIs:

```csharp
/// <summary>
/// Updates the status of a task and raises appropriate events
/// </summary>
/// <param name="newStatus">The new status to set</param>
/// <returns>Result indicating success or failure</returns>
/// <exception cref="InvalidOperationException">
/// Thrown when status transition is invalid
/// </exception>
public Result UpdateStatus(TaskStatus newStatus)
{
    // Implementation
}
```

## Debugging and Troubleshooting

### 1. Logging
Use structured logging:

```csharp
public class TaskService
{
    private readonly ILogger<TaskService> _logger;
    
    public async Task<Result> ProcessTask(string taskId)
    {
        _logger.LogInformation(
            "Processing task {TaskId} started at {StartTime}",
            taskId,
            DateTime.UtcNow);
            
        try
        {
            // Process task
            _logger.LogInformation(
                "Task {TaskId} processed successfully",
                taskId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing task {TaskId}",
                taskId);
            return Result.Failure(ex.Message);
        }
    }
}
```

### 2. Debugging Tools
- Use Visual Studio debugger
- Docker container logs
- Application Insights
- OpenTelemetry traces

### 3. Common Issues
1. Event Store Connection:
```csharp
public async Task<bool> VerifyEventStoreConnection()
{
    try
    {
        await _redisConnection.GetDatabase().PingAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to connect to Event Store");
        return false;
    }
}
```

2. Message Queue Issues:
```csharp
public class MessageQueueHealthCheck : IHealthCheck
{
    private readonly IBusControl _bus;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = await _bus.GetSendEndpoint(
                new Uri("queue:health-check"));
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```
