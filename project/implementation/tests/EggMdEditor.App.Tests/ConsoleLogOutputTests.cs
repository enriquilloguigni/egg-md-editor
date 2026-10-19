using System.IO;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Log output formatted to standard output (T-007): one line per log, with
// the literal level token in front of the already composed message with its
// timestamp and its session.
public class ConsoleLogOutputTests
{
    [Fact]
    public void Every_log_goes_out_as_one_line_with_the_level_token()
    {
        var writer = new StringWriter();
        var output = new ConsoleLogOutput(writer);

        output.Write(LogLevel.Info, "LOG-001: inicio del proceso");
        output.Write(LogLevel.Debug, "LOG-002: conmutación completada");
        output.Write(LogLevel.Warning, "LOG-002: claves sin resolver");
        output.Write(LogLevel.Error, "LOG-003: cierre fallido");

        Assert.Equal(
            "[Info] LOG-001: inicio del proceso\n" +
            "[Debug] LOG-002: conmutación completada\n" +
            "[Warning] LOG-002: claves sin resolver\n" +
            "[Error] LOG-003: cierre fallido\n",
            writer.ToString());
    }

    [Fact]
    public void The_default_destination_is_the_process_standard_output()
    {
        var originalStandardOutput = Console.Out;
        try
        {
            var captured = new StringWriter();
            Console.SetOut(captured);

            new ConsoleLogOutput().Write(LogLevel.Info, "registro por la salida estándar");

            Assert.Equal("[Info] registro por la salida estándar\n", captured.ToString());
        }
        finally
        {
            Console.SetOut(originalStandardOutput);
        }
    }

    [Fact]
    public void A_null_message_is_a_programming_error()
    {
        var output = new ConsoleLogOutput(new StringWriter());

        Assert.Throws<ArgumentNullException>(() => output.Write(LogLevel.Info, null!));
    }
}
