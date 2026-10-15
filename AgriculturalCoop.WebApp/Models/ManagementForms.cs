using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;

namespace AgriculturalCoop.WebApp.Models;

public class MemberForm
{
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Nhập tên xã viên."), StringLength(150)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại."), StringLength(20)]
    public string Phone { get; set; } = "";

    [StringLength(255)]
    public string Address { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email chưa hợp lệ."), StringLength(100)]
    public string? Email { get; set; }

    [Range(typeof(decimal), "0.01", "100000",
        ErrorMessage = "Diện tích phải lớn hơn 0.")]
    public decimal AreaHectare { get; set; } = 1;

    public DateTime JoinDate { get; set; } = DateTime.Today;

    public Member ToEntity() => new()
    {
        MemberId = MemberId,
        FullName = FullName,
        Phone = Phone,
        Address = Address,
        Email = Email ?? "",
        AreaHectare = AreaHectare,
        JoinDate = JoinDate
    };

    public static MemberForm From(Member m) => new()
    {
        MemberId = m.MemberId,
        FullName = m.FullName,
        Phone = m.Phone,
        Address = m.Address,
        Email = string.IsNullOrEmpty(m.Email) ? null : m.Email,
        AreaHectare = m.AreaHectare,
        JoinDate = m.JoinDate
    };
}

public class BatchForm
{
    public int HarvestBatchId { get; set; }

    [Required(ErrorMessage = "Nhập mã lô."), StringLength(50)]
    public string BatchCode { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Chọn xã viên.")]
    public int MemberId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Chọn nông sản.")]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.01", "100000000",
        ErrorMessage = "Sản lượng phải lớn hơn 0.")]
    public decimal ExpectedQuantity { get; set; }

    [Range(typeof(decimal), "0.01", "100000000000",
        ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal UnitPrice { get; set; }

    public DateTime ExpectedHarvestDate { get; set; } =
        DateTime.Today.AddDays(14);

    public HarvestBatch ToEntity() => new()
    {
        HarvestBatchId = HarvestBatchId,
        BatchCode = BatchCode,
        MemberId = MemberId,
        ProductId = ProductId,
        ExpectedQuantity = ExpectedQuantity,
        UnitPrice = UnitPrice,
        ExpectedHarvestDate = ExpectedHarvestDate
    };

    public static BatchForm From(HarvestBatch b) => new()
    {
        HarvestBatchId = b.HarvestBatchId,
        BatchCode = b.BatchCode,
        MemberId = b.MemberId,
        ProductId = b.ProductId,
        ExpectedQuantity = b.ExpectedQuantity,
        UnitPrice = b.UnitPrice,
        ExpectedHarvestDate = b.ExpectedHarvestDate
    };
}

public class ProductForm
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Nhập tên nông sản."), StringLength(150)]
    public string ProductName { get; set; } = "";

    [Required, StringLength(100)]
    public string Category { get; set; } = "Trái cây đặc sản";

    [Required(ErrorMessage = "Nhập đơn vị tính."), StringLength(20)]
    public string Unit { get; set; } = "kg";

    [Range(typeof(decimal), "0", "100000000000",
        ErrorMessage = "Giá không được âm.")]
    public decimal StandardPrice { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = "";

    public string ImageUrl { get; set; } =
        "/images/products/placeholder.svg";

    public Product ToEntity() => new()
    {
        ProductId = ProductId,
        ProductName = ProductName,
        Category = Category,
        Unit = Unit,
        StandardPrice = StandardPrice,
        Description = Description,
        ImageUrl = ImageUrl
    };

    public static ProductForm From(Product p) => new()
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        Category = p.Category,
        Unit = p.Unit,
        StandardPrice = p.StandardPrice,
        Description = p.Description ?? "",
        ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? "/images/products/placeholder.svg" : p.ImageUrl
    };
}
