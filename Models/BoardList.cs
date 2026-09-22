namespace logledge_api.Models;

public class BoardList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; } // order of this list within the board

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}