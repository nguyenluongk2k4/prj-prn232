# Exam State Diagram

```mermaid
stateDiagram-v2
    [*] --> Planned
    Planned --> InProgress: start time reached
    InProgress --> Completed: end time reached
    Planned --> Cancelled: cancel
    InProgress --> Cancelled: emergency stop
```
