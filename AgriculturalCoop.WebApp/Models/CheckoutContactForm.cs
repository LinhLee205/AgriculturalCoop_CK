using System.ComponentModel.DataAnnotations;

namespace AgriculturalCoop.WebApp.Models;

public class CheckoutContactForm
{
    [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
    [StringLength(150, ErrorMessage = "Tên người nhận không quá 150 ký tự.")]
    public string RecipientName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự.")]
    [RegularExpression(@"^\+?[0-9 ()\-]+$", ErrorMessage = "Số điện thoại chỉ gồm chữ số, dấu +, dấu cách, ngoặc hoặc gạch ngang.")]
    public string ContactPhone { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự.")]
    public string DeliveryAddress { get; set; } = "";

    [StringLength(500, ErrorMessage = "Ghi chú không quá 500 ký tự.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
    public string PaymentMethod { get; set; } = "COD";
}
