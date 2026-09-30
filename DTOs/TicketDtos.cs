namespace logledge_api.DTOs;

public class TicketDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssigneeId { get; set; }
    public string? ReporterId { get; set; }
    public string? PriorityId { get; set; }
    public List<string> LabelIds { get; set; } = new();
    public int? StoryPoints { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int Position { get; set; }
}

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Task";
    public string? PriorityId { get; set; }
    public string? ParentId { get; set; }
}

public class UpdateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Task";
    public string? AssigneeId { get; set; }
    public string? ReporterId { get; set; }
    public string? PriorityId { get; set; }
    public string? ParentId { get; set; }
    public List<string>? LabelIds { get; set; }
    public int? StoryPoints { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ReorderTicketsDto
{
    public List<string> OrderedTicketIds { get; set; } = new();
}

public class MoveTicketDto
{
    public string TargetListId { get; set; } = string.Empty;
    public int Position { get; set; }
}