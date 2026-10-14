using JobFlow.Domain.Enums;

namespace JobFlow.Application.DTOs.Applications;

public record ApplyForJobRequest(
    string? CoverLetter
);

public record UpdateApplicationStatusRequest(
    ApplicationStatus Status,
    string? Notes
);

public record ApplicationDto(
    Guid Id,
    Guid JobId,
    string JobTitle,
    string CompanyName,
    string? CompanyLogoUrl,
    string ApplicantName,
    string ApplicantEmail,
    ApplicationStatus Status,
    string? CoverLetter,
    string? CvFileUrl,
    string? RecruiterNotes,
    DateTime CreatedAt,
    DateTime? StatusChangedAt
);

public record ApplicationDetailDto(
    Guid Id,
    Guid JobId,
    string JobTitle,
    string CompanyName,
    string? CompanyLogoUrl,
    CandidateSummaryDto Applicant,
    ApplicationStatus Status,
    string? CoverLetter,
    string? CvFileUrl,
    string? RecruiterNotes,
    DateTime CreatedAt,
    DateTime? StatusChangedAt,
    List<InterviewDto> Interviews
);

public record CandidateSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string? Headline,
    string? Location,
    string? ProfileImageUrl,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl
);

public record InterviewDto(
    Guid Id,
    InterviewType Type,
    InterviewStatus Status,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? Location,
    string? MeetingLink,
    string? Notes,
    string? InterviewerName
);
