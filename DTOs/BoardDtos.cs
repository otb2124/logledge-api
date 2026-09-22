namespace logledge_api.DTOs;

public class CreateBoardDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateBoardDto
{
    public string Name { get; set; } = string.Empty;
}

public class BoardResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ListCount { get; set; }
}

public class BoardDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<BoardListDto> Lists { get; set; } = new();
}