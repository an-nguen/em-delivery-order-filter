using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrderFilter.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OrderFilter;


public sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
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

internal sealed class MainCommand : Command<MainCommand.Settings>
{
    private readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

    public IEnumerable<Order> Orders = [];

    public MainCommand()
    {
        serializerOptions.Converters.Add(new DateTimeOffsetConverter());
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        LoadData(settings.DataFilePath);
        return 0;
    }

    public void LoadData(string dataFilePath)
    {
        var data = File.ReadAllText(dataFilePath);
        var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(data, serializerOptions)
            ?? throw new NullReferenceException("Failed to load an input data");
        Orders = orders;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-i|--input")]
        [DefaultValue("./data.json")]
        public string DataFilePath { get; init; } = null!;

        [CommandOption("-c|--city-district")]
        public string CityDistrict { get; init; } = null!;

        [CommandOption("-d|--first-delivery-time")]
        public DateTimeOffset FirstDeliveryTime { get; init; }

        [CommandOption("-l|--delivery-log")]
        public string DeliveryLog { get; init; } = null!;

        [CommandOption("-o|--delivery-order")]
        public string DeliveryOrder { get; init; } = null!;

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }
    }

}