using Equinor.ProCoSys.IPO.Domain;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class PlantProvider : IPlantProvider, IPlantSetter
    {
        private readonly IConfiguration _configuration;

        public PlantProvider(IConfiguration configuration) => _configuration = configuration;

        public string Plant { get; private set; }

        public bool IsOptimisticConcurrenyEnabled_HACK
            => _configuration.GetValue("IsOptimisticConcurrenyEnabled", false);

        public void SetPlant(string plant) => Plant = plant;
    }
}
