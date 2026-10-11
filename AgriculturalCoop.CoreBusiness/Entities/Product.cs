using System;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = "Nông sản sạch";
        public string Unit { get; set; } = "kg";
        public decimal StandardPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/products/default.jpg";
    }
}
