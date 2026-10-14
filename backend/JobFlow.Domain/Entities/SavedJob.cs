using JobFlow.Domain.Common;

namespace JobFlow.Domain.Entities;

public class SavedJob : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid JobId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Job Job { get; set; } = null!;
}
