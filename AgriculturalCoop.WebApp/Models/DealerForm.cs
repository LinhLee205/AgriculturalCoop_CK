using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;
namespace AgriculturalCoop.WebApp.Models;
public class DealerForm
{
    public int DealerId { get; set; }
    [Required(ErrorMessage = "Nhập tên đại lý."), StringLength(150)] public string DealerName { get; set; } = "";
    [StringLength(100)] public string ContactPerson { get; set; } = "";
    [Required(ErrorMessage = "Nhập điện thoại."), StringLength(20), Phone(ErrorMessage = "Điện thoại chưa hợp lệ.")] public string Phone { get; set; } = "";
    [StringLength(255)] public string Address { get; set; } = "";
    [StringLength(100), RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email chưa hợp lệ.")] public string Email { get; set; } = "";
    [Required, StringLength(30)] public string Tier { get; set; } = "Tiêu chuẩn";
    public bool IsActive { get; set; } = true;
    public Dealer ToEntity() => new() { DealerId = DealerId, DealerName = DealerName, ContactPerson = ContactPerson, Phone = Phone, Address = Address, Email = Email, Tier = Tier, IsActive = IsActive };
    public static DealerForm From(Dealer d) => new() { DealerId = d.DealerId, DealerName = d.DealerName, ContactPerson = d.ContactPerson, Phone = d.Phone, Address = d.Address, Email = d.Email ?? "", Tier = d.Tier, IsActive = d.IsActive };
}
