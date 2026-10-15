using System;
using System.Collections.Generic;
using System.Linq;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public enum PreOrderStatus
    {
        Pending = 0,             // Đang chờ xử lý
        Confirmed = 1,           // Đã xác nhận
        PartiallyAllocated = 2,  // Đã phân bổ 1 phần
        FullyAllocated = 3,      // Đã phân bổ hoàn tất
        Cancelled = 4            // Đã hủy
    }

    public class PreOrder
    {
        public int PreOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int DealerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public PreOrderStatus Status { get; set; } = PreOrderStatus.Confirmed;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? RecipientName { get; set; }
        public string? ContactPhone { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? CancellationReason { get; set; }

        // Navigation helpers
        public Dealer? Dealer { get; set; }
        public List<PreOrderLine> PreOrderLines { get; set; } = new();

        public void RefreshAllocationStatus()
        {
            if (Status == PreOrderStatus.Cancelled || PreOrderLines.Count == 0) return;
            if (!PreOrderLines.Any(l => l.Allocation != null))
            {
                if (Status is PreOrderStatus.PartiallyAllocated or PreOrderStatus.FullyAllocated) Status = PreOrderStatus.Confirmed;
                return;
            }
            Status = PreOrderLines.All(l => l.Allocation != null && l.Allocation.AllocatedQuantity >= l.Quantity)
                ? PreOrderStatus.FullyAllocated : PreOrderStatus.PartiallyAllocated;
        }

        public void RecalculateTotal()
        {
            TotalAmount = PreOrderLines.Sum(l => l.LineTotal);
        }
    }
}
