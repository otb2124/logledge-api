namespace logledge_api.Models;

public enum TicketPriority { Low, Medium, High }

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public int Position { get; set; } // order within the list
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    public int BoardListId { get; set; }
    public BoardList BoardList { get; set; } = null!;

    public string? AssigneeId { get; set; } // FK to ApplicationUser, nullable
    public ApplicationUser? Assignee { get; set; }
}