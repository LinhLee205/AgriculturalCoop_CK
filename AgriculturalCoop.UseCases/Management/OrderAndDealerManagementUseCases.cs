using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
namespace AgriculturalCoop.UseCases.Management;

public interface IManageOrdersUseCase
{
    Task ConfirmAsync(int orderId);
    Task CancelAsync(int orderId, string reason);
}
public class ManageOrdersUseCase(IOrderManagementRepository repository, IAdminAuthorization authorization) : IManageOrdersUseCase
{
    public async Task ConfirmAsync(int orderId)
    {
        await authorization.EnsureAdminAsync();
        if (!await repository.ConfirmOrderAsync(orderId))
            throw new PreOrderDomainException("Chỉ xác nhận đơn đang chờ và chưa có phân bổ. Hãy tải lại đơn.");
    }
    public async Task CancelAsync(int orderId, string reason)
    {
        await authorization.EnsureAdminAsync();
        reason = reason?.Trim() ?? "";
        if (reason.Length is < 1 or > 500) throw new PreOrderDomainException("Nhập lý do hủy từ 1 đến 500 ký tự.");
        if (!await repository.CancelOrderAsync(orderId, reason))
            throw new PreOrderDomainException("Đơn đã hủy hoặc đã có lô phân bổ/hoàn tất. Giữ đơn để đối chiếu lịch sử, không thể hủy ở trạng thái này.");
    }
}

public interface IManageDealersUseCase
{
    Task SaveAsync(Dealer dealer);
    Task DeleteAsync(int dealerId);
}
public class ManageDealersUseCase(IDealerRepository repository, IAdminAuthorization authorization) : IManageDealersUseCase
{
    public async Task SaveAsync(Dealer dealer)
    {
        await authorization.EnsureAdminAsync();
        dealer.DealerName = dealer.DealerName.Trim();
        dealer.Phone = dealer.Phone.Trim();
        dealer.Email = (dealer.Email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(dealer.DealerName) || dealer.DealerName.Length > 150 ||
            string.IsNullOrWhiteSpace(dealer.Phone) || dealer.Phone.Length > 20 ||
            !System.Text.RegularExpressions.Regex.IsMatch(dealer.Phone, @"^\+?[0-9 ()\-]+$") || dealer.Phone.Count(char.IsDigit) is < 8 or > 15 ||
            dealer.Address.Length > 255 || dealer.ContactPerson.Length > 100 || dealer.Tier.Length is < 1 or > 30 ||
            dealer.Email.Length > 100 || (dealer.Email.Length > 0 && !new EmailAddressAttribute().IsValid(dealer.Email)))
            throw new PreOrderDomainException("Kiểm tra tên, số điện thoại, email và độ dài thông tin đại lý.");
        if (dealer.DealerId == 0) await repository.AddDealerAsync(dealer);
        else if (!await repository.UpdateDealerAsync(dealer)) throw new PreOrderDomainException("Đại lý không còn tồn tại. Hãy tải lại danh sách.");
    }
    public async Task DeleteAsync(int dealerId)
    {
        await authorization.EnsureAdminAsync();
        if (!await repository.DeleteDealerAsync(dealerId))
            throw new PreOrderDomainException("Đại lý có tài khoản hoặc đơn hàng liên quan. Dùng Ngừng giao dịch để giữ lịch sử.");
    }
}
