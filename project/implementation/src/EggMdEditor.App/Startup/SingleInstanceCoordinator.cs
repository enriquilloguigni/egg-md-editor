using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EggMdEditor.App.Startup;

/// <summary>
/// Garantiza una única instancia por usuario (COM-005, BR-001, A-002). La
/// primera instancia adquiere un cerrojo con nombre propio y se queda
/// escuchando solicitudes de foco; las siguientes no lo adquieren, envían la
/// solicitud de foco a la instancia existente y terminan sin abrir ventana.
/// </summary>
public sealed class SingleInstanceCoordinator : IDisposable
{
    /// <summary>Identificador estable de la aplicación para el cerrojo y el canal.</summary>
    public const string DefaultApplicationId = "EggMdEditor";

    private const string ActivationMessage = "activate";

    private static readonly TimeSpan DefaultActivationTimeout = TimeSpan.FromSeconds(2);

    private readonly Mutex? _mutex;
    private readonly string _pipeName;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task? _listenTask;
    private bool _disposed;

    private SingleInstanceCoordinator(Mutex? mutex, string pipeName, bool isPrimary)
    {
        _mutex = mutex;
        _pipeName = pipeName;
        IsPrimary = isPrimary;

        if (isPrimary)
        {
            _listenTask = Task.Run(() => ListenAsync(_shutdown.Token));
        }
    }

    /// <summary>
    /// <c>true</c> cuando esta instancia es la única que puede abrir SC-001;
    /// <c>false</c> cuando ya existe otra instancia viva con el mismo
    /// identificador de aplicación.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// COM-005: solicitud de foco recibida de otra instancia. Se notifica en
    /// el hilo del listener; el consumidor es responsable de llevarla a su
    /// hilo de interfaz.
    /// </summary>
    public event EventHandler? ActivationRequested;

    /// <summary>
    /// Adquiere el cerrojo de instancia única. La primera llamada con un
    /// identificador dado devuelve una instancia primaria; las siguientes,
    /// mientras el cerrojo siga vivo, devuelven una instancia secundaria.
    /// </summary>
    public static SingleInstanceCoordinator Acquire(string? applicationId = null)
    {
        var id = NormalizeApplicationId(applicationId);
        var instanceName = BuildInstanceName(id);
        var pipeName = BuildPipeName(id);

        try
        {
            // El objeto Mutex con nombre existe mientras haya un handle
            // abierto; no hace falta ser su propietario para que el cerrojo
            // funcione, lo que evita dependencias del hilo (release affinity).
            var mutex = new Mutex(initiallyOwned: false, instanceName, out var createdNew);
            if (createdNew)
            {
                return new SingleInstanceCoordinator(mutex, pipeName, isPrimary: true);
            }

            mutex.Dispose();
            return new SingleInstanceCoordinator(null, pipeName, isPrimary: false);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            // Sin cerrojo del sistema operativo no se puede garantizar la
            // instancia única: se degrada a instancia primaria para no
            // impedir el arranque (una única ventana en este proceso).
            return new SingleInstanceCoordinator(null, pipeName, isPrimary: true);
        }
    }

    /// <summary>
    /// COM-005: pide a la instancia primaria que enfoque su ventana. Devuelve
    /// <c>true</c> cuando la solicitud se entregó por el canal local.
    /// </summary>
    public bool TryRequestActivation(TimeSpan? timeout = null)
    {
        return SendActivationRequest(_pipeName, timeout ?? DefaultActivationTimeout);
    }

    /// <summary>
    /// Variante estática de COM-005 para el arranque de una segunda
    /// instancia, antes de que exista cualquier objeto de aplicación.
    /// </summary>
    public static bool TryRequestActivation(string? applicationId, TimeSpan? timeout = null)
    {
        var id = NormalizeApplicationId(applicationId);
        return SendActivationRequest(BuildPipeName(id), timeout ?? DefaultActivationTimeout);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _shutdown.Cancel();

        try
        {
            // El listener atiende WaitForConnectionAsync con el token de
            // apagado; se le da un margen acotado para no retrasar la salida.
            _listenTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // La cancelación del listener es el final esperado.
        }
        catch (ObjectDisposedException)
        {
            // El canal ya se cerró durante el apagado.
        }

        _shutdown.Dispose();
        _mutex?.Dispose();
    }

    private static bool SendActivationRequest(string pipeName, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        do
        {
            try
            {
                using var client = new NamedPipeClientStream(
                    ".",
                    pipeName,
                    PipeDirection.Out,
                    PipeOptions.None);
                client.Connect(250);

                using var writer = new StreamWriter(client, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                {
                    AutoFlush = true,
                };
                writer.WriteLine(ActivationMessage);
                return true;
            }
            catch (Exception ex) when (ex is IOException or TimeoutException or UnauthorizedAccessException)
            {
                // La instancia primaria puede estar creando el listener: se
                // reintenta hasta agotar el margen de la solicitud.
                Thread.Sleep(50);
            }
        }
        while (stopwatch.Elapsed < timeout);

        return false;
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                using var reader = new StreamReader(server, Encoding.UTF8);
                var message = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (string.Equals(message?.Trim(), ActivationMessage, StringComparison.Ordinal))
                {
                    RaiseActivationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException)
            {
                // El cliente se desconectó antes de completar el mensaje: se
                // vuelve a quedar a la escucha de la siguiente solicitud.
            }
            catch (UnauthorizedAccessException)
            {
                // Conexión de otro usuario: se ignora y se sigue escuchando.
            }
            catch (ObjectDisposedException)
            {
                // El canal se cerró durante el apagado de la aplicación.
                return;
            }
        }
    }

    private void RaiseActivationRequested()
    {
        try
        {
            ActivationRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            // Un fallo al entregar la solicitud de foco no debe detener el
            // canal: la instancia primaria sigue escuchando la siguiente.
        }
    }

    private static string NormalizeApplicationId(string? applicationId)
    {
        return string.IsNullOrWhiteSpace(applicationId)
            ? DefaultApplicationId
            : applicationId.Trim();
    }

    private static string BuildInstanceName(string applicationId)
    {
        return $"EggMdEditor.Instance.{BuildScopeHash(applicationId)}";
    }

    private static string BuildPipeName(string applicationId)
    {
        return $"EggMdEditor.Activation.{BuildScopeHash(applicationId)}";
    }

    /// <summary>
    /// Aísla el cerrojo y el canal por usuario: la arquitectura declara un
    /// proceso por usuario, no una instancia por máquina.
    /// </summary>
    private static string BuildScopeHash(string applicationId)
    {
        var userName = Environment.UserName;
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "default";
        }

        var scope = $"{applicationId}|{userName}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(scope));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}
