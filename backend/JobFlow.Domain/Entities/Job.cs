using JobFlow.Domain.Common;
using JobFlow.Domain.Enums;

namespace JobFlow.Domain.Entities;

public class Job : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public WorkplaceType WorkplaceType { get; set; } = WorkplaceType.OnSite;
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.MidLevel;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; } = "USD";
    public bool ShowSalary { get; set; } = true;
    public int? VacancyCount { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
    public int ViewCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public Guid CompanyId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PostedById { get; set; }

    public Company Company { get; set; } = null!;
    public JobCategory Category { get; set; } = null!;
    public ApplicationUser PostedBy { get; set; } = null!;
    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}
