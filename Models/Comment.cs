namespace logledge_api.Models;

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string TicketId { get; set; } = string.Empty;
    public Ticket Ticket { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty; // rich text, stored as-is

    public string? ReplyToCommentId { get; set; }
    public Comment? ReplyToComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}