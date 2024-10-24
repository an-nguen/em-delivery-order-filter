using System.ComponentModel;
using System.Text.Json;
using OrderFilter.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OrderFilter;

internal sealed class MainCommand : Command<MainCommand.Settings>
{
    private readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    private Logger? logger;

    public override int Execute(CommandContext context, Settings settings)
    {
        using (logger = new Logger(settings.DeliveryLogFilePath!))
        {
            IOrderRepository repository = new OrderRepository(logger);
            var orders = repository.GetOrders();
            var results = FilterOrders(orders, settings.CityDistrict!, settings.FirstDeliveryDateTime);
            SaveResults(results, settings.DeliveryOrderFilePath!);
        }
        return 0;
    }

    private IEnumerable<Order> FilterOrders(IEnumerable<Order> orders, string cityDistrictName, DateTimeOffset firstDeliveryDateTime)
    {
        logger?.Log($"Filtering the delivery orders (params: '{cityDistrictName}', '{firstDeliveryDateTime}')...");
        var filteredOrders = orders.Where(o => o.DeliveryDateTime > firstDeliveryDateTime.AddMinutes(30)
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
            JsonSerializer.Serialize(writer, filteredOrders, serializerOptions);
            logger?.Log($"The filtered results have been written successfully.");
        }
        catch (Exception e)
        {
            logger?.Log($"Failed to write results to the {outputFilePath} file. Exception message: {e.Message}");
        }
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--city-district")]
        public string? CityDistrict { get; init; }

        [CommandOption("-d|--first-delivery-time")]
        public DateTimeOffset FirstDeliveryDateTime { get; init; }

        [CommandOption("-l|--delivery-log")]
        [DefaultValue("log.txt")]
        public string? DeliveryLogFilePath { get; init; }

        [CommandOption("-o|--delivery-order")]
        [DefaultValue("orders.json")]
        public string? DeliveryOrderFilePath { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrEmpty(CityDistrict))
            {
                return ValidationResult.Error($"The '--city-district' filter option is not provided.");
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