using AgriculturalCoop.CoreBusiness.Entities;
namespace AgriculturalCoop.UseCases.PluginInterfaces;
public interface IHomeBannerRepository
{
    Task<HomeBanner> GetAsync();
    Task SaveAsync(HomeBanner banner);
}
