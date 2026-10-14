using Microsoft.EntityFrameworkCore;
using JobFlow.Application.Common;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(Guid userId, int page, int pageSize)
    {
        var q = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? actionUrl = null, Guid? relatedEntityId = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId, Title = title, Message = message,
            Type = type, ActionUrl = actionUrl, RelatedEntityId = relatedEntityId
        });
        await _context.SaveChangesAsync();
    }

    private static NotificationDto MapToDto(Notification n) =>
        new(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.ReadAt, n.ActionUrl, n.CreatedAt);
}
