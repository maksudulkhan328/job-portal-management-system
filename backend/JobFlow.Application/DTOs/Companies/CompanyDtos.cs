namespace JobFlow.Application.DTOs.Companies;

public record CreateCompanyRequest(
    string Name,
    string? Description,
    string? Website,
    string? Industry,
    string? Size,
    string? Location,
    string? City,
    string? Country,
    string? PhoneNumber,
    string? Email,
    int? FoundedYear
);

public record UpdateCompanyRequest(
    string Name,
    string? Description,
    string? Website,
    string? Industry,
    string? Size,
    string? Location,
    string? City,
    string? Country,
    string? PhoneNumber,
    string? Email,
    int? FoundedYear
);

public record CompanyDto(
    Guid Id,
    string Name,
    string? Description,
    string? Website,
    string? Industry,
    string? Size,
    string? Location,
    string? City,
    string? Country,
    string? LogoUrl,
    string? PhoneNumber,
    string? Email,
    int? FoundedYear,
    bool IsVerified,
    bool IsActive,
    int JobCount,
    DateTime CreatedAt
);
