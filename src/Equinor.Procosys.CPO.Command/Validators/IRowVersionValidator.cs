namespace Equinor.Procosys.CPO.Command.Validators
{
    public interface IRowVersionValidator
    {
        bool IsValid(string rowVersion);
    }
}
