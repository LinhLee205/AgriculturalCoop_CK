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
            if (batch.IsArchived || batch.Status == HarvestBatchStatus.Completed)
                throw new PreOrderDomainException("Lô đã lưu trữ hoặc hoàn tất. Không thay đổi kết quả đã chốt.");

            if (actualHarvestQuantity < 0 ||
    actualHarvestQuantity != decimal.Round(actualHarvestQuantity, 2))
            {
                throw new PreOrderDomainException(
                    "Sản lượng thực tế không âm và có tối đa 2 chữ số thập phân.");
            }

            var lines = (await _preOrderRepository.GetLinesByBatchIdAsync(batchId))
                .Where(l => l.PreOrder?.Status != PreOrderStatus.Cancelled).ToList();
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

            var culture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            string Number(decimal value) => value.ToString("N2", culture);
            var notices = allocationsToSave.Select(a =>
            {
                var line = lines.Single(l => l.PreOrderLineId == a.PreOrderLineId);
                return new DealerNotification
                {
                    DealerId = line.PreOrder?.DealerId ?? throw new PreOrderDomainException("Thiếu thông tin đại lý của đơn đặt."),
                    HarvestBatchId = batchId, PreOrderLineId = a.PreOrderLineId,
                    Subject = $"Kết quả phân bổ lô {batch.BatchCode}",
                    Body = $"Hợp tác xã đã cập nhật kết quả phân bổ.\n\nLô: {batch.BatchCode} — {batch.Product?.ProductName}\nĐơn đặt trước: #{line.PreOrderId}\nSản lượng thực tế toàn lô: {Number(actualHarvestQuantity)} kg\nLượng đã đặt: {Number(a.RequestedQuantity)} kg\nLượng được phân bổ: {Number(a.AllocatedQuantity)} kg\n\n{a.Reason}\n\nXem mục Phân bổ nhận được để đối chiếu."
                };
            }).ToList();
            // Results, batch state and deterministic web notices commit together.
            await _allocationRepository.SaveAllocationsTransactionAsync(batchId, allocationsToSave,
                actualHarvestQuantity, DateTime.Now, notices);

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
