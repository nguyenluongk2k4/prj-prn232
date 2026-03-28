# Attendance State Diagram

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Present: on-time check-in
    Pending --> Late: late check-in
    Pending --> Absent: no show
    Absent --> Excused: approve absence
    Present --> Late: adjust if needed
    Late --> Present: adjust if needed
```
