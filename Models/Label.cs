namespace logledge_api.Models;

public class Label
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Color { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public Project Project { get; set; } = null!;

    public ICollection<TicketLabel> Tickets { get; set; } = new List<TicketLabel>();
}

// Join entity for Ticket <-> Label many-to-many.
public class TicketLabel
{
    public string TicketId { get; set; } = string.Empty;
    public Ticket Ticket { get; set; } = null!;

    public string LabelId { get; set; } = string.Empty;
    public Label Label { get; set; } = null!;
}