using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFlow.Application.Common;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalUsers = await _context.Users.IgnoreQueryFilters().CountAsync(u => !u.IsDeleted);
        var totalEmployers = await _context.Users.IgnoreQueryFilters().CountAsync(u => u.Role == UserRole.Employer && !u.IsDeleted);
        var totalJobSeekers = await _context.Users.IgnoreQueryFilters().CountAsync(u => u.Role == UserRole.JobSeeker && !u.IsDeleted);
        var totalJobs = await _context.Jobs.IgnoreQueryFilters().CountAsync(j => !j.IsDeleted);
        var activeJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Published);
        var totalApplications = await _context.JobApplications.CountAsync();
        var pendingApprovalJobs = await _context.Jobs.IgnoreQueryFilters().CountAsync(j => j.Status == JobStatus.PendingApproval);
        var totalCompanies = await _context.Companies.IgnoreQueryFilters().CountAsync(c => !c.IsDeleted);

        return new DashboardStats(totalUsers, totalEmployers, totalJobSeekers, totalJobs, activeJobs, totalApplications, pendingApprovalJobs, totalCompanies);
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search, UserRole? role)
    {
        var q = _context.Users
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(u => u.Email!.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search));

        if (role.HasValue)
            q = q.Where(u => u.Role == role);

        q = q.OrderByDescending(u => u.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<AdminUserDto>
        {
            Items = items.Select(u => new AdminUserDto(u.Id, u.FullName, u.Email!, u.Role, u.IsActive, u.CreatedAt, u.LastLoginAt)).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");
        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Cannot deactivate an admin account.");
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }
}
