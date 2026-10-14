using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFlow.Domain.Entities;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Api.Controllers;

public class CategoriesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.JobCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Slug, c.Description, c.IconName,
                JobCount = c.Jobs.Count(j => j.Status == Domain.Enums.JobStatus.Published) })
            .ToListAsync();
        return OkResponse(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var category = await _context.JobCategories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");
        return OkResponse(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var slug = request.Name.ToLower().Replace(" ", "-");
        if (await _context.JobCategories.AnyAsync(c => c.Slug == slug))
            return BadRequest("Category with this name already exists.");

        var category = new JobCategory
        {
            Name = request.Name, Slug = slug,
            Description = request.Description, IconName = request.IconName
        };
        _context.JobCategories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedResponse(category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var category = await _context.JobCategories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");
        category.Name = request.Name;
        category.Description = request.Description;
        category.IconName = request.IconName;
        await _context.SaveChangesAsync();
        return OkResponse(category);
    }
}

public record CreateCategoryRequest(string Name, string? Description, string? IconName);
