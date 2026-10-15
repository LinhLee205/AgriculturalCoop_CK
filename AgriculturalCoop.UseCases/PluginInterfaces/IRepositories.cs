using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;

namespace AgriculturalCoop.UseCases.PluginInterfaces
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
        Task AddMemberAsync(Member member);
        Task UpdateMemberAsync(Member member);
        Task<bool> DeleteMemberAsync(int id);
    }

    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
    }

    public interface IHarvestBatchRepository
    {
        Task<IEnumerable<HarvestBatch>> GetAllBatchesAsync();
        Task<HarvestBatch?> GetBatchByIdAsync(int id);
        Task<IEnumerable<HarvestBatch>> GetOpenBatchesAsync();
        Task AddBatchAsync(HarvestBatch batch);
        Task UpdateBatchAsync(HarvestBatch batch);
        Task<bool> DeleteBatchAsync(int id);
        Task UpdateBatchYieldAsync(int batchId, decimal actualYield, System.DateTime harvestDate);
        Task UpdateBatchStatusAsync(int batchId, HarvestBatchStatus status);
        Task<bool> SetPreOrderAvailabilityAsync(int batchId, bool isOpen);
        Task<bool> SetArchivedAsync(int batchId, bool archived);
        Task<bool> CompleteBatchAsync(int batchId);
    }

    public interface IDealerRepository
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(int id);
        Task AddDealerAsync(Dealer dealer);
        Task<bool> UpdateDealerAsync(Dealer dealer);
        Task<bool> DeleteDealerAsync(int dealerId);
    }

    public interface IPreOrderRepository
    {
        Task<int> CreatePreOrderTransactionAsync(PreOrder preOrder);
        Task<IEnumerable<PreOrder>> GetPreOrdersByDealerAsync(int dealerId);
        Task<IEnumerable<PreOrder>> GetAllPreOrdersAsync();
        Task<PreOrder?> GetPreOrderByIdAsync(int id);
        Task<decimal> GetTotalOrderedQuantityForBatchAsync(int batchId);
        Task<IEnumerable<PreOrderLine>> GetLinesByBatchIdAsync(int batchId);
    }

    public interface IAllocationRepository
    {
        Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations,
            decimal actualQuantity, DateTime harvestDate, IEnumerable<DealerNotification> notifications);
        Task<IEnumerable<Allocation>> GetAllocationsByBatchIdAsync(int batchId);
        Task<IEnumerable<Allocation>> GetAllocationsByDealerIdAsync(int dealerId);
    }

    public interface IOrderManagementRepository
    {
        Task<bool> ConfirmOrderAsync(int orderId);
        Task<bool> CancelOrderAsync(int orderId, string reason);
    }

    public interface IUserAccountRepository
    {
        Task<UserAccount?> FindByEmailAsync(string email);
        Task<bool> HasAdminAsync();
        Task CreateAsync(UserAccount account, string phone = "", bool initialAdmin = false);
    }

    public interface IRevenueRepository
    {
        Task<IEnumerable<MemberRevenue>> GetMemberRevenueAsync(
            System.DateTime? from, System.DateTime? to);
    }
}
