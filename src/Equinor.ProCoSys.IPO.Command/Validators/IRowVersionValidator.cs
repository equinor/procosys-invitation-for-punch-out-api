namespace Equinor.ProCoSys.IPO.Command.Validators
{
    public interface IRowVersionValidator
    {
        bool IsValid(string rowVersion);
    }
}
