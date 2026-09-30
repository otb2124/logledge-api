namespace logledge_api.Models;

public enum TicketType { Story, Bug, Task, Epic, Subtask }

public class Ticket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ProjectId { get; set; } = string.Empty;
    public Project Project { get; set; } = null!;

    public string ListId { get; set; } = string.Empty;
    public BoardList List { get; set; } = null!;

    public string? ParentId { get; set; }
    public Ticket? Parent { get; set; }
    public ICollection<Ticket> Children { get; set; } = new List<Ticket>();

    public TicketType Type { get; set; } = TicketType.Task;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }

    public string? ReporterId { get; set; }
    public ApplicationUser? Reporter { get; set; }

    public string? PriorityId { get; set; }
    public Priority? Priority { get; set; }

    public int? StoryPoints { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TicketLabel> Labels { get; set; } = new List<TicketLabel>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<TicketLink> OutgoingLinks { get; set; } = new List<TicketLink>();
    public ICollection<TicketLink> IncomingLinks { get; set; } = new List<TicketLink>();
}