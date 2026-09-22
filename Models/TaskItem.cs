using System.ComponentModel.DataAnnotations;

namespace logledge_api.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}