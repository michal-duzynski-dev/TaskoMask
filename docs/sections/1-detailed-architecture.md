# Detailed Architecture

## Service Communication Patterns

### 1. Synchronous Communication
TaskoMask uses gRPC for efficient service-to-service communication where immediate response is required:

```csharp
public class GetBoardByIdHandler : IRequestHandler<GetBoardByIdRequest, BoardDetailsViewModel>
{
    private readonly GetBoardByIdGrpcServiceClient _getBoardByIdGrpcServiceClient;
    private readonly GetCardsByBoardIdGrpcServiceClient _getCardsByBoardIdGrpcServiceClient;

    public async Task<BoardDetailsViewModel> Handle(GetBoardByIdRequest request)
    {
        var board = await GetBoardAsync(request.Id);
        var cards = await GetCardsAsync(request.Id);
        return new BoardDetailsViewModel { Board = board, Cards = cards };
    }
}
```

### 2. Asynchronous Communication
Event-driven communication using MassTransit and RabbitMQ:

```csharp
public class TaskStatusUpdatedConsumer : IConsumer<TaskStatusUpdated>
{
    private readonly ITaskReadModelRepository _repository;

    public async Task Consume(ConsumeContext<TaskStatusUpdated> context)
    {
        var @event = context.Message;
        await _repository.UpdateTaskStatus(@event.TaskId, @event.NewStatus);
    }
}
```

## Domain Model Design

### 1. Aggregate Roots
Example of Task aggregate:

```csharp
public class Task : Entity, IAggregateRoot
{
    private readonly List<Comment> _comments;
    
    public string Title { get; private set; }
    public TaskStatus Status { get; private set; }
    public string AssigneeId { get; private set; }
    
    public void UpdateStatus(TaskStatus newStatus)
    {
        Status = newStatus;
        AddDomainEvent(new TaskStatusUpdated(Id, newStatus));
    }
    
    public void AddComment(string content, string userId)
    {
        var comment = new Comment(content, userId);
        _comments.Add(comment);
        AddDomainEvent(new CommentAdded(Id, comment.Id));
    }
}
```

### 2. Value Objects
Example of immutable value objects:

```csharp
public class TaskStatus : ValueObject
{
    public string Value { get; }
    
    private TaskStatus(string value)
    {
        Value = value;
    }
    
    public static TaskStatus ToDo = new TaskStatus("ToDo");
    public static TaskStatus InProgress = new TaskStatus("InProgress");
    public static TaskStatus Done = new TaskStatus("Done");
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

## Event Sourcing Implementation

### 1. Event Store
Redis-based event store implementation:

```csharp
public class RedisEventStoreService : IEventStoreService
{
    private readonly IDatabase _redisDb;
    
    public async Task SaveAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        var storedEvent = new StoredEvent
        {
            Id = Guid.NewGuid().ToString(),
            EntityId = @event.EntityId,
            EntityType = @event.EntityType,
            EventType = @event.GetType().Name,
            Data = JsonConvert.SerializeObject(@event),
            Timestamp = DateTime.UtcNow
        };
        
        await _redisDb.ListLeftPushAsync(
            $"events:{@event.EntityType}:{@event.EntityId}",
            JsonConvert.SerializeObject(storedEvent)
        );
    }
}
```

### 2. Event Replay
Capability to rebuild state from events:

```csharp
public class TaskEventRebuilder
{
    private readonly IEventStoreService _eventStore;
    
    public async Task<Task> RebuildTaskState(string taskId)
    {
        var events = await _eventStore.GetEventsAsync("Task", taskId);
        var task = new Task(); // Create empty state
        
        foreach (var @event in events.OrderBy(e => e.Timestamp))
        {
            task.Apply(@event); // Apply each event in sequence
        }
        
        return task;
    }
}
```

## Security Implementation

### 1. Authentication
JWT-based authentication with Identity Service:

```csharp
public class AuthenticationConfig
{
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Jwt:Authority"];
                options.Audience = configuration["Jwt:Audience"];
                options.RequireHttpsMetadata = false;
            });
    }
}
```

### 2. Authorization
Fine-grained permission control:

```csharp
[Authorize("task-write-access")]
public class UpdateTaskStatusEndpoint : EndpointBase
{
    [HttpPut("tasks/{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateTaskStatusRequest request)
    {
        if (!await _authorizationService.CanModifyTask(User, id))
            return Forbid();
            
        var command = new UpdateTaskStatusCommand(id, request.Status);
        await _mediator.Send(command);
        return Ok();
    }
}
```

## Monitoring and Telemetry

### 1. Distributed Tracing
OpenTelemetry integration:

```csharp
public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(config["OpenTelemetry:ServiceName"]))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddRedisInstrumentation()
                .AddMassTransitInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(config["OpenTelemetry:Endpoint"]);
                });
        });
    }
}
```

### 2. Metrics Collection
Key metrics monitoring:

```csharp
public class MetricsCollector
{
    private readonly Meter _meter;
    private readonly Counter<long> _taskCreatedCounter;
    private readonly Histogram<double> _taskCompletionTime;
    
    public MetricsCollector()
    {
        _meter = new Meter("TaskoMask.Tasks");
        _taskCreatedCounter = _meter.CreateCounter<long>("tasks_created_total");
        _taskCompletionTime = _meter.CreateHistogram<double>("task_completion_seconds");
    }
    
    public void RecordTaskCreated()
    {
        _taskCreatedCounter.Add(1);
    }
    
    public void RecordTaskCompletion(TimeSpan duration)
    {
        _taskCompletionTime.Record(duration.TotalSeconds);
    }
}
```
