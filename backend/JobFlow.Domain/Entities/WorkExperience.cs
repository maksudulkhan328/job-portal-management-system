using JobFlow.Domain.Common;

namespace JobFlow.Domain.Entities;

public class WorkExperience : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentJob { get; set; } = false;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
