using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using logledge_api.Data;
using logledge_api.DTOs;
using logledge_api.Models;

namespace logledge_api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/lists")]
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

    private async Task<Project?> GetMemberProject(string projectId) =>
        await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.Members.Any(m => m.UserId == CurrentUserId));

    [HttpPost]
    public async Task<ActionResult<BoardListDto>> CreateList(string projectId, CreateBoardListDto dto)
    {
        var project = await GetMemberProject(projectId);
        if (project == null) return NotFound(new { message = "Project not found." });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "List title is required." });

        var maxPosition = await _context.BoardLists
            .Where(l => l.ProjectId == projectId)
            .Select(l => (int?)l.Position)
            .MaxAsync() ?? -1;

        var list = new BoardList
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            ProjectId = projectId,
            Position = maxPosition + 1
        };

        _context.BoardLists.Add(list);
        await _context.SaveChangesAsync();

        return Ok(new BoardListDto { Id = list.Id, Title = list.Title, Description = list.Description, Position = list.Position });
    }

    [HttpPut("{listId}")]
    public async Task<IActionResult> UpdateList(string projectId, string listId, UpdateBoardListDto dto)
    {
        var project = await GetMemberProject(projectId);
        if (project == null) return NotFound();

        var list = await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == listId && l.ProjectId == projectId);
        if (list == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "List title is required." });

        list.Title = dto.Title.Trim();
        list.Description = dto.Description?.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("reorder")]
    public async Task<IActionResult> ReorderLists(string projectId, ReorderBoardListsDto dto)
    {
        var project = await GetMemberProject(projectId);
        if (project == null) return NotFound();

        var lists = await _context.BoardLists
            .Where(l => l.ProjectId == projectId && dto.OrderedListIds.Contains(l.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedListIds.Count; i++)
        {
            var list = lists.FirstOrDefault(l => l.Id == dto.OrderedListIds[i]);
            if (list != null) list.Position = i;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteList(string projectId, string listId)
    {
        var project = await GetMemberProject(projectId);
        if (project == null) return NotFound();

        var list = await _context.BoardLists.FirstOrDefaultAsync(l => l.Id == listId && l.ProjectId == projectId);
        if (list == null) return NotFound();

        _context.BoardLists.Remove(list);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}