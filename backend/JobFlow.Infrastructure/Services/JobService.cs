using Microsoft.EntityFrameworkCore;
using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Jobs;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly ApplicationDbContext _context;

    public JobService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<JobSummaryDto>> GetJobsAsync(JobQueryParams query)
    {
        var q = _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Where(j => j.Status == JobStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(j => j.Title.Contains(query.Search) || j.Company.Name.Contains(query.Search));

        if (query.CategoryId.HasValue)
            q = q.Where(j => j.CategoryId == query.CategoryId);

        if (!string.IsNullOrWhiteSpace(query.Location))
            q = q.Where(j => j.Location.Contains(query.Location) || (j.City != null && j.City.Contains(query.Location)));

        if (query.WorkplaceType.HasValue)
            q = q.Where(j => j.WorkplaceType == query.WorkplaceType);

        if (query.EmploymentType.HasValue)
            q = q.Where(j => j.EmploymentType == query.EmploymentType);

        if (query.ExperienceLevel.HasValue)
            q = q.Where(j => j.ExperienceLevel == query.ExperienceLevel);

        if (query.SalaryMin.HasValue)
            q = q.Where(j => j.SalaryMin >= query.SalaryMin || j.SalaryMax >= query.SalaryMin);

        if (query.SalaryMax.HasValue)
            q = q.Where(j => j.SalaryMax <= query.SalaryMax || j.SalaryMin <= query.SalaryMax);

        q = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("salary", "asc") => q.OrderBy(j => j.SalaryMin),
            ("salary", _) => q.OrderByDescending(j => j.SalaryMax),
            ("title", "asc") => q.OrderBy(j => j.Title),
            ("title", _) => q.OrderByDescending(j => j.Title),
            _ => q.OrderByDescending(j => j.IsFeatured).ThenByDescending(j => j.PublishedAt)
        };

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(j => MapToSummary(j))
            .ToListAsync();

        return new PagedResult<JobSummaryDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<JobDetailDto> GetJobByIdAsync(Guid id, Guid? currentUserId = null)
    {
        var job = await _context.Jobs
            .IgnoreQueryFilters()
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.PostedBy)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("Job not found.");

        if (currentUserId == null || job.PostedById != currentUserId)
        {
            job.ViewCount++;
            await _context.SaveChangesAsync();
        }

        var applicationCount = await _context.JobApplications.CountAsync(a => a.JobId == id);

        return MapToDetail(job, applicationCount);
    }

    public async Task<JobDetailDto> CreateJobAsync(CreateJobRequest request, Guid employerId)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId && c.OwnerId == employerId)
            ?? throw new UnauthorizedAccessException("Company not found or access denied.");

        var category = await _context.JobCategories.FindAsync(request.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var job = new Job
        {
            Title = request.Title,
            Description = request.Description,
            Responsibilities = request.Responsibilities,
            Requirements = request.Requirements,
            Benefits = request.Benefits,
            Location = request.Location,
            City = request.City,
            Country = request.Country,
            WorkplaceType = request.WorkplaceType,
            EmploymentType = request.EmploymentType,
            ExperienceLevel = request.ExperienceLevel,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            SalaryCurrency = request.SalaryCurrency,
            ShowSalary = request.ShowSalary,
            VacancyCount = request.VacancyCount,
            ApplicationDeadline = request.ApplicationDeadline,
            CompanyId = request.CompanyId,
            CategoryId = request.CategoryId,
            PostedById = employerId,
            Status = JobStatus.Draft
        };

        _context.Jobs.Add(job);

        if (request.SkillIds?.Count > 0)
        {
            var validSkills = await _context.Skills
                .Where(s => request.SkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            job.JobSkills = validSkills.Select(sid => new JobSkill { JobId = job.Id, SkillId = sid }).ToList();
        }

        await _context.SaveChangesAsync();

        return await GetJobByIdAsync(job.Id, employerId);
    }

    public async Task<JobDetailDto> UpdateJobAsync(Guid id, UpdateJobRequest request, Guid employerId)
    {
        var job = await _context.Jobs
            .Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.Id == id && j.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Job not found or access denied.");

        if (job.Status == JobStatus.Closed || job.Status == JobStatus.Archived)
            throw new InvalidOperationException("Cannot update a closed or archived job.");

        job.Title = request.Title;
        job.Description = request.Description;
        job.Responsibilities = request.Responsibilities;
        job.Requirements = request.Requirements;
        job.Benefits = request.Benefits;
        job.Location = request.Location;
        job.City = request.City;
        job.Country = request.Country;
        job.WorkplaceType = request.WorkplaceType;
        job.EmploymentType = request.EmploymentType;
        job.ExperienceLevel = request.ExperienceLevel;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.SalaryCurrency = request.SalaryCurrency;
        job.ShowSalary = request.ShowSalary;
        job.VacancyCount = request.VacancyCount;
        job.ApplicationDeadline = request.ApplicationDeadline;
        job.CategoryId = request.CategoryId;
        job.Status = JobStatus.Draft;

        _context.JobSkills.RemoveRange(job.JobSkills);
        if (request.SkillIds?.Count > 0)
        {
            var validSkills = await _context.Skills
                .Where(s => request.SkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();
            foreach (var sid in validSkills)
                _context.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = sid });
        }

        await _context.SaveChangesAsync();
        return await GetJobByIdAsync(job.Id, employerId);
    }

    public async Task DeleteJobAsync(Guid id, Guid employerId)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == id && j.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Job not found or access denied.");

        if (job.Status == JobStatus.Published)
            throw new InvalidOperationException("Cannot delete a published job. Close it first.");

        job.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<JobDetailDto> SubmitForApprovalAsync(Guid id, Guid employerId)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == id && j.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Job not found or access denied.");

        if (job.Status != JobStatus.Draft && job.Status != JobStatus.Rejected)
            throw new InvalidOperationException($"Job cannot be submitted from status '{job.Status}'.");

        job.Status = JobStatus.PendingApproval;
        await _context.SaveChangesAsync();
        return await GetJobByIdAsync(id, employerId);
    }

    public async Task<JobDetailDto> PublishJobAsync(Guid id, Guid adminId)
    {
        var job = await _context.Jobs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("Job not found.");

        if (job.Status != JobStatus.PendingApproval)
            throw new InvalidOperationException("Job must be pending approval to publish.");

        job.Status = JobStatus.Published;
        job.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetJobByIdAsync(id);
    }

    public async Task<JobDetailDto> RejectJobAsync(Guid id, Guid adminId, string reason)
    {
        var job = await _context.Jobs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("Job not found.");

        if (job.Status != JobStatus.PendingApproval)
            throw new InvalidOperationException("Job must be pending approval to reject.");

        job.Status = JobStatus.Rejected;
        job.RejectionReason = reason;
        await _context.SaveChangesAsync();
        return await GetJobByIdAsync(id);
    }

    public async Task<JobDetailDto> CloseJobAsync(Guid id, Guid employerId)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == id && j.PostedById == employerId)
            ?? throw new UnauthorizedAccessException("Job not found or access denied.");

        if (job.Status != JobStatus.Published)
            throw new InvalidOperationException("Only published jobs can be closed.");

        job.Status = JobStatus.Closed;
        job.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetJobByIdAsync(id, employerId);
    }

    public async Task<PagedResult<JobSummaryDto>> GetMyJobsAsync(Guid employerId, JobQueryParams query)
    {
        var q = _context.Jobs
            .IgnoreQueryFilters()
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Where(j => j.PostedById == employerId && !j.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(j => j.Title.Contains(query.Search));

        if (query.Status.HasValue)
            q = q.Where(j => j.Status == query.Status);

        q = q.OrderByDescending(j => j.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(j => MapToSummary(j))
            .ToListAsync();

        return new PagedResult<JobSummaryDto> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public async Task<PagedResult<JobSummaryDto>> GetAllJobsAdminAsync(JobQueryParams query)
    {
        var q = _context.Jobs
            .IgnoreQueryFilters()
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(j => j.Title.Contains(query.Search) || j.Company.Name.Contains(query.Search));

        if (query.Status.HasValue)
            q = q.Where(j => j.Status == query.Status);

        q = q.OrderByDescending(j => j.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(j => MapToSummary(j))
            .ToListAsync();

        return new PagedResult<JobSummaryDto> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    private static JobSummaryDto MapToSummary(Job j) => new(
        j.Id, j.Title, j.Company.Name, j.Company.LogoUrl, j.Location,
        j.WorkplaceType, j.EmploymentType, j.ExperienceLevel,
        j.SalaryMin, j.SalaryMax, j.SalaryCurrency, j.ShowSalary,
        j.Status, j.Category.Name,
        j.JobSkills.Select(js => js.Skill.Name).ToList(),
        j.Applications.Count, j.PublishedAt, j.ApplicationDeadline, j.IsFeatured
    );

    private static JobDetailDto MapToDetail(Job j, int applicationCount) => new(
        j.Id, j.Title, j.Description, j.Responsibilities, j.Requirements, j.Benefits,
        j.Location, j.City, j.Country, j.WorkplaceType, j.EmploymentType, j.ExperienceLevel,
        j.SalaryMin, j.SalaryMax, j.SalaryCurrency, j.ShowSalary, j.VacancyCount, j.Status,
        j.Category.Name, j.CategoryId,
        new CompanyInfoDto(j.Company.Id, j.Company.Name, j.Company.LogoUrl, j.Company.Website, j.Company.Industry, j.Company.Location),
        j.JobSkills.Select(js => new SkillDto(js.SkillId, js.Skill.Name, js.IsRequired)).ToList(),
        applicationCount, j.ViewCount, j.PublishedAt, j.ApplicationDeadline, j.IsFeatured, j.CreatedAt
    );
}
