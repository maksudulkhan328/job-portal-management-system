using JobFlow.Domain.Common;
using JobFlow.Domain.Enums;

namespace JobFlow.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid JobId { get; set; }
    public Guid ApplicantId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public string? CoverLetter { get; set; }
    public string? CvFileName { get; set; }
    public string? CvFileUrl { get; set; }
    public string? RecruiterNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public bool IsWithdrawn { get; set; } = false;
    public DateTime? WithdrawnAt { get; set; }
    public string? WithdrawalReason { get; set; }

    public Job Job { get; set; } = null!;
    public ApplicationUser Applicant { get; set; } = null!;
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
