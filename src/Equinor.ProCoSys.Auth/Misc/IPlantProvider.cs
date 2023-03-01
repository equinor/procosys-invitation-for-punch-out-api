namespace Equinor.ProCoSys.Auth.Misc
{
    public interface IPlantProvider
    {
        string Plant { get; }
        bool IsCrossPlantQuery { get; }
    }
}
