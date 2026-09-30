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
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetProjects()
    {
        var projects = await _context.Projects
            .Where(p => p.Members.Any(m => m.UserId == CurrentUserId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                ListCount = p.Lists.Count
            })
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailDto>> GetProject(string id)
    {
        var project = await _context.Projects
            .Where(p => p.Id == id && p.Members.Any(m => m.UserId == CurrentUserId))
            .Select(p => new ProjectDetailDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                MemberIds = p.Members.Select(m => m.UserId).ToList(),
                Priorities = p.Priorities
                    .OrderBy(pr => pr.Order)
                    .Select(pr => new PriorityDto { Id = pr.Id, Title = pr.Title, Icon = pr.Icon, Order = pr.Order })
                    .ToList(),
                Labels = p.Labels
                    .Select(l => new LabelDto { Id = l.Id, Title = l.Title, Color = l.Color })
                    .ToList(),
                Lists = p.Lists
                    .OrderBy(l => l.Position)
                    .Select(l => new BoardListDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Description = l.Description,
                        Position = l.Position,
                        Tickets = l.Tickets
                            .OrderBy(t => t.Position)
                            .Select(t => new TicketSummaryDto
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Type = t.Type.ToString(),
                                PriorityId = t.PriorityId,
                                AssigneeId = t.AssigneeId,
                                Position = t.Position
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponseDto>> CreateProject(CreateProjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Project title is required." });

        var project = new Project
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        project.Members.Add(new ProjectMember { ProjectId = project.Id, UserId = CurrentUserId });

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, new ProjectResponseDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            ListCount = 0
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, UpdateProjectDto dto)
    {
        var project = await GetOwnedProject(id);
        if (project == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Project title is required." });

        project.Title = dto.Title.Trim();
        project.Description = dto.Description?.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var project = await GetOwnedProject(id);
        if (project == null) return NotFound();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/members/{userId}")]
    public async Task<IActionResult> AddMember(string id, string userId)
    {
        var project = await GetOwnedProject(id);
        if (project == null) return NotFound();

        var alreadyMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == id && m.UserId == userId);
        if (alreadyMember) return Conflict(new { message = "User is already a member." });

        _context.ProjectMembers.Add(new ProjectMember { ProjectId = id, UserId = userId });
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string id, string userId)
    {
        var project = await GetOwnedProject(id);
        if (project == null) return NotFound();

        var membership = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);
        if (membership == null) return NotFound();

        _context.ProjectMembers.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // No owner/admin role currently exists — every member can update/delete the
    // project and manage membership. Add a Role column on ProjectMember later
    // if you need an owner-only tier for destructive actions.
    private async Task<Project?> GetOwnedProject(string projectId) =>
        await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.Members.Any(m => m.UserId == CurrentUserId));
}