namespace TaskHub.Models;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public enum TaskStatus
{
    New,
    InProgress,
    Done
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime Deadline { get; set; } = DateTime.Now.AddDays(1);
    public TaskStatus Status { get; set; } = TaskStatus.New;

    public bool IsOverdue => Status != TaskStatus.Done && Deadline < DateTime.Now;
}
