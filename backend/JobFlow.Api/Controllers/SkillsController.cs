using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFlow.Domain.Entities;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Api.Controllers;

public class SkillsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public SkillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSkills([FromQuery] string? search)
    {
        var q = _context.Skills.Where(s => s.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => s.Name.Contains(search));
        var skills = await q.OrderBy(s => s.Name).Select(s => new { s.Id, s.Name, s.Slug }).ToListAsync();
        return OkResponse(skills);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request)
    {
        var slug = request.Name.ToLower().Replace(" ", "-");
        if (await _context.Skills.AnyAsync(s => s.Slug == slug))
            return BadRequest("Skill already exists.");

        var skill = new Skill { Name = request.Name, Slug = slug };
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return CreatedResponse(new { skill.Id, skill.Name, skill.Slug });
    }
}

public record CreateSkillRequest(string Name);
