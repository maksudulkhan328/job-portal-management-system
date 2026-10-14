using Microsoft.EntityFrameworkCore;
using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Companies;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _context;

    public CompanyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CompanyDto>> GetCompaniesAsync(int page, int pageSize, string? search)
    {
        var q = _context.Companies
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Name.Contains(search) || (c.Industry != null && c.Industry.Contains(search)));

        q = q.OrderBy(c => c.Name);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<CompanyDto>
        {
            Items = await Task.WhenAll(items.Select(async c =>
            {
                var jobCount = await _context.Jobs.CountAsync(j => j.CompanyId == c.Id && !j.IsDeleted);
                return MapToDto(c, jobCount);
            })).ContinueWith(t => t.Result.ToList()),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<CompanyDto> GetCompanyByIdAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id)
            ?? throw new KeyNotFoundException("Company not found.");

        var jobCount = await _context.Jobs.CountAsync(j => j.CompanyId == id && !j.IsDeleted);
        return MapToDto(company, jobCount);
    }

    public async Task<List<CompanyDto>> GetMyCompaniesAsync(Guid ownerId)
    {
        var companies = await _context.Companies
            .IgnoreQueryFilters()
            .Where(c => c.OwnerId == ownerId && !c.IsDeleted)
            .ToListAsync();

        var result = new List<CompanyDto>();
        foreach (var c in companies)
        {
            var jobCount = await _context.Jobs.CountAsync(j => j.CompanyId == c.Id && !j.IsDeleted);
            result.Add(MapToDto(c, jobCount));
        }
        return result;
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, Guid ownerId)
    {
        var company = new Company
        {
            Name = request.Name,
            Description = request.Description,
            Website = request.Website,
            Industry = request.Industry,
            Size = request.Size,
            Location = request.Location,
            City = request.City,
            Country = request.Country,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            FoundedYear = request.FoundedYear,
            OwnerId = ownerId
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return MapToDto(company, 0);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, Guid ownerId)
    {
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId)
            ?? throw new UnauthorizedAccessException("Company not found or access denied.");

        company.Name = request.Name;
        company.Description = request.Description;
        company.Website = request.Website;
        company.Industry = request.Industry;
        company.Size = request.Size;
        company.Location = request.Location;
        company.City = request.City;
        company.Country = request.Country;
        company.PhoneNumber = request.PhoneNumber;
        company.Email = request.Email;
        company.FoundedYear = request.FoundedYear;

        await _context.SaveChangesAsync();
        var jobCount = await _context.Jobs.CountAsync(j => j.CompanyId == id && !j.IsDeleted);
        return MapToDto(company, jobCount);
    }

    public async Task UpdateLogoAsync(Guid id, string logoFileName, string logoUrl, Guid ownerId)
    {
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId)
            ?? throw new UnauthorizedAccessException("Company not found or access denied.");

        company.LogoFileName = logoFileName;
        company.LogoUrl = logoUrl;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCompanyAsync(Guid id, Guid ownerId)
    {
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId)
            ?? throw new UnauthorizedAccessException("Company not found or access denied.");

        var hasActiveJobs = await _context.Jobs
            .AnyAsync(j => j.CompanyId == id && j.Status == Domain.Enums.JobStatus.Published);

        if (hasActiveJobs)
            throw new InvalidOperationException("Cannot delete a company with active job postings.");

        company.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private static CompanyDto MapToDto(Company c, int jobCount) => new(
        c.Id, c.Name, c.Description, c.Website, c.Industry, c.Size,
        c.Location, c.City, c.Country, c.LogoUrl, c.PhoneNumber, c.Email,
        c.FoundedYear, c.IsVerified, c.IsActive, jobCount, c.CreatedAt
    );
}
