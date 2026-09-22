namespace logledge_api.DTOs;

public class CreateBoardListDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateBoardListDto
{
    public string Name { get; set; } = string.Empty;
}

public class ReorderBoardListDto
{
    public int Position { get; set; }
}

public class BoardListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public List<TicketSummaryDto> Tickets { get; set; } = new();
}

public class TicketSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Position { get; set; }
}

public class ReorderBoardListsDto
{
    public List<int> OrderedListIds { get; set; } = new();
}