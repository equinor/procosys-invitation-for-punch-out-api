using System;

namespace Equinor.ProCoSys.IPO.ForeignApi.Exceptions
{
    public class InvalidResultException : Exception
    {
        public InvalidResultException(string message) : base(message)
        {
        }
    }
}
