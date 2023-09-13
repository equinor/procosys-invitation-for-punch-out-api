using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            var result = Guid.TryParse(value.ToString(), out var guid)
                ? guid
                : Guid.TryParse(ByteArrayToHexString((byte[])value), out var guid2)
                    ? guid2
                    : throw new Exception($"Should be able to handle oracle raw 16 to .net guid {value}");
            return result;
        }

        public static readonly GuidTypeHandler Default = new();

        public override void SetValue(IDbDataParameter parameter, Guid value)
            => throw new NotImplementedException();

        private static string ByteArrayToHexString(IReadOnlyCollection<byte> bytes)
        {
            var result = new StringBuilder(bytes.Count * 2);

            foreach (var b in bytes)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }
    }
}
