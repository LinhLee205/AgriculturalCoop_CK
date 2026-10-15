using AgriculturalCoop.CoreBusiness.Entities;
namespace AgriculturalCoop.UseCases.PluginInterfaces;

public interface IDealerNotificationRepository
{
    Task<IEnumerable<DealerNotification>> GetByDealerAsync(int dealerId);
    Task<int> GetUnreadCountAsync(int dealerId);
    Task<bool> MarkReadAsync(int notificationId, int dealerId);
    Task SaveReviewedAsync(DealerNotification notification);
}

public interface IDealerIdentity
{
    Task<int> GetDealerIdAsync();
}
