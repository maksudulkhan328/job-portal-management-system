using JobFlow.Domain.Common;
using JobFlow.Domain.Enums;

namespace JobFlow.Domain.Entities;

public class Interview : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public InterviewType Type { get; set; } = InterviewType.Video;
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    public string? Feedback { get; set; }
    public string? InterviewerName { get; set; }
    public DateTime? CompletedAt { get; set; }

    public JobApplication Application { get; set; } = null!;
}
