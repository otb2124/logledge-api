namespace logledge_api.Models;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Priority> Priorities { get; set; } = new List<Priority>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

// Join entity for Project <-> ApplicationUser, replacing the old single Board.OwnerId.
public class ProjectMember
{
    public string ProjectId { get; set; } = string.Empty;
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}