using JobFlow.Domain.Common;

namespace JobFlow.Domain.Entities;

public class Education : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentlyStudying { get; set; } = false;
    public string? Grade { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
