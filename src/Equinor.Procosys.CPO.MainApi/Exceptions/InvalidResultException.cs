using System;

namespace Equinor.Procosys.CPO.MainApi.Exceptions
{
    public class InvalidResultException : Exception
    {
        public InvalidResultException(string message) : base(message)
        {
        }
    }
}
