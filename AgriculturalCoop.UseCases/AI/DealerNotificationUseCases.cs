using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
namespace AgriculturalCoop.UseCases.AI;

public interface IDealerNotificationUseCases
{
    Task<IEnumerable<DealerNotification>> ListAsync();
    Task<int> UnreadCountAsync();
    Task MarkReadAsync(int id);
}

public class DealerNotificationUseCases(IDealerNotificationRepository repository, IDealerIdentity identity) : IDealerNotificationUseCases
{
    public async Task<IEnumerable<DealerNotification>> ListAsync() => await repository.GetByDealerAsync(await identity.GetDealerIdAsync());
    public async Task<int> UnreadCountAsync() => await repository.GetUnreadCountAsync(await identity.GetDealerIdAsync());
    public async Task MarkReadAsync(int id)
    {
        if (!await repository.MarkReadAsync(id, await identity.GetDealerIdAsync()))
            throw new PreOrderDomainException("Thông báo không tồn tại hoặc không thuộc tài khoản của bạn.");
    }
}

public interface IPublishAllocationNoticeUseCase
{
    Task ExecuteAsync(AllocationNoticeDraft draft, bool approved);
}

public class PublishAllocationNoticeUseCase(AllocationNoticeDataReader data, IDealerNotificationRepository repository,
    IAdminAuthorization authorization) : IPublishAllocationNoticeUseCase
{
    public async Task ExecuteAsync(AllocationNoticeDraft draft, bool approved)
    {
        await authorization.EnsureAdminAsync();
        if (!approved) throw new PreOrderDomainException("Cần kiểm tra và duyệt nội dung trước khi đăng thông báo.");
        if (string.IsNullOrWhiteSpace(draft.Subject) || draft.Subject.Length > 160 || draft.Subject.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(draft.Body) || draft.Body.Length > 16000)
            throw new PreOrderDomainException("Tiêu đề từ 1 đến 160 ký tự, không xuống dòng; nội dung từ 1 đến 16.000 ký tự.");
        var current = await data.ReadAsync(draft.Snapshot.BatchId, draft.Snapshot.PreOrderLineId);
        if (AllocationNoticeDataReader.Fingerprint(current) != draft.Fingerprint)
            throw new PreOrderDomainException("Dữ liệu phân bổ đã thay đổi. Hãy tạo lại bản nháp trước khi duyệt.");
        await repository.SaveReviewedAsync(new()
        {
            DealerId = current.DealerId, HarvestBatchId = current.BatchId, PreOrderLineId = current.PreOrderLineId,
            Kind = 1, Subject = draft.Subject.Trim(), Body = draft.Body.Trim(), IsAiGenerated = draft.IsAiGenerated
        });
    }
}
