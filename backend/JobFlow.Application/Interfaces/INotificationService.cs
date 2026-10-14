using JobFlow.Application.Common;
using JobFlow.Domain.Enums;

namespace JobFlow.Application.Interfaces;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime? ReadAt,
    string? ActionUrl,
    DateTime CreatedAt
);

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(Guid userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? actionUrl = null, Guid? relatedEntityId = null);
}
