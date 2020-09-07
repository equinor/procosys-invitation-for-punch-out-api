using System;

namespace Equinor.Procosys.CPO.Domain
{
    public static class ByteArrayExtensions
    {
        public static string ConvertToString(this byte[] bytes) => Convert.ToBase64String(bytes);
    }
}
