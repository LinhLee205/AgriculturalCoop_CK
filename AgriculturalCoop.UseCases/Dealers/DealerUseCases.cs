using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Dealers
{
    public interface IViewDealersUseCase
    {
        Task<IEnumerable<Dealer>> ExecuteAsync();
    }

    public class ViewDealersUseCase : IViewDealersUseCase
    {
        private readonly IDealerRepository _dealerRepository;
        public ViewDealersUseCase(IDealerRepository dealerRepository)
        {
            _dealerRepository = dealerRepository;
        }

        public async Task<IEnumerable<Dealer>> ExecuteAsync()
        {
            return await _dealerRepository.GetAllDealersAsync();
        }
    }

    public interface IGetDealerByIdUseCase
    {
        Task<Dealer?> ExecuteAsync(int id);
    }

    public class GetDealerByIdUseCase : IGetDealerByIdUseCase
    {
        private readonly IDealerRepository _dealerRepository;
        public GetDealerByIdUseCase(IDealerRepository dealerRepository)
        {
            _dealerRepository = dealerRepository;
        }

        public async Task<Dealer?> ExecuteAsync(int id)
        {
            return await _dealerRepository.GetDealerByIdAsync(id);
        }
    }
}
