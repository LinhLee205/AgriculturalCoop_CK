using System;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public class Dealer
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tier { get; set; } = "Bạc"; // Kim Cương, Vàng, Bạc, Tiêu chuẩn
        public bool IsActive { get; set; } = true;
        public bool HasAccount { get; set; }
    }
}
