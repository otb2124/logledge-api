namespace logledge_api.DTOs;

public class CreateBoardListDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateBoardListDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BoardListDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public List<TicketSummaryDto> Tickets { get; set; } = new();
}

public class TicketSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? PriorityId { get; set; }
    public string? AssigneeId { get; set; }
    public int Position { get; set; }
}

public class ReorderBoardListsDto
{
    public List<string> OrderedListIds { get; set; } = new();
}