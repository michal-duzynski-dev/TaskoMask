# Architecture Diagrams

## System Overview
```
+----------------+     +-----------------+
|                |     |                 |
|   Client Apps  +---->|   API Gateway   |
|                |     |                 |
+----------------+     +--------+--------+
                               |
         +-------------------+ | +-------------------+
         |                   | | |                   |
         v                   v v v                   v
+----------------+  +----------------+  +----------------+
|                |  |                |  |                |
| Identity API   |  |   Tasks API    |  |   Boards API   |
|                |  |                |  |                |
+----------------+  +----------------+  +----------------+
         |                |     |              |
         |                |     |              |
         v                v     v              v
+----------------+  +----------+ +------------+ +-----------+
|                |  |          | |            | |           |
|  User Store    |  |  Redis   | |  RabbitMQ  | | MongoDB  |
|                |  |          | |            | |           |
+----------------+  +----------+ +------------+ +-----------+
```

## Event Sourcing Flow
```
Command --> Aggregate Root --> Domain Event
     |            |               |
     v            v               v
Handler -----> Event Store --> Event Publisher
                                  |
                                  v
                            Event Consumer
                                  |
                                  v
                            Read Model
```

## Service Architecture (Example: Tasks Service)
```
Tasks.Write.Api/
|
+-- Domain/
|   |-- Task.cs (Aggregate Root)
|   |-- Comment.cs (Entity)
|   `-- TaskStatus.cs (Value Object)
|
+-- Application/
|   |-- Commands/
|   |   |-- CreateTask/
|   |   |   |-- CreateTaskCommand.cs
|   |   |   `-- CreateTaskCommandHandler.cs
|   |   `-- UpdateStatus/
|   |       |-- UpdateTaskStatusCommand.cs
|   |       `-- UpdateTaskStatusCommandHandler.cs
|   `-- Events/
|       |-- TaskCreated.cs
|       `-- TaskStatusUpdated.cs
|
+-- Infrastructure/
|   |-- Persistence/
|   |   |-- TaskRepository.cs
|   |   `-- TaskContext.cs
|   `-- EventStore/
|       |-- RedisEventStore.cs
|       `-- EventPublisher.cs
|
`-- API/
    |-- Controllers/
    |   `-- TasksController.cs
    `-- DTOs/
        |-- CreateTaskRequest.cs
        `-- TaskResponse.cs
```

## Data Flow Diagram
```
+-------------+    Command     +-------------+
|             |-------------->|             |
|   Client    |               | Write API   |
|             |    Response   |             |
|             |<--------------|             |
+-------------+               +-------------+
                                    |
                              Domain Event
                                    |
                                    v
+-------------+               +-------------+
|             |    Query     |             |
|   Client    |------------->|  Read API   |
|             |   Response   |             |
|             |<-------------|             |
+-------------+               +-------------+
```

## Authentication Flow
```
+--------+          +---------+         +-------------+
|        |  1. Auth |         | 2. JWT  |             |
| Client |--------->|Identity |-------->| Other APIs  |
|        |          |   API   |         |             |
+--------+          +---------+         +-------------+
    |                                         |
    |              3. Request                 |
    |---------------------------------------->|
    |                                         |
    |           4. Protected Resource         |
    |<----------------------------------------|
```

## Event Store Structure (Redis)
```
Key Pattern: events:{entityType}:{entityId}
Example: events:task:123

[Newest Event] --> [Event N-1] --> ... --> [Event 1] --> [Event 0]

Event Structure:
{
    "id": "guid",
    "entityId": "123",
    "entityType": "task",
    "eventType": "TaskCreated",
    "data": { ... },
    "timestamp": "2025-01-24T18:47:10Z"
}
```

## Message Queue Structure (RabbitMQ)
```
Exchange: taskomask.events
Type: topic

Queues:
+------------------------+    +-----------------------+
| tasks.events           |    | boards.events        |
+------------------------+    +-----------------------+
         ^                             ^
         |                             |
    task.* events               board.* events
         |                             |
+------------------------+    +-----------------------+
| TaskEventConsumer      |    | BoardEventConsumer   |
+------------------------+    +-----------------------+
```

## Deployment Architecture
```
                   [Load Balancer]
                         |
                   [API Gateway]
                         |
        +----------------+----------------+
        |                |               |
[Identity API]    [Tasks API]     [Boards API]
        |                |               |
        v                v               v
   [User Store]     [Event Store]   [Read Store]
                         ^
                         |
                  [Message Queue]
```

## Monitoring Setup
```
Services
   |
   +-> Metrics
   |     |
   |     +-> Prometheus
   |     |     |
   |     |     +-> Grafana
   |     |
   |     +-> Application Insights
   |
   +-> Logs
   |     |
   |     +-> Elasticsearch
   |     |     |
   |     |     +-> Kibana
   |     |
   |     +-> Application Insights
   |
   +-> Traces
         |
         +-> Jaeger
         |
         +-> Application Insights
```
