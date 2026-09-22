using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using logledge_api.Data;
using logledge_api.DTOs;
using logledge_api.Models;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BoardsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    // GET: api/boards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BoardResponseDto>>> GetBoards()
    {
        var boards = await _context.Boards
            .Where(b => b.OwnerId == CurrentUserId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BoardResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                CreatedAt = b.CreatedAt,
                ListCount = b.Lists.Count
            })
            .ToListAsync();

        return Ok(boards);
    }

    // GET: api/boards/5  (full board with lists + tickets, for the board view)
    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDetailDto>> GetBoard(int id)
    {
        var board = await _context.Boards
            .Where(b => b.Id == id && b.OwnerId == CurrentUserId)
            .Select(b => new BoardDetailDto
            {
                Id = b.Id,
                Name = b.Name,
                CreatedAt = b.CreatedAt,
                Lists = b.Lists
                    .OrderBy(l => l.Position)
                    .Select(l => new BoardListDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Position = l.Position,
                        Tickets = l.Tickets
                            .OrderBy(t => t.Position)
                            .Select(t => new TicketSummaryDto
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Priority = t.Priority.ToString(),
                                Position = t.Position
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (board == null) return NotFound();
        return Ok(board);
    }

    // POST: api/boards
    [HttpPost]
    public async Task<ActionResult<BoardResponseDto>> CreateBoard(CreateBoardDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Board name is required." });

        var board = new Board
        {
            Name = dto.Name.Trim(),
            OwnerId = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, new BoardResponseDto
        {
            Id = board.Id,
            Name = board.Name,
            CreatedAt = board.CreatedAt,
            ListCount = 0
        });
    }

    // PUT: api/boards/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(int id, UpdateBoardDto dto)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == CurrentUserId);
        if (board == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Board name is required." });

        board.Name = dto.Name.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/boards/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == CurrentUserId);
        if (board == null) return NotFound();

        _context.Boards.Remove(board); // cascades to Lists -> Tickets per the DbContext config
        await _context.SaveChangesAsync();

        return NoContent();
    }
}