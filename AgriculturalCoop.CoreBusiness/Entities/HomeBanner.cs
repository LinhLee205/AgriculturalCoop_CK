using System.ComponentModel.DataAnnotations;

namespace AgriculturalCoop.CoreBusiness.Entities;

public class HomeBanner
{
    [Required, StringLength(60)]
    public string Eyebrow { get; set; } = "TƯƠI TỪ NÔNG TRẠI";

    [Required(ErrorMessage = "Nhập tiêu đề banner."), StringLength(140)]
    public string Title { get; set; } = "Nông sản tươi lành, trọn vị mùa vụ.";

    [Required(ErrorMessage = "Nhập mô tả banner."), StringLength(400)]
    public string Description { get; set; } = "Chọn nông sản từ những người trồng tận tâm. Đặt trước mùa thu hoạch, đón những sản phẩm tươi ngon từ hợp tác xã.";

    [Required, StringLength(40)]
    public string ButtonText { get; set; } = "Khám phá nông sản";

    [Required, StringLength(200)]
    public string ButtonUrl { get; set; } = "/#nong-san";

    [StringLength(500)]
    public string ImageUrl { get; set; } = "/images/banner-harvest.svg";

    [StringLength(500)]
    public string? LogoUrl { get; set; } = null;

    [StringLength(100)]
    public string BrandName { get; set; } = "COOPAGRI";

    [StringLength(150)]
    public string BrandTagline { get; set; } = "Nông sản sạch theo mùa";

    [StringLength(500)]
    public string? IntroImageUrl { get; set; } = "/images/showcase-farm-produce.svg";
}
