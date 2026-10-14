using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Auth;
using JobFlow.Domain.Enums;

namespace JobFlow.Application.Interfaces;

public record DashboardStats(
    int TotalUsers,
    int TotalEmployers,
    int TotalJobSeekers,
    int TotalJobs,
    int ActiveJobs,
    int TotalApplications,
    int PendingApprovalJobs,
    int TotalCompanies
);

public record AdminUserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search, UserRole? role);
    Task DeactivateUserAsync(Guid userId);
    Task ActivateUserAsync(Guid userId);
}
