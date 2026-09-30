namespace logledge_api.Models;

public class Priority
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Order { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public Project Project { get; set; } = null!;
}