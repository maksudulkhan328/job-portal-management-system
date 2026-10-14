using JobFlow.Domain.Common;

namespace JobFlow.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoFileName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? FoundedYear { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public Guid OwnerId { get; set; }

    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
