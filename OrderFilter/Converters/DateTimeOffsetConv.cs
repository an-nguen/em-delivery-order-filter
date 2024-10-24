using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderFilter.Converters;

public sealed class CustomDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private readonly string _format = "yyyy-MM-dd HH:mm:ss";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (DateTimeOffset.TryParseExact(reader.GetString(), _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            return value;
        }
        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}