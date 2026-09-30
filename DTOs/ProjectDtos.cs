namespace logledge_api.DTOs;

public class CreateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ProjectResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ListCount { get; set; }
}

public class ProjectDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> MemberIds { get; set; } = new();
    public List<PriorityDto> Priorities { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public List<BoardListDto> Lists { get; set; } = new();
}

public class PriorityDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Order { get; set; }
}

public class LabelDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Color { get; set; }
}