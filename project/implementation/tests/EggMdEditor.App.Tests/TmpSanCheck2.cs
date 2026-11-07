using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using Xunit;

public class TmpSanCheck2
{
    [Fact]
    public void LogsLog014()
    {
        var path = Path.Combine(Path.GetTempPath(), "tmp_san.md");
        File.WriteAllText(path, "Hello <script>alert(1)</script>", new UTF8Encoding(false));
        var logger = new RecordingLogger2();
        AbrirOperation.Logger = logger;
        try
        {
            AbrirOperation.Execute(path);
            Assert.Contains(logger.Entries, e => e.Message.Contains("LOG-014"));
        }
        finally
        {
            AbrirOperation.Logger = null;
            if (File.Exists(path)) File.Delete(path);
        }
    }
}

public class RecordingLogger2 : ILogger
{
    public System.Collections.Generic.List<(Microsoft.Extensions.Logging.LogLevel Level, string Message)> Entries { get; } = new();
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, formatter(state, exception)));
}