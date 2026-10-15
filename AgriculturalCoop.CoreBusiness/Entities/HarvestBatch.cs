using System;
using System.Collections.Generic;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public enum HarvestBatchStatus
    {
        Planned = 0,         // Dự kiến
        OpenForPreOrder = 1, // Đang mở đặt trước
        Harvested = 2,       // Đã thu hoạch (đã có sản lượng thực tế)
        Allocated = 3,       // Đã phân bổ sản lượng
        Completed = 4        // Đã hoàn tất giao hàng
    }

    public class HarvestBatch
    {
        public int HarvestBatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public int ProductId { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public DateTime ExpectedHarvestDate { get; set; } = DateTime.Today.AddDays(14);
        public DateTime? ActualHarvestDate { get; set; }
        public decimal UnitPrice { get; set; }
        public HarvestBatchStatus Status { get; set; } = HarvestBatchStatus.OpenForPreOrder;
        public bool IsArchived { get; set; }

        // Navigation helpers
        public Member? Member { get; set; }
        public Product? Product { get; set; }
        public List<PreOrderLine> PreOrderLines { get; set; } = new();

        // Computed helper
        public decimal TotalOrderedQuantity { get; set; }
        public int LinkedOrderCount { get; set; }
        public decimal RemainingQuantity => Math.Max(0, ExpectedQuantity - TotalOrderedQuantity);
    }
}
