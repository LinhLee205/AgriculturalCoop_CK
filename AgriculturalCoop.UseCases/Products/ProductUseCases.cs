using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Products
{
    public interface IViewProductsUseCase
    {
        Task<IEnumerable<Product>> ExecuteAsync();
    }

    public class ViewProductsUseCase : IViewProductsUseCase
    {
        private readonly IProductRepository _productRepository;
        public ViewProductsUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> ExecuteAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }
    }

    public interface IAddProductUseCase
    {
        Task ExecuteAsync(Product product);
    }

    public class AddProductUseCase : IAddProductUseCase
    {
        private readonly IProductRepository _productRepository;
        public AddProductUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task ExecuteAsync(Product product)
        {
            await _productRepository.AddProductAsync(product);
        }
    }
}
