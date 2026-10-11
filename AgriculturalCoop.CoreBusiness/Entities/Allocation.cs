using System;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public class Allocation
    {
        public int AllocationId { get; set; }
        public int PreOrderLineId { get; set; }
        public int HarvestBatchId { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal AllocatedQuantity { get; set; }
        public DateTime AllocationDate { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty;

        // Navigation helpers
        public PreOrderLine? PreOrderLine { get; set; }
        public HarvestBatch? HarvestBatch { get; set; }
    }
}
