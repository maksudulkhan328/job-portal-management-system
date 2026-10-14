using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.DTOs.Companies;
using JobFlow.Application.Interfaces;

namespace JobFlow.Api.Controllers;

public class CompaniesController : BaseController
{
    private readonly ICompanyService _companyService;
    private readonly IFileService _fileService;

    public CompaniesController(ICompanyService companyService, IFileService fileService)
    {
        _companyService = companyService;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _companyService.GetCompaniesAsync(page, pageSize, search);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        var result = await _companyService.GetCompanyByIdAsync(id);
        return OkResponse(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMyCompanies()
    {
        var result = await _companyService.GetMyCompaniesAsync(CurrentUserId);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        var result = await _companyService.CreateCompanyAsync(request, CurrentUserId);
        return CreatedResponse(result, "Company created successfully");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        var result = await _companyService.UpdateCompanyAsync(id, request, CurrentUserId);
        return OkResponse(result, "Company updated successfully");
    }

    [HttpPost("{id:guid}/logo")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo)
    {
        if (logo == null || logo.Length == 0)
            return BadRequest("No file provided.");

        var (fileName, fileUrl) = await _fileService.SaveFileAsync(
            logo.OpenReadStream(), logo.FileName, "logos",
            2 * 1024 * 1024, [".jpg", ".jpeg", ".png", ".gif", ".webp"]);

        await _companyService.UpdateLogoAsync(id, fileName, fileUrl, CurrentUserId);
        return OkResponse(new { logoUrl = fileUrl }, "Logo uploaded successfully");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        await _companyService.DeleteCompanyAsync(id, CurrentUserId);
        return OkResponse<object>(null!, "Company deleted");
    }
}
