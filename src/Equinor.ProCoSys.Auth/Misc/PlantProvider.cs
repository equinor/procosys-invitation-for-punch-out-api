namespace Equinor.ProCoSys.Auth.Misc
{
    /// <summary>
    /// Set and get current plant in application. Normally set in Middleware for each request
    /// </summary>
    public class PlantProvider : IPlantProvider, IPlantSetter
    {
        public string Plant { get; private set; }
        public bool IsCrossPlantQuery { get; private set; }

        public void SetPlant(string plant) => Plant = plant;
        public void SetCrossPlantQuery() => IsCrossPlantQuery = true;
        public void ClearCrossPlantQuery() => IsCrossPlantQuery = false;
    }
}
