using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Xunit.Abstractions;

namespace OrderFilter.Tests;

public class MainCommandTest
{
    private readonly ITestOutputHelper output;

    public MainCommandTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Should_PrintOrdersToFile_On_ConfigFile()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        var dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data.json");
        var configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "my-config.json");
        var resultFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "my-orders.json");
        var appSettings = new MainCommand.Settings()
        {
            InputFilePath = dataFilePath,
            CityDistrict = "Ульяновск, Засвияжский район",
            FirstDeliveryDateTime = DateTimeOffset.Parse("2024-01-16T10:30:00"),
            DeliveryLogFilePath = "log0.txt",
            DeliveryOrderFilePath = resultFilePath,
        };
        using (var fileStream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write))
        {
            JsonSerializer.Serialize(fileStream, appSettings, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        AnsiConsole.Record();

        // act
        var result = app.Run(["--config", configFilePath]);

        // assert
        output.WriteLine(AnsiConsole.ExportText());
        output.WriteLine(dataFilePath);
        Assert.Equal(0, result);
        var text = File.ReadAllText(resultFilePath);
        Assert.NotEmpty(text);

        File.Delete(resultFilePath);
    }

    [Fact]
    public void Should_Fail_On_InvalidInputFile()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        AnsiConsole.Record();

        // act
        var result = app.Run(["--input-file", "devnull/data.json"]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.InvalidInputFileErr, output);
    }

    [Fact]
    public void Should_Fail_On_InvalidFirstDeliveryDateTime()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        AnsiConsole.Record();

        // act
        var result = app.Run(["--first-delivery-date-time", "2024-13-25 26:10:99"]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.DateConvertErr, output);
    }

    [Fact]
    public void Should_Fail_On_EmptyFirstDeliveryDateTime()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        Console.WriteLine(Directory.GetCurrentDirectory());
        AnsiConsole.Record();

        // act
        var result = app.Run([
            "--city-district", "Ульяновск, Засвияжский район"
        ]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.EmptyFirstDeliveryDateTimeErr, output);
    }

    [Fact]
    public void Should_Fail_On_EmptyDistrictName()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        AnsiConsole.Record();

        // act
        var result = app.Run(["--city-district", ""]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.EmptyCityDistrictErr, output);
    }

    [Fact]
    public void Should_Fail_On_NonExistingLogDirectory()
    {

        // arrange
        var app = new CommandApp<MainCommand>();
        AnsiConsole.Record();

        // act
        var result = app.Run([
            "--city-district", "Ульяновск, Засвияжский район",
            "--first-delivery-date-time", "2024-01-15 10:30:00",
            "--delivery-log", "devnull/log.txt"
        ]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.DirNotExistErr, output);
    }

    [Fact]
    public void Should_Fail_On_NonExistingOrdersDirectory()
    {

        // arrange
        var app = new CommandApp<MainCommand>();
        AnsiConsole.Record();

        // act
        var result = app.Run([
            "--city-district", "Ульяновск, Засвияжский район",
            "--first-delivery-date-time", "2024-01-15 10:30:00",
            "--delivery-order", "devnull/orders.json"
        ]);

        // assert
        var output = AnsiConsole.ExportText();
        Assert.Equal(-1, result);
        Assert.Contains(TestData.DirNotExistErr, output);
    }

    [Fact]
    public void Should_PrintOrdersToFile()
    {
        // arrange
        var app = new CommandApp<MainCommand>();
        var id = Guid.NewGuid();
        string resultsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"orders-{id}.json");

        // act
        var result = app.Run([
            "--input", "data.json",
            "--city-district", "Ульяновск, Засвияжский район",
            "--first-delivery-date-time", "2024-01-15T11:40:00",
            "--delivery-log", $"{nameof(Should_PrintOrdersToFile)}.txt",
            "--delivery-order", resultsFilePath
        ]);

        // assert
        Assert.Equal(0, result);
        var text = File.ReadAllText(resultsFilePath);
        Assert.NotEmpty(text);

        File.Delete(resultsFilePath);
    }
}