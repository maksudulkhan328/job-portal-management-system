using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Applications;
using JobFlow.Domain.Enums;

namespace JobFlow.Application.Interfaces;

public interface IApplicationService
{
    Task<ApplicationDto> ApplyAsync(Guid jobId, Guid applicantId, ApplyForJobRequest request, string? cvFileName, string? cvFileUrl);
    Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(Guid applicantId, int page, int pageSize);
    Task<PagedResult<ApplicationDetailDto>> GetJobApplicationsAsync(Guid jobId, Guid employerId, int page, int pageSize);
    Task<ApplicationDetailDto> GetApplicationByIdAsync(Guid id, Guid userId);
    Task<ApplicationDetailDto> UpdateStatusAsync(Guid id, ApplicationStatus status, string? notes, Guid employerId);
    Task WithdrawApplicationAsync(Guid id, Guid applicantId, string? reason);
    Task<ApplicationDetailDto> AddRecruiterNotesAsync(Guid id, string notes, Guid employerId);
}
