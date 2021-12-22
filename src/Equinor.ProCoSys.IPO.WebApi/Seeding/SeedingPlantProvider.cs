using Equinor.ProCoSys.IPO.Domain;

namespace Equinor.ProCoSys.IPO.WebApi.Seeding
{
    public class SeedingPlantProvider : IPlantProvider
    {
        public SeedingPlantProvider(string plant) => Plant = plant;

        public string Plant { get; }

        public bool IsOptimisticConcurrenyEnabled_HACK => throw new System.NotImplementedException();

        public void SetTemporaryPlant(string plant) => throw new System.NotImplementedException();
        public void ReleaseTemporaryPlant() => throw new System.NotImplementedException();
    }
}
