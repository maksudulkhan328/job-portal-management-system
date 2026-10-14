using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Enums;

namespace JobFlow.Api.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
    private readonly IJobService _jobService;
    private readonly IJobService _job;

    public AdminController(IAdminService adminService, IJobService jobService)
    {
        _adminService = adminService;
        _jobService = jobService;
        _job = jobService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return OkResponse(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null)
    {
        var result = await _adminService.GetUsersAsync(page, pageSize, search, role);
        return OkResponse(result);
    }

    [HttpPatch("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        await _adminService.DeactivateUserAsync(userId);
        return OkResponse<object>(null!, "User deactivated");
    }

    [HttpPatch("users/{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        await _adminService.ActivateUserAsync(userId);
        return OkResponse<object>(null!, "User activated");
    }
}
