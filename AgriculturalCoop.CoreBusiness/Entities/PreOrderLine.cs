using System;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public class PreOrderLine
    {
        public int PreOrderLineId { get; set; }
        public int PreOrderId { get; set; }
        public int HarvestBatchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;

        // Navigation helpers
        public PreOrder? PreOrder { get; set; }
        public HarvestBatch? HarvestBatch { get; set; }
        public Allocation? Allocation { get; set; }
    }
}
