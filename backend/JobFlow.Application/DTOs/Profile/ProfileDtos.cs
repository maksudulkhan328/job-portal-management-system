namespace JobFlow.Application.DTOs.Profile;

public record UpdateProfileRequest(
    string? Headline,
    string? Summary,
    string? PhoneNumber,
    string? Location,
    string? City,
    string? Country,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? WebsiteUrl,
    bool IsProfilePublic,
    bool IsOpenToWork
);

public record AddEducationRequest(
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrentlyStudying,
    string? Grade,
    string? Description
);

public record AddWorkExperienceRequest(
    string CompanyName,
    string JobTitle,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrentJob,
    string? Description
);

public record CandidateProfileDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    string? Headline,
    string? Summary,
    string? PhoneNumber,
    string? Location,
    string? City,
    string? Country,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? WebsiteUrl,
    string? CvFileUrl,
    bool IsProfilePublic,
    bool IsOpenToWork,
    int ProfileCompletionPercent,
    List<EducationDto> Educations,
    List<WorkExperienceDto> WorkExperiences,
    List<SkillItemDto> Skills
);

public record EducationDto(
    Guid Id,
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrentlyStudying,
    string? Grade,
    string? Description
);

public record WorkExperienceDto(
    Guid Id,
    string CompanyName,
    string JobTitle,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrentJob,
    string? Description
);

public record SkillItemDto(Guid SkillId, string SkillName, int? ProficiencyLevel);
