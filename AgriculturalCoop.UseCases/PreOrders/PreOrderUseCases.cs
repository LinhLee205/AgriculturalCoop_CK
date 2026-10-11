using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.PreOrders
{
    public interface ICreatePreOrderUseCase
    {
        Task<int> ExecuteAsync(PreOrder preOrder);
    }

    public class CreatePreOrderUseCase : ICreatePreOrderUseCase
    {
        private readonly IPreOrderRepository _preOrderRepository;
        private readonly IHarvestBatchRepository _batchRepository;

        public CreatePreOrderUseCase(
            IPreOrderRepository preOrderRepository,
            IHarvestBatchRepository batchRepository)
        {
            _preOrderRepository = preOrderRepository;
            _batchRepository = batchRepository;
        }

        public async Task<int> ExecuteAsync(PreOrder preOrder)
        {
            if (preOrder == null)
                throw new ArgumentNullException(nameof(preOrder));

            if (!preOrder.PreOrderLines.Any())
                throw new PreOrderDomainException("Đơn đặt trước phải chứa ít nhất 1 dòng nông sản.");

            // -------------------------------------------------------------
            // LUẬT NGHIỆP VỤ 1 (K1.3): Không đặt vượt sản lượng dự kiến của lô
            // Tổng đặt trước của lô <= ExpectedQuantity
            // -------------------------------------------------------------
            foreach (var line in preOrder.PreOrderLines)
            {
                var batch = await _batchRepository.GetBatchByIdAsync(line.HarvestBatchId);
                if (batch == null)
                    throw new PreOrderDomainException($"Lô thu hoạch #{line.HarvestBatchId} không tồn tại trong hệ thống.");

                if (batch.Status != HarvestBatchStatus.OpenForPreOrder)
                    throw new PreOrderDomainException($"Lô [{batch.BatchCode}] hiện không mở nhận đặt trước (Trạng thái: {batch.Status}).");

                decimal currentOrdered = await _preOrderRepository.GetTotalOrderedQuantityForBatchAsync(line.HarvestBatchId);
                decimal requested = line.Quantity;

                if (requested <= 0)
                    throw new PreOrderDomainException($"Số lượng đặt phải lớn hơn 0 (Lô {batch.BatchCode}).");

                if (currentOrdered + requested > batch.ExpectedQuantity)
                {
                    // Ném ngoại lệ nghiệp vụ cụ thể để UI bắt và hiển thị thông báo thân thiện
                    throw new ExceededExpectedYieldException(
                        batch.HarvestBatchId, 
                        batch.ExpectedQuantity, 
                        currentOrdered, 
                        requested);
                }
            }

            preOrder.RecalculateTotal();
            if (string.IsNullOrEmpty(preOrder.OrderNumber))
            {
                preOrder.OrderNumber = $"PRE-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            }

            // Ghi nhận Header-Line trong 1 Transaction duy nhất (K2.2)
            return await _preOrderRepository.CreatePreOrderTransactionAsync(preOrder);
        }
    }

    public interface IViewPreOrdersByDealerUseCase
    {
        Task<IEnumerable<PreOrder>> ExecuteAsync(int dealerId);
    }

    public class ViewPreOrdersByDealerUseCase : IViewPreOrdersByDealerUseCase
    {
        private readonly IPreOrderRepository _preOrderRepository;
        public ViewPreOrdersByDealerUseCase(IPreOrderRepository preOrderRepository)
        {
            _preOrderRepository = preOrderRepository;
        }

        public async Task<IEnumerable<PreOrder>> ExecuteAsync(int dealerId)
        {
            return await _preOrderRepository.GetPreOrdersByDealerAsync(dealerId);
        }
    }

    public interface IViewAllPreOrdersUseCase
    {
        Task<IEnumerable<PreOrder>> ExecuteAsync();
    }

    public class ViewAllPreOrdersUseCase : IViewAllPreOrdersUseCase
    {
        private readonly IPreOrderRepository _preOrderRepository;
        public ViewAllPreOrdersUseCase(IPreOrderRepository preOrderRepository)
        {
            _preOrderRepository = preOrderRepository;
        }

        public async Task<IEnumerable<PreOrder>> ExecuteAsync()
        {
            return await _preOrderRepository.GetAllPreOrdersAsync();
        }
    }

    public interface IViewPreOrderByIdUseCase
    {
        Task<PreOrder?> ExecuteAsync(int id);
    }

    public class ViewPreOrderByIdUseCase : IViewPreOrderByIdUseCase
    {
        private readonly IPreOrderRepository _preOrderRepository;
        public ViewPreOrderByIdUseCase(IPreOrderRepository preOrderRepository)
        {
            _preOrderRepository = preOrderRepository;
        }

        public async Task<PreOrder?> ExecuteAsync(int id)
        {
            return await _preOrderRepository.GetPreOrderByIdAsync(id);
        }
    }
}
