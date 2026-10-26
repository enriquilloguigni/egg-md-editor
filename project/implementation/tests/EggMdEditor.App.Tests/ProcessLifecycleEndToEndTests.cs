using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EggMdEditor.App.Startup;
using Xunit;

namespace EggMdEditor.App.Tests;

// T-018 (AC-005, AC-006, AC-007): evidencias a nivel de proceso, no solo de
// unidad. Cada prueba lanza el ejecutable real de la aplicación con el arnés
// EGGMD_TEST_MODE (Avalonia headless) y observa su salida de estado, su salida
// estándar de error y su código de finalización. El identificador de instancia
// única es propio de cada prueba para que puedan ejecutarse en paralelo.
public sealed class ProcessLifecycleEndToEndTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(60);
    private static readonly string ApplicationAssemblyPath =
        Path.Combine(AppContext.BaseDirectory, "EggMdEditor.App.dll");

    [Fact]
    public async Task ASecondInstanceFocusesTheRunningWindowWithoutCreatingAnother()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var applicationId = NewApplicationId();
        var primaryStatePath = NewStatePath();

        await using var primary = LaunchedApp.Start(
            applicationId,
            primaryStatePath,
            new Dictionary<string, string?>
            {
                [StartupTestMode.MinimizeOnReadyVariable] = "1",
            });

        // AC-005: SC-001 existe y se ha presentado; se deja minimizada para
        // que la activación de la segunda instancia tenga un efecto observable.
        var presentedEvents = await primary.WaitForEventAsync(
            "window-minimized",
            cancellationToken);
        Assert.Single(presentedEvents, evt => evt.Name == "window-presented");

        var secondaryStatePath = NewStatePath();
        await using (var secondary = LaunchedApp.Start(
            applicationId,
            secondaryStatePath,
            extraEnvironment: null))
        {
            var secondaryResult = await secondary.WaitForExitAsync(ProcessTimeout, cancellationToken);

            Assert.Equal(ExitCodes.Success, secondaryResult.ExitCode);
            Assert.Contains(
                secondaryResult.Events,
                evt => evt.Name == "secondary" && evt.Value("windowCount") == "0");
            Assert.DoesNotContain(
                secondaryResult.Events,
                evt => evt.Name is "window-created" or "window-presented");
            AssertNoRunningProcess(secondaryResult.Pid);
        }

        // El efecto se observa en la ventana primaria: se restaura de
        // minimizada y el contador de activaciones avanza; no se creó una
        // segunda ventana en el proceso primario.
        var primaryEvents = await primary.WaitForEventAsync(
            "activation-applied",
            cancellationToken);
        Assert.Contains(primaryEvents, evt => evt.Name == "activation-received");

        var activation = Assert.Single(primaryEvents, evt => evt.Name == "activation-applied");
        Assert.Equal("1", activation.Value("count"));
        Assert.Equal("Normal", activation.Value("state"));
        Assert.Equal("True", activation.Value("visible"));
        Assert.Single(primaryEvents, evt => evt.Name == "window-created");
        Assert.Single(primaryEvents, evt => evt.Name == "window-presented");
        Assert.True(primary.IsRunning, "La instancia primaria debe seguir viva tras la activación.");
    }

    [Theory]
    [InlineData(false)] // AC-006/ERR-001: fallo antes de crear SC-001.
    [InlineData(true)] // AC-006/ERR-002: fallo de pintado antes de presentar SC-001.
    public async Task StartupFailureEndsWithErrorCodeSpanishMessageAndWithoutAUsableWindow(
        bool failDuringRender)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var applicationId = NewApplicationId();
        var statePath = NewStatePath();
        var extraEnvironment = failDuringRender
            ? new Dictionary<string, string?>
            {
                [StartupTestMode.FailRenderVariable] = "1",
            }
            : new Dictionary<string, string?>
            {
                [StartupTestMode.FailStartupVariable] = "1",
            };

        await using var app = LaunchedApp.Start(applicationId, statePath, extraEnvironment);
        var result = await app.WaitForExitAsync(ProcessTimeout, cancellationToken);

        // ERR-001/ERR-002: el proceso termina con código de error y sin
        // procesos residuales.
        Assert.Equal(ExitCodes.StartupFailure, result.ExitCode);
        AssertNoRunningProcess(result.Pid);

        // LOG-001: el mensaje exigido, en español, sale por la salida de error.
        Assert.Contains(
            "Error al iniciar la aplicación: ",
            result.StandardError,
            StringComparison.Ordinal);
        Assert.Contains(
            nameof(InvalidOperationException),
            result.StandardError,
            StringComparison.Ordinal);
        var simulatedFailure = failDuringRender
            ? StartupTestMode.RenderFailureMessage
            : StartupTestMode.StartupFailureMessage;
        Assert.Contains(simulatedFailure, result.StandardError, StringComparison.Ordinal);

        // AC-006: ninguna ventana usable. En ERR-001 no llega a crearse; en
        // ERR-002 se crea pero no se presenta y se cierra antes de terminar.
        Assert.DoesNotContain(result.Events, evt => evt.Name == "window-presented");
        if (failDuringRender)
        {
            Assert.Single(result.Events, evt => evt.Name == "window-created");
            Assert.Contains(result.Events, evt => evt.Name == "window-closed");
        }
        else
        {
            Assert.DoesNotContain(result.Events, evt => evt.Name == "window-created");
        }

        Assert.Contains(
            result.Events,
            evt => evt.Name == "process-exit" && evt.Value("code") == ExitCodes.StartupFailure.ToString());
    }

    [Fact]
    public async Task ClosingEndsTheProcessAndTheNextLaunchStartsInSpanishAndSystemTheme()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var applicationId = NewApplicationId();

        // Primer arranque: se presenta en español/Sistema, se conmutan idioma
        // y tema en caliente y se cierra como lo haría el usuario.
        var firstStatePath = NewStatePath();
        var firstResult = await RunToExitAsync(
            applicationId,
            firstStatePath,
            new Dictionary<string, string?>
            {
                [StartupTestMode.SwitchLanguageVariable] = "en",
                [StartupTestMode.SwitchThemeVariable] = "dark",
                [StartupTestMode.CloseAfterVariable] = "1500",
            },
            cancellationToken);

        Assert.Equal(ExitCodes.Success, firstResult.ExitCode);
        AssertNoRunningProcess(firstResult.Pid);
        Assert.Contains(firstResult.Events, evt => evt.Name == "primary");

        var firstPresented = Assert.Single(
            firstResult.Events,
            evt => evt.Name == "window-presented");
        Assert.Equal("es", firstPresented.Value("language"));
        Assert.Equal("System", firstPresented.Value("themePreference"));

        Assert.Equal(
            "en",
            Assert.Single(firstResult.Events, evt => evt.Name == "language-changed")
                .Value("language"));
        var changedTheme = Assert.Single(
            firstResult.Events,
            evt => evt.Name == "theme-changed");
        Assert.Equal("Dark", changedTheme.Value("theme"));
        Assert.Equal("Dark", changedTheme.Value("preference"));

        // AC-007/LOG-003: el cierre completa la sesión sin residuales.
        Assert.Contains(firstResult.Events, evt => evt.Name == "window-closed");
        Assert.Contains(firstResult.Events, evt => evt.Name == "shutdown-requested");
        Assert.Contains(
            firstResult.Events,
            evt => evt.Name == "process-exit" && evt.Value("code") == ExitCodes.Success.ToString());

        // Segundo arranque con el mismo identificador de instancia: si el
        // proceso anterior dejara cerrojo, canal o procesos residuales, esta
        // instancia sería secundaria y no presentaría SC-001. Además debe
        // partir de Español y de la preferencia Sistema (no persistencia).
        var secondStatePath = NewStatePath();
        var secondResult = await RunToExitAsync(
            applicationId,
            secondStatePath,
            new Dictionary<string, string?>
            {
                [StartupTestMode.CloseAfterVariable] = "1000",
            },
            cancellationToken);

        Assert.Equal(ExitCodes.Success, secondResult.ExitCode);
        AssertNoRunningProcess(secondResult.Pid);
        Assert.Contains(secondResult.Events, evt => evt.Name == "primary");
        Assert.DoesNotContain(secondResult.Events, evt => evt.Name == "secondary");

        var secondPresented = Assert.Single(
            secondResult.Events,
            evt => evt.Name == "window-presented");
        Assert.Equal("es", secondPresented.Value("language"));
        Assert.Equal("System", secondPresented.Value("themePreference"));
        Assert.DoesNotContain(secondResult.Events, evt => evt.Name == "language-changed");
        Assert.DoesNotContain(secondResult.Events, evt => evt.Name == "theme-changed");
        Assert.Contains(secondResult.Events, evt => evt.Name == "window-closed");
    }

    private static async Task<ProcessResult> RunToExitAsync(
        string applicationId,
        string statePath,
        IReadOnlyDictionary<string, string?> environment,
        CancellationToken cancellationToken)
    {
        await using var app = LaunchedApp.Start(applicationId, statePath, environment);
        return await app.WaitForExitAsync(ProcessTimeout, cancellationToken);
    }

    private static string NewApplicationId() =>
        "EggMdEditor.Tests.E2E." + Guid.NewGuid().ToString("N");

    private static string NewStatePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "egg-md-editor-t18");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, Guid.NewGuid().ToString("N") + ".state");
    }

    /// <summary>
    /// AC-006/AC-007: el PID lanzado ya no está en ejecución; no queda ningún
    /// proceso residual del arranque/cierre comprobado.
    /// </summary>
    private static void AssertNoRunningProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            Assert.True(
                process.HasExited,
                $"El proceso {processId} sigue en ejecución después de la prueba.");
        }
        catch (ArgumentException)
        {
            // El proceso ya no existe: finalización completa.
        }
    }

    /// <summary>
    /// Ejecuta el binario real de la aplicación con el arnés E2E, captura sus
    /// flujos y lee las evidencias del fichero de estado.
    /// </summary>
    private sealed class LaunchedApp : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Task<string> _standardOutput;
        private readonly Task<string> _standardError;
        private bool _disposed;

        private LaunchedApp(Process process, string statePath)
        {
            _process = process;
            StatePath = statePath;
            _standardOutput = process.StandardOutput.ReadToEndAsync();
            _standardError = process.StandardError.ReadToEndAsync();
        }

        public string StatePath { get; }

        public int Pid => _process.Id;

        public bool IsRunning => !_process.HasExited;

        public static LaunchedApp Start(
            string applicationId,
            string statePath,
            IReadOnlyDictionary<string, string?>? extraEnvironment)
        {
            if (!File.Exists(ApplicationAssemblyPath))
            {
                throw new FileNotFoundException(
                    "No se encontró el ejecutable de la aplicación bajo prueba.",
                    ApplicationAssemblyPath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ResolveDotnetHost(),
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add(ApplicationAssemblyPath);

            // El entorno del runner no debe filtrar opciones del arnés entre
            // pruebas: se eliminan y se fijan las propias de esta ejecución.
            foreach (var variable in startInfo.Environment.Keys
                         .Where(key => key.StartsWith("EGGMD_TEST_", StringComparison.Ordinal))
                         .ToArray())
            {
                startInfo.Environment.Remove(variable);
            }

            startInfo.Environment[StartupTestMode.ModeVariable] = "1";
            startInfo.Environment[StartupTestMode.ApplicationIdVariable] = applicationId;
            startInfo.Environment[StartupTestMode.StateFileVariable] = statePath;
            startInfo.Environment["AVALONIA_TELEMETRY_OPTOUT"] = "1";

            if (extraEnvironment is not null)
            {
                foreach (var (key, value) in extraEnvironment)
                {
                    if (value is not null)
                    {
                        startInfo.Environment[key] = value;
                    }
                }
            }

            var process = new Process { StartInfo = startInfo };
            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException(
                        "No se pudo iniciar el proceso de la aplicación bajo prueba.");
                }
            }
            catch
            {
                process.Dispose();
                throw;
            }

            return new LaunchedApp(process, statePath);
        }

        public async Task<IReadOnlyList<StateEvent>> WaitForEventAsync(
            string eventName,
            CancellationToken cancellationToken)
        {
            var watchdog = Stopwatch.StartNew();
            while (watchdog.Elapsed < ProcessTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var events = ReadStateFile(StatePath);
                if (events.Any(evt => evt.Name == eventName))
                {
                    return events;
                }

                if (_process.HasExited)
                {
                    throw new InvalidOperationException(
                        $"El proceso {Pid} terminó antes de registrar «{eventName}». "
                        + await BuildDiagnosticsAsync());
                }

                await Task.Delay(50, cancellationToken);
            }

            throw new TimeoutException(
                $"El proceso {Pid} no registró «{eventName}» en "
                + $"{ProcessTimeout.TotalSeconds} s. {await BuildDiagnosticsAsync()}");
        }

        public async Task<ProcessResult> WaitForExitAsync(
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);

            try
            {
                await _process.WaitForExitAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                await KillAsync();
                throw new TimeoutException(
                    $"El proceso {Pid} no terminó en {timeout.TotalSeconds} s. "
                    + await BuildDiagnosticsAsync());
            }

            var standardOutput = await _standardOutput;
            var standardError = await _standardError;
            return new ProcessResult(
                _process.ExitCode,
                standardOutput,
                standardError,
                ReadStateFile(StatePath),
                Pid);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                if (!_process.HasExited)
                {
                    await KillAsync();
                }
            }
            finally
            {
                _process.Dispose();
            }
        }

        private async Task KillAsync()
        {
            try
            {
                _process.Kill(entireProcessTree: true);
                using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await _process.WaitForExitAsync(timeoutSource.Token);
            }
            catch (Exception exception) when (
                exception is InvalidOperationException or OperationCanceledException)
            {
                // La limpieza no debe ocultar el fallo original de la prueba.
            }
        }

        private async Task<string> BuildDiagnosticsAsync()
        {
            var standardOutput = await SafeReadAsync(_standardOutput);
            var standardError = await SafeReadAsync(_standardError);
            var events = string.Join(
                Environment.NewLine,
                ReadStateFile(StatePath).Select(evt => evt.Name));
            return $"stdout: {standardOutput} stderr: {standardError} eventos: {events}";
        }

        private static async Task<string> SafeReadAsync(Task<string> pending)
        {
            try
            {
                return await pending.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception) when (
                exception is TimeoutException or ObjectDisposedException or IOException)
            {
                return "<no disponible>";
            }
        }

        private static string ResolveDotnetHost()
        {
            var hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
            if (!string.IsNullOrWhiteSpace(hostPath) && File.Exists(hostPath))
            {
                return hostPath;
            }

            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath)
                && Path.GetFileNameWithoutExtension(processPath)
                    .Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                return processPath;
            }

            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrWhiteSpace(dotnetRoot))
            {
                var candidate = Path.Combine(
                    dotnetRoot,
                    OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return "dotnet";
        }
    }

    private sealed record StateEvent(string Name, IReadOnlyDictionary<string, string> Values)
    {
        public string? Value(string key) =>
            Values.TryGetValue(key, out var value) ? value : null;
    }

    private sealed record ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError,
        IReadOnlyList<StateEvent> Events,
        int Pid);

    /// <summary>
    /// Lee el fichero de evidencias mientras la aplicación lo está ampliando.
    /// Una línea incompleta se descarta en la siguiente lectura.
    /// </summary>
    private static IReadOnlyList<StateEvent> ReadStateFile(string path)
    {
        var events = new List<StateEvent>();

        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);

            while (reader.ReadLine() is { } line)
            {
                var parsed = ParseEvent(line);
                if (parsed is not null)
                {
                    events.Add(parsed);
                }
            }
        }
        catch (Exception exception) when (
            exception is FileNotFoundException
                or DirectoryNotFoundException
                or IOException)
        {
            // El arnés puede estar creando el fichero o ampliándolo: la
            // siguiente lectura verá el estado completo.
        }

        return events;
    }

    private static StateEvent? ParseEvent(string line)
    {
        var firstSeparator = line.IndexOf('|', StringComparison.Ordinal);
        if (firstSeparator < 0)
        {
            return null;
        }

        var secondSeparator = line.IndexOf('|', firstSeparator + 1);
        var name = secondSeparator < 0
            ? line[(firstSeparator + 1)..]
            : line[(firstSeparator + 1)..secondSeparator];
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (secondSeparator >= 0 && secondSeparator + 1 < line.Length)
        {
            foreach (var part in line[(secondSeparator + 1)..]
                         .Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var equals = part.IndexOf('=', StringComparison.Ordinal);
                if (equals > 0)
                {
                    values[part[..equals]] = part[(equals + 1)..];
                }
            }
        }

        return new StateEvent(name, values);
    }
}
