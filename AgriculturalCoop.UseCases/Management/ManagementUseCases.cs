using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Management;

public interface IManageMembersUseCase
{
    Task SaveAsync(Member member);
    Task DeleteAsync(int id);
}

public class ManageMembersUseCase(IMemberRepository repository)
    : IManageMembersUseCase
{
    public async Task SaveAsync(Member member)
    {
        if (string.IsNullOrWhiteSpace(member.FullName) || member.AreaHectare <= 0)
            throw new PreOrderDomainException(
                "Nhập tên xã viên và diện tích canh tác lớn hơn 0.");

        member.FullName = member.FullName.Trim();
        if (member.FullName.Length > 150 || string.IsNullOrWhiteSpace(member.Phone) ||
            member.Phone.Length > 20 || member.Address.Length > 255 ||
            member.Email.Length > 100 ||
            (member.Email.Length > 0 && !new EmailAddressAttribute().IsValid(member.Email)) ||
            member.AreaHectare >= 100000000m ||
            member.AreaHectare != decimal.Round(member.AreaHectare, 2))
            throw new PreOrderDomainException("Kiểm tra tên, liên hệ và diện tích (tối đa 2 chữ số thập phân).");

        if (member.MemberId == 0)
            await repository.AddMemberAsync(member);
        else
            await repository.UpdateMemberAsync(member);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await repository.DeleteMemberAsync(id))
            throw new PreOrderDomainException(
                "Xã viên đã có lô thu hoạch hoặc không còn tồn tại. Hãy giữ hồ sơ để bảo toàn lịch sử.");
    }
}

public interface IManageBatchesUseCase
{
    Task SaveAsync(HarvestBatch batch);
    Task DeleteAsync(int id);
    Task SetOpenAsync(int id, bool isOpen);
    Task SetArchivedAsync(int id, bool archived);
    Task CompleteAsync(int id);
}

public class ManageBatchesUseCase(IHarvestBatchRepository repository)
    : IManageBatchesUseCase
{
    public async Task SetArchivedAsync(int id, bool archived)
    {
        if (!await repository.SetArchivedAsync(id, archived))
            throw new PreOrderDomainException("Lô không còn tồn tại hoặc đã thay đổi. Hãy tải lại danh sách.");
    }
    public async Task CompleteAsync(int id)
    {
        if (!await repository.CompleteBatchAsync(id))
            throw new PreOrderDomainException("Chỉ hoàn tất lô đang hoạt động và đã có kết quả phân bổ đầy đủ. Hãy tải lại danh sách.");
    }
    public async Task SetOpenAsync(int id, bool isOpen)
    {
        if (!await repository.SetPreOrderAvailabilityAsync(id, isOpen))
            throw new PreOrderDomainException("Chỉ mở hoặc tạm ẩn lô chưa thu hoạch. Vui lòng tải lại danh sách.");
    }

    public async Task SaveAsync(HarvestBatch batch)
    {
        if (batch.MemberId <= 0 ||
            batch.ProductId <= 0 ||
            string.IsNullOrWhiteSpace(batch.BatchCode) ||
            batch.ExpectedQuantity <= 0 ||
            batch.UnitPrice <= 0)
        {
            throw new PreOrderDomainException(
                "Chọn xã viên, nông sản và nhập mã lô, sản lượng, đơn giá hợp lệ.");
        }

        if (batch.ExpectedQuantity != decimal.Round(batch.ExpectedQuantity, 2))
            throw new PreOrderDomainException(
                "Sản lượng chỉ được có tối đa 2 chữ số thập phân.");

        batch.BatchCode = batch.BatchCode.Trim();
        if (batch.BatchCode.Length > 50 || batch.ExpectedQuantity >= 10000000000m ||
            batch.UnitPrice >= 10000000000000000m ||
            batch.UnitPrice != decimal.Round(batch.UnitPrice, 2))
            throw new PreOrderDomainException("Mã lô tối đa 50 ký tự; đơn giá có tối đa 2 chữ số thập phân và nằm trong giới hạn lưu trữ.");

        if (batch.HarvestBatchId == 0)
        {
            await repository.AddBatchAsync(batch);
            return;
        }

        var existing = await repository.GetBatchByIdAsync(batch.HarvestBatchId)
            ?? throw new PreOrderDomainException("Lô không còn tồn tại.");

        if (batch.ExpectedQuantity < existing.TotalOrderedQuantity)
            throw new PreOrderDomainException(
                "Không giảm sản lượng dự kiến xuống dưới tổng lượng đã đặt.");

        if (existing.TotalOrderedQuantity > 0 &&
            (batch.MemberId != existing.MemberId ||
             batch.ProductId != existing.ProductId ||
             batch.UnitPrice != existing.UnitPrice))
        {
            throw new PreOrderDomainException(
                "Lô đã có đơn đặt: không đổi xã viên, nông sản hoặc đơn giá để bảo toàn giao dịch.");
        }

        if (existing.IsArchived || existing.Status >= HarvestBatchStatus.Harvested)
            throw new PreOrderDomainException(
                "Lô đã thu hoạch: giữ nguyên thông tin đã chốt.");

        await repository.UpdateBatchAsync(batch);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await repository.DeleteBatchAsync(id))
            throw new PreOrderDomainException(
                "Không xóa lô đã có đơn đặt/phân bổ, đã thu hoạch hoặc không còn tồn tại.");
    }
}

public interface IManageProductsUseCase
{
    Task SaveAsync(Product product);
    Task DeleteAsync(int id);
}

public class ManageProductsUseCase(IProductRepository repository)
    : IManageProductsUseCase
{
    public async Task SaveAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductName) ||
            string.IsNullOrWhiteSpace(product.Unit) ||
            product.StandardPrice < 0)
        {
            throw new PreOrderDomainException(
                "Nhập tên nông sản, đơn vị tính và giá không âm.");
        }

        product.ProductName = product.ProductName.Trim();
        if (product.ProductName.Length > 150 || string.IsNullOrWhiteSpace(product.Category) ||
            product.Category.Length > 100 || product.Unit.Length > 20 ||
            product.Description.Length > 500 || product.ImageUrl.Length > 500 ||
            product.StandardPrice >= 10000000000000000m ||
            product.StandardPrice != decimal.Round(product.StandardPrice, 2))
            throw new PreOrderDomainException("Kiểm tra độ dài thông tin nông sản; mô tả tối đa 500 ký tự và giá có tối đa 2 chữ số thập phân.");

        if (product.ProductId == 0)
            await repository.AddProductAsync(product);
        else
            await repository.UpdateProductAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await repository.DeleteProductAsync(id))
            throw new PreOrderDomainException(
                "Nông sản đã được sử dụng trong lô thu hoạch hoặc không còn tồn tại.");
    }
}

public interface IViewMemberRevenueUseCase
{
    Task<IEnumerable<MemberRevenue>> ExecuteAsync(DateTime? from, DateTime? to);
}

public class ViewMemberRevenueUseCase(IRevenueRepository repository)
    : IViewMemberRevenueUseCase
{
    public Task<IEnumerable<MemberRevenue>> ExecuteAsync(DateTime? from, DateTime? to)
    {
        if (from?.Date > to?.Date || to?.Date == DateTime.MaxValue.Date)
            throw new PreOrderDomainException(
                "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.");

        return repository.GetMemberRevenueAsync(
            from?.Date, to?.Date.AddDays(1));
    }
}
