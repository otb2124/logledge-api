namespace logledge_api.DTOs;

public class TicketDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int Position { get; set; }
    public int BoardListId { get; set; }
}

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class UpdateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class ReorderTicketsDto
{
    public List<int> OrderedTicketIds { get; set; } = new();
}

public class MoveTicketDto
{
    public int TargetListId { get; set; }
    public int Position { get; set; }
}