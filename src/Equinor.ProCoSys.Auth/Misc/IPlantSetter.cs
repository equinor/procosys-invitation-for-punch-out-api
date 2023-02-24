namespace Equinor.ProCoSys.Auth.Misc
{
    public interface IPlantSetter
    {
        void SetPlant(string plant);
        void SetCrossPlantQuery();
        void ClearCrossPlantQuery();
    }
}
