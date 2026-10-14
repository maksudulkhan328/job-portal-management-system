using JobFlow.Domain.Common;

namespace JobFlow.Domain.Entities;

public class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CvFileName { get; set; }
    public string? CvFileUrl { get; set; }
    public DateTime? CvUploadedAt { get; set; }
    public bool IsProfilePublic { get; set; } = true;
    public bool IsOpenToWork { get; set; } = true;
    public int ProfileCompletionPercent { get; set; } = 0;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
}
