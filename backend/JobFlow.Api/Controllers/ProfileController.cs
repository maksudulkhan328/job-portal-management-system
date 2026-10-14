using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.DTOs.Profile;
using JobFlow.Application.Interfaces;

namespace JobFlow.Api.Controllers;

[Authorize(Roles = "JobSeeker")]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;
    private readonly IFileService _fileService;

    public ProfileController(IProfileService profileService, IFileService fileService)
    {
        _profileService = profileService;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _profileService.GetProfileAsync(CurrentUserId);
        return OkResponse(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _profileService.UpdateProfileAsync(CurrentUserId, request);
        return OkResponse(result, "Profile updated successfully");
    }

    [HttpPost("cv")]
    public async Task<IActionResult> UploadCv(IFormFile cv)
    {
        if (cv == null || cv.Length == 0)
            return BadRequest("No file provided.");

        var (fileName, fileUrl) = await _fileService.SaveFileAsync(
            cv.OpenReadStream(), cv.FileName, "cvs",
            10 * 1024 * 1024, [".pdf"]);

        await _profileService.UpdateCvAsync(CurrentUserId, fileName, fileUrl);
        return OkResponse(new { cvUrl = fileUrl }, "CV uploaded successfully");
    }

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation([FromBody] AddEducationRequest request)
    {
        var result = await _profileService.AddEducationAsync(CurrentUserId, request);
        return CreatedResponse(result, "Education added");
    }

    [HttpPut("education/{id:guid}")]
    public async Task<IActionResult> UpdateEducation(Guid id, [FromBody] AddEducationRequest request)
    {
        var result = await _profileService.UpdateEducationAsync(CurrentUserId, id, request);
        return OkResponse(result, "Education updated");
    }

    [HttpDelete("education/{id:guid}")]
    public async Task<IActionResult> DeleteEducation(Guid id)
    {
        var result = await _profileService.DeleteEducationAsync(CurrentUserId, id);
        return OkResponse(result, "Education deleted");
    }

    [HttpPost("experience")]
    public async Task<IActionResult> AddExperience([FromBody] AddWorkExperienceRequest request)
    {
        var result = await _profileService.AddWorkExperienceAsync(CurrentUserId, request);
        return CreatedResponse(result, "Work experience added");
    }

    [HttpPut("experience/{id:guid}")]
    public async Task<IActionResult> UpdateExperience(Guid id, [FromBody] AddWorkExperienceRequest request)
    {
        var result = await _profileService.UpdateWorkExperienceAsync(CurrentUserId, id, request);
        return OkResponse(result, "Work experience updated");
    }

    [HttpDelete("experience/{id:guid}")]
    public async Task<IActionResult> DeleteExperience(Guid id)
    {
        var result = await _profileService.DeleteWorkExperienceAsync(CurrentUserId, id);
        return OkResponse(result, "Work experience deleted");
    }

    [HttpPut("skills")]
    public async Task<IActionResult> UpdateSkills([FromBody] UpdateSkillsRequest request)
    {
        var result = await _profileService.UpdateSkillsAsync(CurrentUserId, request.SkillIds);
        return OkResponse(result, "Skills updated");
    }
}

public record UpdateSkillsRequest(List<Guid> SkillIds);
