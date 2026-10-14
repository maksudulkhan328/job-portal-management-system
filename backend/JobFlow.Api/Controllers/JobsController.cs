using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.DTOs.Jobs;
using JobFlow.Application.Interfaces;

namespace JobFlow.Api.Controllers;

public class JobsController : BaseController
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] JobQueryParams query)
    {
        var result = await _jobService.GetJobsAsync(query);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? CurrentUserId : null;
        var result = await _jobService.GetJobByIdAsync(id, currentUserId);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
    {
        var result = await _jobService.CreateJobAsync(request, CurrentUserId);
        return CreatedResponse(result, "Job created successfully");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateJob(Guid id, [FromBody] UpdateJobRequest request)
    {
        var result = await _jobService.UpdateJobAsync(id, request, CurrentUserId);
        return OkResponse(result, "Job updated successfully");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        await _jobService.DeleteJobAsync(id, CurrentUserId);
        return OkResponse<object>(null!, "Job deleted successfully");
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> SubmitForApproval(Guid id)
    {
        var result = await _jobService.SubmitForApprovalAsync(id, CurrentUserId);
        return OkResponse(result, "Job submitted for approval");
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishJob(Guid id)
    {
        var result = await _jobService.PublishJobAsync(id, CurrentUserId);
        return OkResponse(result, "Job published successfully");
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectJob(Guid id, [FromBody] RejectJobRequest request)
    {
        var result = await _jobService.RejectJobAsync(id, CurrentUserId, request.Reason);
        return OkResponse(result, "Job rejected");
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> CloseJob(Guid id)
    {
        var result = await _jobService.CloseJobAsync(id, CurrentUserId);
        return OkResponse(result, "Job closed successfully");
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMyJobs([FromQuery] JobQueryParams query)
    {
        var result = await _jobService.GetMyJobsAsync(CurrentUserId, query);
        return OkResponse(result);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllJobsAdmin([FromQuery] JobQueryParams query)
    {
        var result = await _jobService.GetAllJobsAdminAsync(query);
        return OkResponse(result);
    }
}

public record RejectJobRequest(string Reason);
