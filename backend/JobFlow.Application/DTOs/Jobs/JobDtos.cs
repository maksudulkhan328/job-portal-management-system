using JobFlow.Domain.Enums;

namespace JobFlow.Application.DTOs.Jobs;

public record CreateJobRequest(
    string Title,
    string Description,
    string? Responsibilities,
    string? Requirements,
    string? Benefits,
    string Location,
    string? City,
    string? Country,
    WorkplaceType WorkplaceType,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    bool ShowSalary,
    int? VacancyCount,
    DateTime? ApplicationDeadline,
    Guid CompanyId,
    Guid CategoryId,
    List<Guid>? SkillIds
);

public record UpdateJobRequest(
    string Title,
    string Description,
    string? Responsibilities,
    string? Requirements,
    string? Benefits,
    string Location,
    string? City,
    string? Country,
    WorkplaceType WorkplaceType,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    bool ShowSalary,
    int? VacancyCount,
    DateTime? ApplicationDeadline,
    Guid CategoryId,
    List<Guid>? SkillIds
);

public record JobSummaryDto(
    Guid Id,
    string Title,
    string CompanyName,
    string? CompanyLogoUrl,
    string Location,
    WorkplaceType WorkplaceType,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    bool ShowSalary,
    JobStatus Status,
    string CategoryName,
    List<string> Skills,
    int ApplicationCount,
    DateTime? PublishedAt,
    DateTime? ApplicationDeadline,
    bool IsFeatured
);

public record JobDetailDto(
    Guid Id,
    string Title,
    string Description,
    string? Responsibilities,
    string? Requirements,
    string? Benefits,
    string Location,
    string? City,
    string? Country,
    WorkplaceType WorkplaceType,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    bool ShowSalary,
    int? VacancyCount,
    JobStatus Status,
    string CategoryName,
    Guid CategoryId,
    CompanyInfoDto Company,
    List<SkillDto> Skills,
    int ApplicationCount,
    int ViewCount,
    DateTime? PublishedAt,
    DateTime? ApplicationDeadline,
    bool IsFeatured,
    DateTime CreatedAt
);

public record CompanyInfoDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? Website,
    string? Industry,
    string? Location
);

public record SkillDto(Guid Id, string Name, bool IsRequired);

public record JobQueryParams(
    string? Search,
    Guid? CategoryId,
    string? Location,
    WorkplaceType? WorkplaceType,
    EmploymentType? EmploymentType,
    ExperienceLevel? ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    JobStatus? Status,
    string? SortBy,
    string? SortOrder,
    int Page = 1,
    int PageSize = 10
);
