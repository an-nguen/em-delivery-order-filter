using Spectre.Console;
using Spectre.Console.Cli;

namespace OrderFilter.Tests;

public class CliTest
{
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
        Console.WriteLine(Directory.GetCurrentDirectory());
        var id = new Guid();
        string resultsFilePath = $"orders-{id}.json";

        // act
        var result = app.Run([
            "--input", "data.json",
            "--city-district", "Ульяновск, Засвияжский район",
            "--first-delivery-date-time", "2024-01-15 11:40:00",
            "--delivery-order", resultsFilePath
        ]);

        // assert
        var text = File.ReadAllText(resultsFilePath);
        Assert.Equal(0, result);
        Assert.NotEmpty(text);

        File.Delete(resultsFilePath);
    }
}