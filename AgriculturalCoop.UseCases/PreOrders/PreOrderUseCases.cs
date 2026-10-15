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

    public interface IValidatePreOrderUseCase
    {
        Task ValidateAsync(PreOrder preOrder);
    }

    public class CreatePreOrderUseCase : ICreatePreOrderUseCase, IValidatePreOrderUseCase
    {
        private readonly IPreOrderRepository _preOrderRepository;
        private readonly IHarvestBatchRepository _batchRepository;
        private readonly IDealerRepository _dealerRepository;

        public CreatePreOrderUseCase(
            IPreOrderRepository preOrderRepository,
            IHarvestBatchRepository batchRepository, IDealerRepository dealerRepository)
        {
            _preOrderRepository = preOrderRepository;
            _batchRepository = batchRepository;
            _dealerRepository = dealerRepository;
        }

        public async Task ValidateAsync(PreOrder preOrder)
        {
            if (preOrder == null)
                throw new ArgumentNullException(nameof(preOrder));
            if ((await _dealerRepository.GetDealerByIdAsync(preOrder.DealerId)) is not { IsActive: true })
                throw new PreOrderDomainException("Đại lý không tồn tại hoặc đã ngừng giao dịch. Vui lòng liên hệ hợp tác xã.");

            if (!preOrder.PreOrderLines.Any())
                throw new PreOrderDomainException(
                    "Đơn đặt trước phải chứa ít nhất 1 dòng nông sản.");

            foreach (var group in preOrder.PreOrderLines
                .GroupBy(l => l.HarvestBatchId))
            {
                var batch = await _batchRepository.GetBatchByIdAsync(group.Key)
                    ?? throw new PreOrderDomainException(
                        $"Lô thu hoạch #{group.Key} không tồn tại trong hệ thống.");

                if (batch.IsArchived || batch.Status != HarvestBatchStatus.OpenForPreOrder)
                    throw new PreOrderDomainException(
                        $"Lô [{batch.BatchCode}] hiện không mở nhận đặt trước.");

                if (group.Any(l =>
                    l.Quantity <= 0 ||
                    l.Quantity != decimal.Round(l.Quantity, 2)))
                {
                    throw new PreOrderDomainException(
                        "Khối lượng phải lớn hơn 0 và có tối đa 2 chữ số thập phân.");
                }

                var currentOrdered =
                    await _preOrderRepository.GetTotalOrderedQuantityForBatchAsync(
                        group.Key);

                var requested = group.Sum(l => l.Quantity);

                foreach (var line in group)
                    line.UnitPrice = batch.UnitPrice;

                if (currentOrdered + requested > batch.ExpectedQuantity)
                {
                    throw new ExceededExpectedYieldException(
                        batch.HarvestBatchId,
                        batch.ExpectedQuantity,
                        currentOrdered,
                        requested);
                }
            }

            preOrder.RecalculateTotal();
        }

        public async Task<int> ExecuteAsync(PreOrder preOrder)
        {
            await ValidateAsync(preOrder);
            preOrder.RecipientName = preOrder.RecipientName?.Trim();
            preOrder.ContactPhone = preOrder.ContactPhone?.Trim();
            preOrder.DeliveryAddress = preOrder.DeliveryAddress?.Trim();
            if (string.IsNullOrWhiteSpace(preOrder.RecipientName) || preOrder.RecipientName.Length > 150)
                throw new PreOrderDomainException("Vui lòng nhập tên người nhận (tối đa 150 ký tự).");
            if (string.IsNullOrWhiteSpace(preOrder.ContactPhone) || preOrder.ContactPhone.Length > 20 ||
                !System.Text.RegularExpressions.Regex.IsMatch(preOrder.ContactPhone, @"^\+?[0-9 ()\-]+$") ||
                preOrder.ContactPhone.Count(char.IsDigit) is < 8 or > 15)
                throw new PreOrderDomainException("Vui lòng nhập số điện thoại hợp lệ, từ 8 đến 15 chữ số.");
            if (string.IsNullOrWhiteSpace(preOrder.DeliveryAddress) || preOrder.DeliveryAddress.Length > 255)
                throw new PreOrderDomainException("Vui lòng nhập địa chỉ nhận hàng (tối đa 255 ký tự).");
            if (preOrder.Notes?.Length > 500)
                throw new PreOrderDomainException("Ghi chú không quá 500 ký tự.");

            if (string.IsNullOrEmpty(preOrder.OrderNumber))
            {
                preOrder.OrderNumber =
                    $"PRE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
            }

            return await _preOrderRepository.CreatePreOrderTransactionAsync(preOrder);
        }
    }

    public interface IViewPreOrdersForBatchUseCase
    {
        Task<IEnumerable<PreOrderLine>> ExecuteAsync(int batchId);
    }

    public class ViewPreOrdersForBatchUseCase(IPreOrderRepository repository) : IViewPreOrdersForBatchUseCase
    {
        public async Task<IEnumerable<PreOrderLine>> ExecuteAsync(int batchId)
        {
            var lines = (await repository.GetLinesByBatchIdAsync(batchId)).ToList();
            foreach (var group in lines.GroupBy(l => l.PreOrderId))
            {
                var order = await repository.GetPreOrderByIdAsync(group.Key);
                foreach (var line in group) line.PreOrder = order;
            }
            return lines.OrderBy(l => l.PreOrder?.OrderDate).ThenBy(l => l.PreOrderLineId).ToList();
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
