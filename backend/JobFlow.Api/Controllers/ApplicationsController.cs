using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.DTOs.Applications;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Enums;

namespace JobFlow.Api.Controllers;

public class ApplicationsController : BaseController
{
    private readonly IApplicationService _applicationService;
    private readonly IFileService _fileService;

    public ApplicationsController(IApplicationService applicationService, IFileService fileService)
    {
        _applicationService = applicationService;
        _fileService = fileService;
    }

    [HttpPost("jobs/{jobId:guid}/apply")]
    [Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> Apply(Guid jobId, [FromForm] ApplyForJobRequest request, IFormFile? cvFile)
    {
        string? cvFileName = null, cvFileUrl = null;

        if (cvFile != null)
        {
            (cvFileName, cvFileUrl) = await _fileService.SaveFileAsync(
                cvFile.OpenReadStream(), cvFile.FileName, "cvs",
                10 * 1024 * 1024, [".pdf", ".doc", ".docx"]);
        }

        var result = await _applicationService.ApplyAsync(jobId, CurrentUserId, request, cvFileName, cvFileUrl);
        return CreatedResponse(result, "Application submitted successfully");
    }

    [HttpGet("my")]
    [Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> GetMyApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _applicationService.GetMyApplicationsAsync(CurrentUserId, page, pageSize);
        return OkResponse(result);
    }

    [HttpGet("jobs/{jobId:guid}/applications")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetJobApplications(Guid jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _applicationService.GetJobApplicationsAsync(jobId, CurrentUserId, page, pageSize);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var result = await _applicationService.GetApplicationByIdAsync(id, CurrentUserId);
        return OkResponse(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequest request)
    {
        var result = await _applicationService.UpdateStatusAsync(id, request.Status, request.Notes, CurrentUserId);
        return OkResponse(result, "Status updated successfully");
    }

    [HttpPost("{id:guid}/withdraw")]
    [Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest request)
    {
        await _applicationService.WithdrawApplicationAsync(id, CurrentUserId, request.Reason);
        return OkResponse<object>(null!, "Application withdrawn");
    }

    [HttpPatch("{id:guid}/notes")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> AddNotes(Guid id, [FromBody] NotesRequest request)
    {
        var result = await _applicationService.AddRecruiterNotesAsync(id, request.Notes, CurrentUserId);
        return OkResponse(result, "Notes updated");
    }
}

public record WithdrawRequest(string? Reason);
public record NotesRequest(string Notes);
