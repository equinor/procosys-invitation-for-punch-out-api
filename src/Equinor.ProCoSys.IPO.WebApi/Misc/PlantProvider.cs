using Equinor.ProCoSys.IPO.Domain;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class PlantProvider : IPlantProvider, IPlantSetter
    {
        public string Plant { get; private set; }

        public void SetPlant(string plant) => Plant = plant;
    }
}
