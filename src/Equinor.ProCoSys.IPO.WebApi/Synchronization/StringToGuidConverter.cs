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

            // Try parsing with hyphens (D format).
            if (Guid.TryParseExact(stringGuid, "D", out Guid guid))
            {
                return guid;
            }

            // If the D format fails, try without hyphens (N format).
            else if (Guid.TryParseExact(stringGuid, "N", out guid))
            {
                return guid;
            }

            throw new JsonException($"Unable to deserialize JSON. Cannot convert {stringGuid} to {typeof(Guid)}.");
        }

        throw new JsonException("The JSON value could not be read as a guid.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        // This method writes the Guid in the non-hyphenated format.
        writer.WriteStringValue(value.ToString("N"));
    }
}
