using System.ComponentModel;
using System.Text;
using System.Text.Json;
using OrderFilter.Converters;

namespace OrderFilter.Tests;

public class DateTimeOffsetConvTest
{
    [Fact]
    public void Should_Read_ValidDateTime()
    {
        // arrange
        const string json = "{\"date\": \"2024-01-01 16:00:00\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        while (reader.TokenType != JsonTokenType.String)
        {
            if (!reader.Read())
            {
                break;
            }
        }
        var converter = new CustomDateTimeOffsetConverter();

        // act
        var result = converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // assert
        Assert.Equal(DateTimeOffset.Parse("2024-01-01T16:00:00"), result);
    }

    [Fact]
    public void Should_Write_ValidDateTime()
    {
        // arrange
        using var memStream = new MemoryStream();
        using var writer = new Utf8JsonWriter(memStream);
        var converter = new CustomDateTimeOffsetConverter();
        var dateTime = DateTimeOffset.Parse("2024-01-01T16:00:00");

        // act
        converter.Write(writer, dateTime, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        writer.Flush();

        // assert
        var result = Encoding.UTF8.GetString(memStream.ToArray());
        Assert.Equal("\"2024-01-01 16:00:00\"", result);
    }
}