using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.HarvestBatches
{
    public interface IViewHarvestBatchesUseCase
    {
        Task<IEnumerable<HarvestBatch>> ExecuteAsync();
    }

    public class ViewHarvestBatchesUseCase : IViewHarvestBatchesUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        public ViewHarvestBatchesUseCase(IHarvestBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<IEnumerable<HarvestBatch>> ExecuteAsync()
        {
            return await _batchRepository.GetAllBatchesAsync();
        }
    }

    public interface IViewOpenBatchesUseCase
    {
        Task<IEnumerable<HarvestBatch>> ExecuteAsync();
    }

    public class ViewOpenBatchesUseCase : IViewOpenBatchesUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        public ViewOpenBatchesUseCase(IHarvestBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<IEnumerable<HarvestBatch>> ExecuteAsync()
        {
            return await _batchRepository.GetOpenBatchesAsync();
        }
    }

    public interface IViewHarvestBatchByIdUseCase
    {
        Task<HarvestBatch?> ExecuteAsync(int id);
    }

    public class ViewHarvestBatchByIdUseCase : IViewHarvestBatchByIdUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        public ViewHarvestBatchByIdUseCase(IHarvestBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<HarvestBatch?> ExecuteAsync(int id)
        {
            return await _batchRepository.GetBatchByIdAsync(id);
        }
    }

    public interface ICreateHarvestBatchUseCase
    {
        Task ExecuteAsync(HarvestBatch batch);
    }

    public class CreateHarvestBatchUseCase : ICreateHarvestBatchUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        public CreateHarvestBatchUseCase(IHarvestBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task ExecuteAsync(HarvestBatch batch)
        {
            await _batchRepository.AddBatchAsync(batch);
        }
    }

    public interface IUpdateActualYieldUseCase
    {
        Task ExecuteAsync(int batchId, decimal actualYield, DateTime harvestDate);
    }

    public class UpdateActualYieldUseCase : IUpdateActualYieldUseCase
    {
        private readonly IHarvestBatchRepository _batchRepository;
        public UpdateActualYieldUseCase(IHarvestBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task ExecuteAsync(int batchId, decimal actualYield, DateTime harvestDate)
        {
            await _batchRepository.UpdateBatchYieldAsync(batchId, actualYield, harvestDate);
        }
    }
}
