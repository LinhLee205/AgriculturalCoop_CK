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
    }

    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
    }

    public interface IHarvestBatchRepository
    {
        Task<IEnumerable<HarvestBatch>> GetAllBatchesAsync();
        Task<HarvestBatch?> GetBatchByIdAsync(int id);
        Task<IEnumerable<HarvestBatch>> GetOpenBatchesAsync();
        Task AddBatchAsync(HarvestBatch batch);
        Task UpdateBatchYieldAsync(int batchId, decimal actualYield, System.DateTime harvestDate);
        Task UpdateBatchStatusAsync(int batchId, HarvestBatchStatus status);
    }

    public interface IDealerRepository
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(int id);
        Task AddDealerAsync(Dealer dealer);
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
        Task SaveAllocationsTransactionAsync(int batchId, IEnumerable<Allocation> allocations);
        Task<IEnumerable<Allocation>> GetAllocationsByBatchIdAsync(int batchId);
        Task<IEnumerable<Allocation>> GetAllocationsByDealerIdAsync(int dealerId);
    }
}
