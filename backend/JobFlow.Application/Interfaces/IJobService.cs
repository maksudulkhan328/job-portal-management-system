using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Jobs;

namespace JobFlow.Application.Interfaces;

public interface IJobService
{
    Task<PagedResult<JobSummaryDto>> GetJobsAsync(JobQueryParams query);
    Task<JobDetailDto> GetJobByIdAsync(Guid id, Guid? currentUserId = null);
    Task<JobDetailDto> CreateJobAsync(CreateJobRequest request, Guid employerId);
    Task<JobDetailDto> UpdateJobAsync(Guid id, UpdateJobRequest request, Guid employerId);
    Task DeleteJobAsync(Guid id, Guid employerId);
    Task<JobDetailDto> SubmitForApprovalAsync(Guid id, Guid employerId);
    Task<JobDetailDto> PublishJobAsync(Guid id, Guid adminId);
    Task<JobDetailDto> RejectJobAsync(Guid id, Guid adminId, string reason);
    Task<JobDetailDto> CloseJobAsync(Guid id, Guid employerId);
    Task<PagedResult<JobSummaryDto>> GetMyJobsAsync(Guid employerId, JobQueryParams query);
    Task<PagedResult<JobSummaryDto>> GetAllJobsAdminAsync(JobQueryParams query);
}
