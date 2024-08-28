using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization;

public class StringToDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateTime.TryParseExact(dateString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
        }

        throw new JsonException($"Unable to deserialize JSON. Cannot convert {reader.GetString()} to {typeof(DateTime)}. Expected format: {DateTimeFormat}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
    }
}
