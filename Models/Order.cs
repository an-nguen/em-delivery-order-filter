namespace OrderFilter.Models;

public class Order
{
    public Guid Id { get; set; }

    public double Weight { get; set; }

    public string CityDistrictName { get; set; } = null!;

    public DateTimeOffset DeliveryDateTime { get; set; }
}