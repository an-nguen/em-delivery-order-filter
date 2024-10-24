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
    private readonly string inputFile;

    public OrderRepository(Logger logger, string inputFile)
    {
        this.logger = logger;
        this.inputFile = inputFile;
        serializerOptions.Converters.Add(new CustomDateTimeOffsetConverter());
    }

    public IEnumerable<Order> GetOrders()
    {
        logger.Log($"{DateTimeOffset.Now:G} - Loading an input data from the {inputFile} file...");
        var data = File.ReadAllText(inputFile);
        var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(data, serializerOptions)
            ?? throw new NullReferenceException($"{DateTimeOffset.Now:G} - Failed to deserialize an input data from the file.");
        logger.Log($"{DateTimeOffset.Now:G} - The orders have been successfully loaded from the {inputFile} file.");
        return orders;
    }
}