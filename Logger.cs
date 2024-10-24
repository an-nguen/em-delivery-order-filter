namespace OrderFilter;

public sealed class Logger : IDisposable
{
    private readonly FileStream fileStream;
    private readonly StreamWriter writer;
    private readonly TextWriter consoleOut;

    public Logger(string logFilePath)
    {
        fileStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        writer = new StreamWriter(fileStream);
        consoleOut = Console.Out;
    }

    public void Log(string message)
    {
        Console.SetOut(writer);
        Console.WriteLine($"{DateTimeOffset.Now:G} - {message}");
        Console.SetOut(consoleOut);
    }

    public void Dispose()
    {
        writer.Dispose();
        fileStream.Dispose();
    }
}