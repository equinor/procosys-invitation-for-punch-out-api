using Equinor.ProCoSys.Common.Misc;

namespace Equinor.ProCoSys.IPO.WebApi.Seeding
{
    public class SeedingPlantProvider : IPlantProvider
    {
        public SeedingPlantProvider(string plant) => Plant = plant;

        public string Plant { get; }

        public bool IsCrossPlantQuery => throw new System.NotImplementedException();

        public void SetTemporaryPlant(string plant) => throw new System.NotImplementedException();
        public void ReleaseTemporaryPlant() => throw new System.NotImplementedException();
    }
}
