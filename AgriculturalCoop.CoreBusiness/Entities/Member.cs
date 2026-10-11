using System;

namespace AgriculturalCoop.CoreBusiness.Entities
{
    public class Member
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal AreaHectare { get; set; } = 1.0m;
        public DateTime JoinDate { get; set; } = DateTime.Now;
    }
}
