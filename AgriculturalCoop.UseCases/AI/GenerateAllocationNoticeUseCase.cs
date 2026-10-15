using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.AI;

public interface IGenerateAllocationNoticeUseCase
{
    bool IsAiAvailable { get; }
    Task<AllocationNoticeDraft> ExecuteAsync(int batchId, int lineId, string? reason, bool useAi, CancellationToken cancellationToken = default);
}
public interface ISendAllocationNoticeUseCase
{
    bool IsEmailAvailable { get; }
    Task ExecuteAsync(AllocationNoticeDraft draft, bool approved, CancellationToken cancellationToken = default);
}

public sealed class AllocationNoticeDataReader(
    IHarvestBatchRepository batches, IAllocationRepository allocations,
    IPreOrderRepository orders, IDealerRepository dealers)
{
    public async Task<AllocationNoticeSnapshot> ReadAsync(int batchId, int lineId)
    {
        var batch = await batches.GetBatchByIdAsync(batchId)
            ?? throw new PreOrderDomainException("Lô thu hoạch không còn tồn tại.");
        if (batch.Status < HarvestBatchStatus.Allocated || !batch.ActualQuantity.HasValue)
            throw new PreOrderDomainException("Cần lưu kết quả phân bổ trước khi soạn thông báo.");
        var allocation = (await allocations.GetAllocationsByBatchIdAsync(batchId))
            .SingleOrDefault(a => a.PreOrderLineId == lineId)
            ?? throw new PreOrderDomainException("Không tìm thấy kết quả phân bổ của dòng đặt trước.");
        var line = (await orders.GetLinesByBatchIdAsync(batchId)).SingleOrDefault(l => l.PreOrderLineId == lineId)
            ?? throw new PreOrderDomainException("Dòng đặt trước không còn tồn tại.");
        var order = await orders.GetPreOrderByIdAsync(line.PreOrderId)
            ?? throw new PreOrderDomainException("Đơn đặt trước không còn tồn tại.");
        if (order.Status == PreOrderStatus.Cancelled)
            throw new PreOrderDomainException("Không gửi thông báo cho đơn đã hủy.");
        var dealer = await dealers.GetDealerByIdAsync(order.DealerId)
            ?? throw new PreOrderDomainException("Đại lý không còn tồn tại.");
        return new(batchId, batch.BatchCode, batch.Product?.ProductName ?? "Nông sản", lineId,
            line.PreOrderId, dealer.DealerId, dealer.DealerName, dealer.Email?.Trim() ?? "",
            batch.ExpectedQuantity, batch.ActualQuantity.Value, allocation.RequestedQuantity,
            allocation.AllocatedQuantity, line.UnitPrice, batch.Product?.Unit ?? "kg");
    }
    public static string Fingerprint(AllocationNoticeSnapshot snapshot) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(snapshot)));
}

public sealed class GenerateAllocationNoticeUseCase(
    AllocationNoticeDataReader data, IAllocationNoticeWriter writer, IAdminAuthorization authorization)
    : IGenerateAllocationNoticeUseCase
{
    public bool IsAiAvailable => writer.IsConfigured;
    public async Task<AllocationNoticeDraft> ExecuteAsync(int batchId, int lineId, string? reason, bool useAi,
        CancellationToken cancellationToken = default)
    {
        await authorization.EnsureAdminAsync();
        if ((reason?.Length ?? 0) > 500)
            throw new PreOrderDomainException("Lý do mùa vụ tối đa 500 ký tự.");
        var snapshot = await data.ReadAsync(batchId, lineId);
        var wording = new AllocationNoticeWording(
            "Hợp tác xã xin thông báo kết quả thu hoạch và phân bổ cho đơn đặt trước của Quý đại lý.",
            "Vui lòng liên hệ hợp tác xã nếu cần đối chiếu thêm. Trân trọng cảm ơn sự đồng hành của Quý đại lý.");
        if (useAi)
        {
            if (!writer.IsConfigured)
                throw new PreOrderDomainException("Chưa cấu hình dịch vụ AI bên ngoài. Bạn có thể dùng nút Tạo bản nháp theo mẫu.");
            wording = await writer.WriteAsync(new(snapshot.BatchCode, snapshot.ProductName,
                snapshot.ExpectedQuantity, snapshot.ActualQuantity, snapshot.RequestedQuantity,
                snapshot.AllocatedQuantity, snapshot.UnitPrice, snapshot.Unit, reason?.Trim() ?? ""), cancellationToken);
        }
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        string Number(decimal value) => value.ToString("N2", culture);
        var body = new StringBuilder();
        body.AppendLine($"Kính gửi Quý đại lý {snapshot.DealerName},").AppendLine();
        body.AppendLine(wording.Introduction).AppendLine();
        body.AppendLine($"Lô: {snapshot.BatchCode} — {snapshot.ProductName}");
        body.AppendLine($"Đơn đặt trước: #{snapshot.PreOrderId}");
        body.AppendLine($"Sản lượng dự kiến toàn lô: {Number(snapshot.ExpectedQuantity)} {snapshot.Unit}");
        body.AppendLine($"Sản lượng thực tế toàn lô: {Number(snapshot.ActualQuantity)} {snapshot.Unit}");
        body.AppendLine($"Lượng đại lý đã đặt: {Number(snapshot.RequestedQuantity)} {snapshot.Unit}");
        body.AppendLine($"Lượng được phân bổ: {Number(snapshot.AllocatedQuantity)} {snapshot.Unit}");
        body.AppendLine($"Đơn giá đã chốt: {Number(snapshot.UnitPrice)} đ/{snapshot.Unit}");
        body.AppendLine($"Giá trị theo lượng phân bổ: {Number(snapshot.AllocatedQuantity * snapshot.UnitPrice)} đ");
        if (!string.IsNullOrWhiteSpace(reason)) body.AppendLine($"Lý do mùa vụ do hợp tác xã cung cấp: {reason.Trim()}");
        body.AppendLine().AppendLine(wording.Closing).AppendLine().AppendLine("BAN QUẢN LÝ COOPAGRI");
        var subject = $"Thông báo phân bổ lô {snapshot.BatchCode}";
        subject = string.Concat(subject.Select(c => char.IsControl(c) ? ' ' : c));
        return new()
        {
            Snapshot = snapshot,
            Fingerprint = AllocationNoticeDataReader.Fingerprint(snapshot),
            Subject = subject.Length > 160 ? subject[..160] : subject,
            Body = body.ToString(),
            IsAiGenerated = useAi
        };
    }
}

public sealed class SendAllocationNoticeUseCase(
    AllocationNoticeDataReader data, IAllocationNoticeEmailSender sender, IAdminAuthorization authorization)
    : ISendAllocationNoticeUseCase
{
    public bool IsEmailAvailable => sender.IsConfigured;
    public async Task ExecuteAsync(AllocationNoticeDraft draft, bool approved, CancellationToken cancellationToken = default)
    {
        await authorization.EnsureAdminAsync();
        if (!approved) throw new PreOrderDomainException("Hãy xác nhận đã kiểm tra bản nháp trước khi gửi.");
        if (!sender.IsConfigured) throw new PreOrderDomainException("Chưa cấu hình tài khoản gửi email SMTP.");
        if (string.IsNullOrWhiteSpace(draft.Subject) || draft.Subject.Length > 160 || draft.Subject.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(draft.Body) || draft.Body.Length > 16000)
            throw new PreOrderDomainException("Tiêu đề tối đa 160 ký tự, không xuống dòng; nội dung tối đa 16.000 ký tự.");
        var current = await data.ReadAsync(draft.Snapshot.BatchId, draft.Snapshot.PreOrderLineId);
        if (!string.Equals(AllocationNoticeDataReader.Fingerprint(current), draft.Fingerprint, StringComparison.Ordinal))
            throw new PreOrderDomainException("Số liệu phân bổ hoặc thông tin đại lý đã thay đổi. Hãy tạo lại bản nháp.");
        if (string.IsNullOrWhiteSpace(current.RecipientEmail) || !new EmailAddressAttribute().IsValid(current.RecipientEmail) || current.RecipientEmail.Any(char.IsControl))
            throw new PreOrderDomainException("Đại lý chưa có email hợp lệ. Hãy cập nhật hồ sơ đại lý trước khi gửi.");
        await sender.SendAsync(current.RecipientEmail, draft.Subject.Trim(), draft.Body.Trim(), cancellationToken);
    }
}
