using System;
using Equinor.ProCoSys.IPO.Domain;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class PlantProvider : IPlantProvider, IPlantSetter
    {
        private string _plant;

        public string Plant => _plant ?? throw new Exception("Could not determine current plant");

        public void SetPlant(string plant) => _plant = plant;
    }
}
