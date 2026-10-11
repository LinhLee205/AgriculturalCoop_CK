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

        // Navigation helpers
        public Dealer? Dealer { get; set; }
        public List<PreOrderLine> PreOrderLines { get; set; } = new();

        public void RecalculateTotal()
        {
            TotalAmount = PreOrderLines.Sum(l => l.LineTotal);
        }
    }
}
