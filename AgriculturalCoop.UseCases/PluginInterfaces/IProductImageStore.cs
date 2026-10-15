namespace AgriculturalCoop.UseCases.PluginInterfaces;

public interface IProductImageStore
{
    Task<string> SaveAsync(Stream image, long size);
    Task RemoveAsync(string url);
}
