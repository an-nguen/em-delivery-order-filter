using OrderFilter.Models;

namespace OrderFilter.Tests;

public sealed class OrderRepositoryTest
{
    [Fact]
    public void Should_ReturnEnumerable()
    {
        // arrange
        var repository = new OrderRepository(new Logger("log.txt"), "data.json");

        // act
        var orders = repository.GetOrders();

        // assert
        Assert.NotEmpty(orders);
    }
}