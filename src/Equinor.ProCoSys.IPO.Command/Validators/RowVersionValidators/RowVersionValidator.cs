using System;

namespace Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators
{
    public class RowVersionValidator : IRowVersionValidator
    {
        public bool IsValid(string rowVersion)
            => !string.IsNullOrWhiteSpace(rowVersion) && TryConvertBase64StringToByteArray(rowVersion);

        private static bool TryConvertBase64StringToByteArray(string input)
        {
            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
