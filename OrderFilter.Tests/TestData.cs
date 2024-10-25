namespace OrderFilter.Tests;

public sealed class TestData
{
    public static readonly string DeliveryOrdersFilePath = "./orders.json";

    public static readonly string DateConvertErr = "Failed to convert";
    public static readonly string EmptyCityDistrictErr = "The '--city-district' filter option is not provided.";
    public static readonly string EmptyFirstDeliveryDateTimeErr = "Error: The '--first-delivery-date-time' filter option is not provided or \ninvalid.";
    public static readonly string DirNotExistErr = "A root directory for the ";
    public static readonly string InvalidInputFileErr = "The '--input' option is not provided or invalid.";
}