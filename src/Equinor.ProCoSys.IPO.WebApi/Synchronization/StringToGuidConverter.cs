using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization;

public class StringToGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringGuid = reader.GetString();
            if (Guid.TryParseExact(stringGuid, "N", out Guid guid)) // "N" format: 32 digits without hyphens
            {
                return guid;
            }
            else
            {
                throw new JsonException($"The JSON value is not in a guid format: {stringGuid}");
            }
        }

        throw new JsonException("The JSON value could not be read as a guid.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N")); // "N" format: 32 digits without hyphens
    }
}
