using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.DataStore.InMemory;

public class InMemoryHomeBannerRepository : IHomeBannerRepository
{
    private HomeBanner banner = new();

    public Task<HomeBanner> GetAsync() => Task.FromResult(Copy(banner));

    public Task SaveAsync(HomeBanner value)
    {
        banner = Copy(value);
        return Task.CompletedTask;
    }

    private static HomeBanner Copy(HomeBanner value) => new()
    {
        Eyebrow = value.Eyebrow,
        Title = value.Title,
        Description = value.Description,
        ButtonText = value.ButtonText,
        ButtonUrl = value.ButtonUrl,
        ImageUrl = value.ImageUrl,
        LogoUrl = value.LogoUrl,
        BrandName = value.BrandName,
        BrandTagline = value.BrandTagline
    };
}