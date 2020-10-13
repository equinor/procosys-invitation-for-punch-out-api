using System;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class InValidProjectException : Exception
    {
        public InValidProjectException(string error) : base(error)
        {
        }
    }
}
