namespace Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators
{
    public interface IRowVersionValidator
    {
        bool IsValid(string rowVersion);
    }
}
