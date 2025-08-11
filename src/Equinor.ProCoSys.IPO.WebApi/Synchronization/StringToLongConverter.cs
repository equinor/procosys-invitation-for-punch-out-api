using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization;

public class StringToLongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var result))
        {
            return result;
        }
        throw new JsonException($"Unable to deserialize JSON. Cannot convert {reader.GetString()} to {typeof(long)}.");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
