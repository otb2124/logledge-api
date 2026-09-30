namespace logledge_api.Models;

public enum TicketLinkType { Blocks, IsBlockedBy, RelatesTo }

public class TicketLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TicketLinkType Type { get; set; }

    public string FromTicketId { get; set; } = string.Empty;
    public Ticket FromTicket { get; set; } = null!;

    public string ToTicketId { get; set; } = string.Empty;
    public Ticket ToTicket { get; set; } = null!;
}