using System;

namespace Equinor.ProCoSys.IPO.Domain
{
    public static class ByteArrayExtensions
    {
        public static string ConvertToString(this byte[] bytes) => Convert.ToBase64String(bytes);
    }
}
