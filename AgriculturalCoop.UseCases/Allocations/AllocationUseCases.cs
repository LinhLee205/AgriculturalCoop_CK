using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.CoreBusiness.Services;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Allocations
{
    public interface IAllocateHarvestUseCase
    {
        Task<List<AllocationResultItem>> ExecuteAsync(int batchId, decimal actualHarvestQuantity);
    }

    public class AllocateHarvestUseCase : IAllocateHarvestUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        private readonly IPreOrderRepository _preOrderRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly AllocationService _allocationService;

        public AllocateHarvestUseCase(
            IHarvestBatchRepository batchRepository,
            IPreOrderRepository preOrderRepository,
            IAllocationRepository allocationRepository,
            AllocationService allocationService)
        {
            _batchRepository = batchRepository;
            _preOrderRepository = preOrderRepository;
            _allocationRepository = allocationRepository;
            _allocationService = allocationService;
        }

        public async Task<List<AllocationResultItem>> ExecuteAsync(int batchId, decimal actualHarvestQuantity)
        {
            var batch = await _batchRepository.GetBatchByIdAsync(batchId);
            if (batch == null)
                throw new PreOrderDomainException($"Lô thu hoạch #{batchId} không tồn tại.");

            if (actualHarvestQuantity < 0)
                throw new PreOrderDomainException("Sản lượng thực tế thu hoạch không được nhỏ hơn 0.");

            var lines = (await _preOrderRepository.GetLinesByBatchIdAsync(batchId)).ToList();
            if (!lines.Any())
                throw new PreOrderDomainException($"Lô [{batch.BatchCode}] chưa có bất kỳ đơn đặt trước nào để phân bổ.");

            // -------------------------------------------------------------
            // LUẬT NGHIỆP VỤ 2 (K1.3, K5.2): Chạy thuật toán phân bổ theo tỷ lệ
            // Đảm bảo: SUM(AllocatedQuantity) == ActualHarvestQuantity tuyệt đối.
            // -------------------------------------------------------------
            var resultItems = _allocationService.CalculateAllocation(batch, lines, actualHarvestQuantity);

            // Chuyển đổi sang danh sách Entity Allocation để lưu CSDL
            var allocationsToSave = resultItems.Select(r => new Allocation
            {
                PreOrderLineId = r.PreOrderLineId,
                HarvestBatchId = batchId,
                RequestedQuantity = r.RequestedQuantity,
                AllocatedQuantity = r.AllocatedQuantity,
                AllocationDate = DateTime.Now,
                Reason = r.Note
            }).ToList();

            // Lưu vào CSDL trong 1 Transaction duy nhất (K2.2)
            await _allocationRepository.SaveAllocationsTransactionAsync(batchId, allocationsToSave);

            // Cập nhật trạng thái lô sang Harvested / Allocated
            await _batchRepository.UpdateBatchYieldAsync(batchId, actualHarvestQuantity, DateTime.Now);
            await _batchRepository.UpdateBatchStatusAsync(batchId, HarvestBatchStatus.Allocated);

            return resultItems;
        }
    }

    public interface IViewAllocationsByBatchUseCase
    {
        Task<IEnumerable<Allocation>> ExecuteAsync(int batchId);
    }

    public class ViewAllocationsByBatchUseCase : IViewAllocationsByBatchUseCase
    {
        private readonly IAllocationRepository _allocationRepository;
        public ViewAllocationsByBatchUseCase(IAllocationRepository allocationRepository)
        {
            _allocationRepository = allocationRepository;
        }

        public async Task<IEnumerable<Allocation>> ExecuteAsync(int batchId)
        {
            return await _allocationRepository.GetAllocationsByBatchIdAsync(batchId);
        }
    }

    public interface IViewAllocationsByDealerUseCase
    {
        Task<IEnumerable<Allocation>> ExecuteAsync(int dealerId);
    }

    public class ViewAllocationsByDealerUseCase : IViewAllocationsByDealerUseCase
    {
        private readonly IAllocationRepository _allocationRepository;
        public ViewAllocationsByDealerUseCase(IAllocationRepository allocationRepository)
        {
            _allocationRepository = allocationRepository;
        }

        public async Task<IEnumerable<Allocation>> ExecuteAsync(int dealerId)
        {
            return await _allocationRepository.GetAllocationsByDealerIdAsync(dealerId);
        }
    }
}
