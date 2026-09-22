using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using logledge_api.Data;
using logledge_api.DTOs;
using logledge_api.Models;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/boards/{boardId}/lists/{listId}/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    private async Task<BoardList?> GetOwnedList(int boardId, int listId) =>
        await _context.BoardLists
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == listId && l.BoardId == boardId && l.Board.OwnerId == CurrentUserId);

    // GET: api/boards/5/lists/3/tickets/7
    [HttpGet("{ticketId}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(int boardId, int listId, int ticketId)
    {
        var list = await GetOwnedList(boardId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BoardListId == listId);
        if (ticket == null) return NotFound();

        return Ok(new TicketDetailDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            Position = ticket.Position,
            BoardListId = ticket.BoardListId
        });
    }

    // POST: api/boards/5/lists/3/tickets
    [HttpPost]
    public async Task<ActionResult<TicketSummaryDto>> CreateTicket(int boardId, int listId, CreateTicketDto dto)
    {
        var list = await GetOwnedList(boardId, listId);
        if (list == null) return NotFound(new { message = "List not found." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Ticket title is required." });

        var maxPosition = await _context.Tickets
            .Where(t => t.BoardListId == listId)
            .Select(t => (int?)t.Position)
            .MaxAsync() ?? -1;

        if (!Enum.TryParse<TicketPriority>(dto.Priority, ignoreCase: true, out var priority))
            return BadRequest(new { message = "Invalid priority value." });

        var ticket = new Ticket
        {
            Title = dto.Title.Trim(),
            Priority = priority,
            BoardListId = listId,
            Position = maxPosition + 1
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(new TicketSummaryDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Priority = ticket.Priority.ToString(),
            Position = ticket.Position
        });
    }

    // PUT: api/boards/5/lists/3/tickets/7
    [HttpPut("{ticketId}")]
    public async Task<IActionResult> UpdateTicket(int boardId, int listId, int ticketId, UpdateTicketDto dto)
    {
        var list = await GetOwnedList(boardId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BoardListId == listId);
        if (ticket == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Ticket title is required." });

        if (!Enum.TryParse<TicketPriority>(dto.Priority, ignoreCase: true, out var priority))
            return BadRequest(new { message = "Invalid priority value." });

        ticket.Title = dto.Title.Trim();
        ticket.Priority = priority;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/boards/5/lists/3/tickets/7/reorder
    // Same-list reorder: client sends the full ordered id list for this list after the move.
    [HttpPatch("reorder")]
    public async Task<IActionResult> ReorderTickets(int boardId, int listId, ReorderTicketsDto dto)
    {
        var list = await GetOwnedList(boardId, listId);
        if (list == null) return NotFound();

        var tickets = await _context.Tickets
            .Where(t => t.BoardListId == listId && dto.OrderedTicketIds.Contains(t.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedTicketIds.Count; i++)
        {
            var ticket = tickets.FirstOrDefault(t => t.Id == dto.OrderedTicketIds[i]);
            if (ticket != null) ticket.Position = i;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PATCH: api/boards/5/lists/3/tickets/7/move
    // Cross-list move: ticket leaves this list's route context and lands in targetListId.
    [HttpPatch("{ticketId}/move")]
    public async Task<IActionResult> MoveTicket(int boardId, int listId, int ticketId, MoveTicketDto dto)
    {
        var sourceList = await GetOwnedList(boardId, listId);
        if (sourceList == null) return NotFound(new { message = "Source list not found." });

        var targetList = await GetOwnedList(boardId, dto.TargetListId);
        if (targetList == null) return NotFound(new { message = "Target list not found." });

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BoardListId == listId);
        if (ticket == null) return NotFound(new { message = "Ticket not found." });

        // Close the gap in the source list
        var sourceSiblings = await _context.Tickets
            .Where(t => t.BoardListId == listId && t.Position > ticket.Position)
            .ToListAsync();
        foreach (var sibling in sourceSiblings) sibling.Position -= 1;

        // Make room in the target list at dto.Position
        var targetSiblings = await _context.Tickets
            .Where(t => t.BoardListId == dto.TargetListId && t.Position >= dto.Position)
            .ToListAsync();
        foreach (var sibling in targetSiblings) sibling.Position += 1;

        ticket.BoardListId = dto.TargetListId;
        ticket.Position = dto.Position;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/boards/5/lists/3/tickets/7
    [HttpDelete("{ticketId}")]
    public async Task<IActionResult> DeleteTicket(int boardId, int listId, int ticketId)
    {
        var list = await GetOwnedList(boardId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BoardListId == listId);
        if (ticket == null) return NotFound();

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}