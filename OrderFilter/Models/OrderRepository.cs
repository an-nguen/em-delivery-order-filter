using System.Text.Json;
using OrderFilter.Converters;

namespace OrderFilter.Models;

public interface IOrderRepository
{
    public IEnumerable<Order> GetOrders();
}

public sealed class OrderRepository : IOrderRepository
{
    private readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly Logger logger;

    public readonly string INPUT_FILE = "data.json";

    public OrderRepository(Logger logger)
    {
        this.logger = logger;
        serializerOptions.Converters.Add(new CustomDateTimeOffsetConverter());
    }

    public IEnumerable<Order> GetOrders()
    {
        logger.Log($"{DateTimeOffset.Now:G} - Loading an input data from the {INPUT_FILE} file...");
        var data = File.ReadAllText(INPUT_FILE);
        var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(data, serializerOptions)
            ?? throw new NullReferenceException($"{DateTimeOffset.Now:G} - Failed to deserialize an input data from the file.");
        logger.Log($"{DateTimeOffset.Now:G} - The orders have been successfully loaded from the {INPUT_FILE} file.");
        return orders;
    }
}