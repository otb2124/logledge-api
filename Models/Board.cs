namespace logledge_api.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty; // FK to ApplicationUser
    public ApplicationUser Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();
}