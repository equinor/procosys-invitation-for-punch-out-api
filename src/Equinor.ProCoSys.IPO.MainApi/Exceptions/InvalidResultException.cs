using System;

namespace Equinor.ProCoSys.IPO.MainApi.Exceptions
{
    public class InvalidResultException : Exception
    {
        public InvalidResultException(string message) : base(message)
        {
        }
    }
}
