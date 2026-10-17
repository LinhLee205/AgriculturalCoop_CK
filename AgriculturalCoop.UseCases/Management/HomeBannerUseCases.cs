using System.ComponentModel.DataAnnotations;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Management;

public interface IHomeBannerUseCases
{
    Task<HomeBanner> GetAsync();
    Task SaveAsync(HomeBanner banner);
}

public class HomeBannerUseCases(IHomeBannerRepository repository) : IHomeBannerUseCases
{
    public Task<HomeBanner> GetAsync() => repository.GetAsync();

    public Task SaveAsync(HomeBanner banner)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(banner, new ValidationContext(banner), results, true))
            throw new PreOrderDomainException(results[0].ErrorMessage ?? "Kiểm tra nội dung banner.");

        if (!IsLocalUrl(banner.ButtonUrl) || !IsLocalUrl(banner.ImageUrl))
            throw new PreOrderDomainException("Liên kết cần là đường dẫn trong website, bắt đầu bằng một dấu /.");

        if (!string.IsNullOrWhiteSpace(banner.LogoUrl) && !IsLocalUrl(banner.LogoUrl))
            throw new PreOrderDomainException("Đường dẫn logo cần là đường dẫn trong website, bắt đầu bằng một dấu /.");

        banner.Eyebrow = banner.Eyebrow.Trim();
        banner.Title = banner.Title.Trim();
        banner.Description = banner.Description.Trim();
        banner.ButtonText = banner.ButtonText.Trim();
        banner.BrandName = string.IsNullOrWhiteSpace(banner.BrandName) ? "COOPAGRI" : banner.BrandName.Trim();
        banner.BrandTagline = string.IsNullOrWhiteSpace(banner.BrandTagline) ? "Nông sản sạch theo mùa" : banner.BrandTagline.Trim();

        return repository.SaveAsync(banner);
    }

    private static bool IsLocalUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.StartsWith('/') &&
        !value.StartsWith("//") && !value.Contains('\\') && !value.Any(char.IsControl) &&
        !value.StartsWith("/auth", StringComparison.OrdinalIgnoreCase);
}