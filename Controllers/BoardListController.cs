using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using logledge_api.Data;
using logledge_api.DTOs;
using logledge_api.Models;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/boards/{boardId}/lists")]
[Authorize]
public class BoardListsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BoardListsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    private async Task<Board?> GetOwnedBoard(int boardId) =>
        await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId && b.OwnerId == CurrentUserId);

    // POST: api/boards/5/lists
    [HttpPost]
    public async Task<ActionResult<BoardListDto>> CreateList(int boardId, CreateBoardListDto dto)
    {
        var board = await GetOwnedBoard(boardId);
        if (board == null) return NotFound(new { message = "Board not found." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "List name is required." });

        var maxPosition = await _context.BoardLists
            .Where(l => l.BoardId == boardId)
            .Select(l => (int?)l.Position)
            .MaxAsync() ?? -1;

        var list = new BoardList
        {
            Name = dto.Name.Trim(),
            BoardId = boardId,
            Position = maxPosition + 1
        };

        _context.BoardLists.Add(list);
        await _context.SaveChangesAsync();

        return Ok(new BoardListDto { Id = list.Id, Name = list.Name, Position = list.Position });
    }

    // PUT: api/boards/5/lists/3
    [HttpPut("{listId}")]
    public async Task<IActionResult> UpdateList(int boardId, int listId, UpdateBoardListDto dto)
    {
        var board = await GetOwnedBoard(boardId);
        if (board == null) return NotFound();

        var list = await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == listId && l.BoardId == boardId);
        if (list == null) return NotFound();

        list.Name = dto.Name.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/boards/5/lists/3/position
    [HttpPatch("{listId}/position")]
    public async Task<IActionResult> ReorderList(int boardId, int listId, ReorderBoardListDto dto)
    {
        var board = await GetOwnedBoard(boardId);
        if (board == null) return NotFound();

        var list = await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == listId && l.BoardId == boardId);
        if (list == null) return NotFound();

        list.Position = dto.Position;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/boards/5/lists/3
    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteList(int boardId, int listId)
    {
        var board = await GetOwnedBoard(boardId);
        if (board == null) return NotFound();

        var list = await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == listId && l.BoardId == boardId);
        if (list == null) return NotFound();

        _context.BoardLists.Remove(list); // cascades to its Tickets
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/boards/5/lists/reorder
    [HttpPatch("reorder")]
    public async Task<IActionResult> ReorderLists(int boardId, ReorderBoardListsDto dto)
    {
        var board = await GetOwnedBoard(boardId);
        if (board == null) return NotFound();

        var lists = await _context.BoardLists
            .Where(l => l.BoardId == boardId && dto.OrderedListIds.Contains(l.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedListIds.Count; i++)
        {
            var list = lists.FirstOrDefault(l => l.Id == dto.OrderedListIds[i]);
            if (list != null) list.Position = i;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}