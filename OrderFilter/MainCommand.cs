using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrderFilter.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OrderFilter;

public sealed class MainCommand : Command<MainCommand.Settings>
{
    private readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private Logger? logger;

    public override int Execute(CommandContext context, Settings settings)
    {
        var currentSettings = settings.HasConfigFile ? LoadConfig(settings.ConfigFilePath!) : settings;
        using (logger = new Logger(currentSettings.DeliveryLogFilePath!))
        {
            IOrderRepository repository = new OrderRepository(logger, currentSettings.InputFilePath);
            var orders = repository.GetOrders();
            var results = FilterOrders(orders, currentSettings.CityDistrict!, currentSettings.FirstDeliveryDateTime);
            PrintResults(results);
            SaveResults(results, currentSettings.DeliveryOrderFilePath!);
        }
        return 0;
    }

    private Settings LoadConfig(string configFilePath)
    {
        var jsonStr = File.ReadAllText(configFilePath);
        var settings = JsonSerializer.Deserialize<Settings>(jsonStr, SerializerOptions)
            ?? throw new ArgumentException("Failed to parse a config file");
        var validationResult = settings.Validate();
        if (!validationResult.Successful)
        {
            throw new ArgumentException(validationResult.Message);
        }
        return settings;
    }

    private void PrintResults(IEnumerable<Order> orders)
    {
        var table = new Table();
        table.AddColumns([
            "ID",
            "Weight",
            "City District Name",
            "Delivery Date Time"
        ]);
        foreach (var order in orders)
        {
            table.AddRow(order.Id.ToString(), order.Weight.ToString(), order.CityDistrictName, order.DeliveryDateTime.ToString());
        }
        AnsiConsole.Write(table);
    }

    private IEnumerable<Order> FilterOrders(IEnumerable<Order> orders, string cityDistrictName, DateTimeOffset firstDeliveryDateTime)
    {
        logger?.Log($"Filtering the delivery orders (params: '{cityDistrictName}', '{firstDeliveryDateTime}')...");
        var filteredOrders = orders.Where(o => o.DeliveryDateTime < firstDeliveryDateTime.AddMinutes(30)
                                                  && o.DeliveryDateTime > firstDeliveryDateTime
                                                  && o.CityDistrictName.Contains(cityDistrictName))
                                   .ToList();
        logger?.Log($"A filtering process has been done. The size of the filtered list is {filteredOrders.Count}.");
        return filteredOrders;
    }

    private void SaveResults(IEnumerable<Order> filteredOrders, string outputFilePath)
    {
        try
        {
            using var fs = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            using var writer = new Utf8JsonWriter(fs);
            logger?.Log($"Writing result to the {outputFilePath} file...");
            JsonSerializer.Serialize(writer, filteredOrders, SerializerOptions);
            logger?.Log($"The filtered results have been written successfully.");
        }
        catch (Exception e)
        {
            logger?.Log($"Failed to write results to the {outputFilePath} file. Exception message: {e.Message}");
        }
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--config")]
        [Description("A config file that match to command line options")]
        [JsonIgnore]
        public string? ConfigFilePath { get; init; }

        [CommandOption("-i|--input-file")]
        [DefaultValue("data.json")]
        [Description("The input data file in JSON format.")]
        public string InputFilePath { get; init; } = null!;

        [CommandOption("-c|--city-district")]
        [Description("A city district (required)")]
        public string? CityDistrict { get; init; }

        [CommandOption("-d|--first-delivery-date-time")]
        [Description("A first delivery date time from current system locale (required)")]
        public DateTimeOffset FirstDeliveryDateTime { get; init; }

        [CommandOption("-l|--delivery-log")]
        [DefaultValue("log.txt")]
        [Description("A file path to the output log file (default is 'log.txt')")]
        public string? DeliveryLogFilePath { get; init; }

        [CommandOption("-o|--delivery-order")]
        [DefaultValue("orders.json")]
        [Description("A file path to the output order file (default is 'orders.json')")]
        public string? DeliveryOrderFilePath { get; init; }

        [JsonIgnore]
        public bool HasConfigFile = false;

        public override ValidationResult Validate()
        {
            if (!string.IsNullOrEmpty(ConfigFilePath) && File.Exists(ConfigFilePath))
            {
                HasConfigFile = true;
                return ValidationResult.Success();
            }

            if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath))
            {
                return ValidationResult.Error($"The '--input' option is not provided or invalid.");
            }
            if (string.IsNullOrEmpty(CityDistrict))
            {
                return ValidationResult.Error($"The '--city-district' filter option is not provided.");
            }
            if (FirstDeliveryDateTime == DateTimeOffset.MinValue)
            {
                return ValidationResult.Error($"The '--first-delivery-date-time' filter option is not provided or invalid.");
            }
            if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(DeliveryLogFilePath!))))
            {
                return ValidationResult.Error($"A root directory for the log '{DeliveryLogFilePath}' file does not exist!");
            }
            if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(DeliveryOrderFilePath!))))
            {
                return ValidationResult.Error($"A root directory for the delivery order '{DeliveryOrderFilePath}' file does not exist!");
            }

            return ValidationResult.Success();
        }
    }

}