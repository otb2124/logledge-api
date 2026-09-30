using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using logledge_api.Data;
using logledge_api.DTOs;
using logledge_api.Models;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/lists/{listId}/tickets")]
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

    private async Task<BoardList?> GetMemberList(string projectId, string listId) =>
        await _context.BoardLists
            .Include(l => l.Project)
            .FirstOrDefaultAsync(l => l.Id == listId && l.ProjectId == projectId
                && l.Project.Members.Any(m => m.UserId == CurrentUserId));

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(string projectId, string listId, string ticketId)
    {
        var list = await GetMemberList(projectId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.ListId == listId);
        if (ticket == null) return NotFound();

        return Ok(ToDetailDto(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketSummaryDto>> CreateTicket(string projectId, string listId, CreateTicketDto dto)
    {
        var list = await GetMemberList(projectId, listId);
        if (list == null) return NotFound(new { message = "List not found." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Ticket title is required." });

        if (!Enum.TryParse<TicketType>(dto.Type, ignoreCase: true, out var type))
            return BadRequest(new { message = "Invalid ticket type." });

        if (dto.PriorityId != null)
        {
            var priorityExists = await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId && p.ProjectId == projectId);
            if (!priorityExists) return BadRequest(new { message = "Priority not found in this project." });
        }

        if (dto.ParentId != null)
        {
            var parentExists = await _context.Tickets.AnyAsync(t => t.Id == dto.ParentId && t.ProjectId == projectId);
            if (!parentExists) return BadRequest(new { message = "Parent ticket not found in this project." });
        }

        var maxPosition = await _context.Tickets
            .Where(t => t.ListId == listId)
            .Select(t => (int?)t.Position)
            .MaxAsync() ?? -1;

        var ticket = new Ticket
        {
            Title = dto.Title.Trim(),
            Type = type,
            ProjectId = projectId,
            ListId = listId,
            PriorityId = dto.PriorityId,
            ParentId = dto.ParentId,
            ReporterId = CurrentUserId,
            Position = maxPosition + 1
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return Ok(new TicketSummaryDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Type = ticket.Type.ToString(),
            PriorityId = ticket.PriorityId,
            AssigneeId = ticket.AssigneeId,
            Position = ticket.Position
        });
    }

    [HttpPut("{ticketId}")]
    public async Task<IActionResult> UpdateTicket(string projectId, string listId, string ticketId, UpdateTicketDto dto)
    {
        var list = await GetMemberList(projectId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.ListId == listId);
        if (ticket == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Ticket title is required." });

        if (!Enum.TryParse<TicketType>(dto.Type, ignoreCase: true, out var type))
            return BadRequest(new { message = "Invalid ticket type." });

        if (dto.ParentId == ticketId)
            return BadRequest(new { message = "A ticket cannot be its own parent." });

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description;
        ticket.Type = type;
        ticket.AssigneeId = dto.AssigneeId;
        ticket.ReporterId = dto.ReporterId;
        ticket.PriorityId = dto.PriorityId;
        ticket.ParentId = dto.ParentId;
        ticket.StoryPoints = dto.StoryPoints;
        ticket.StartDate = dto.StartDate;
        ticket.DueDate = dto.DueDate;

        if (dto.LabelIds != null)
        {
            _context.TicketLabels.RemoveRange(ticket.Labels);
            foreach (var labelId in dto.LabelIds.Distinct())
            {
                ticket.Labels.Add(new TicketLabel { TicketId = ticket.Id, LabelId = labelId });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("reorder")]
    public async Task<IActionResult> ReorderTickets(string projectId, string listId, ReorderTicketsDto dto)
    {
        var list = await GetMemberList(projectId, listId);
        if (list == null) return NotFound();

        var tickets = await _context.Tickets
            .Where(t => t.ListId == listId && dto.OrderedTicketIds.Contains(t.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedTicketIds.Count; i++)
        {
            var ticket = tickets.FirstOrDefault(t => t.Id == dto.OrderedTicketIds[i]);
            if (ticket != null) ticket.Position = i;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{ticketId}/move")]
    public async Task<IActionResult> MoveTicket(string projectId, string listId, string ticketId, MoveTicketDto dto)
    {
        var sourceList = await GetMemberList(projectId, listId);
        if (sourceList == null) return NotFound(new { message = "Source list not found." });

        var targetList = await GetMemberList(projectId, dto.TargetListId);
        if (targetList == null) return NotFound(new { message = "Target list not found." });

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.ListId == listId);
        if (ticket == null) return NotFound(new { message = "Ticket not found." });

        var sourceSiblings = await _context.Tickets
            .Where(t => t.ListId == listId && t.Position > ticket.Position)
            .ToListAsync();
        foreach (var sibling in sourceSiblings) sibling.Position -= 1;

        var targetSiblings = await _context.Tickets
            .Where(t => t.ListId == dto.TargetListId && t.Position >= dto.Position)
            .ToListAsync();
        foreach (var sibling in targetSiblings) sibling.Position += 1;

        ticket.ListId = dto.TargetListId;
        ticket.Position = dto.Position;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{ticketId}")]
    public async Task<IActionResult> DeleteTicket(string projectId, string listId, string ticketId)
    {
        var list = await GetMemberList(projectId, listId);
        if (list == null) return NotFound();

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.ListId == listId);
        if (ticket == null) return NotFound();

        var hasChildren = await _context.Tickets.AnyAsync(t => t.ParentId == ticketId);
        if (hasChildren)
            return Conflict(new { message = "Cannot delete a ticket that has child tickets. Reassign or delete them first." });

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TicketDetailDto ToDetailDto(Ticket ticket) => new()
    {
        Id = ticket.Id,
        ProjectId = ticket.ProjectId,
        ListId = ticket.ListId,
        ParentId = ticket.ParentId,
        Type = ticket.Type.ToString(),
        Title = ticket.Title,
        Description = ticket.Description,
        AssigneeId = ticket.AssigneeId,
        ReporterId = ticket.ReporterId,
        PriorityId = ticket.PriorityId,
        LabelIds = ticket.Labels.Select(l => l.LabelId).ToList(),
        StoryPoints = ticket.StoryPoints,
        StartDate = ticket.StartDate,
        DueDate = ticket.DueDate,
        Position = ticket.Position
    };
}