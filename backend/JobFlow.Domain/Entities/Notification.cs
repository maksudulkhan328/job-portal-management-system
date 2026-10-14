using JobFlow.Domain.Common;
using JobFlow.Domain.Enums;

namespace JobFlow.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
