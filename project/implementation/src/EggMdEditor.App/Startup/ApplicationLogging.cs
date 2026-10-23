using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace EggMdEditor.App.Startup;

/// <summary>
/// Composición del registro técnico de F-001 (LOG-001..LOG-003).
/// Todos los registros salen por la salida estándar, llevan marca de tiempo
/// ISO 8601 en UTC y adjuntan el identificador de sesión; nada se escribe a
/// disco ni sale del equipo (C-001, NFR-004, NFR-005, NFR-010).
/// </summary>
public static class ApplicationLogging
{
    /// <summary>Formato ISO 8601 UTC usado por el formateador de consola.</summary>
    public const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";

    /// <summary>Categoría de los registros propios del ciclo de vida.</summary>
    public const string LifecycleCategory = "EggMdEditor.App.Startup";

    /// <summary>
    /// Crea el <see cref="ILoggerFactory"/> de la aplicación. El nivel mínimo
    /// es <see cref="LogLevel.Debug"/> para que LOG-002 pueda emitir su
    /// registro de éxito; por defecto el formateador escribe todo por la
    /// salida estándar y nunca por la salida de error.
    /// </summary>
    public static ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = TimestampFormat;
                options.UseUtcTimestamp = true;
                options.IncludeScopes = true;
                options.ColorBehavior = LoggerColorBehavior.Disabled;
            });

            // logs.md: todo el registro técnico sale por la salida estándar.
            // La salida de error se reserva a los mensajes ERR-001/ERR-002/
            // ERR-005, que escribe ApplicationLifecycle.
            builder.Services.Configure<ConsoleLoggerOptions>(options =>
                options.LogToStandardErrorThreshold = LogLevel.None);
        });
    }

    /// <summary>
    /// Envuelve un logger para que todo registro incluya el identificador de
    /// sesión como ámbito estructurado (LOG-001..LOG-003: correlación de
    /// sesión).
    /// </summary>
    public static ILogger CreateSessionLogger(ILogger logger, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        return new SessionScopedLogger(logger, sessionId);
    }

    /// <summary>
    /// Crea un logger de la categoría indicada con el identificador de sesión
    /// adjunto.
    /// </summary>
    public static ILogger CreateSessionLogger(
        ILoggerFactory loggerFactory,
        string categoryName,
        string sessionId)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        return CreateSessionLogger(loggerFactory.CreateLogger(categoryName), sessionId);
    }

    private sealed class SessionScopedLogger : ILogger
    {
        private readonly ILogger _inner;
        private readonly string _sessionId;

        public SessionScopedLogger(ILogger inner, string sessionId)
        {
            _inner = inner;
            _sessionId = sessionId;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            try
            {
                var values = new Dictionary<string, object?>(StringComparer.Ordinal);
                if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
                {
                    foreach (var (key, value) in pairs)
                    {
                        values[key] = value;
                    }
                }
                else
                {
                    values["Scope"] = state;
                }

                values["SessionId"] = _sessionId;
                return _inner.BeginScope(new SessionLogScope(values));
            }
            catch (Exception)
            {
                // Un proveedor de registro no debe impedir la operación.
                return null;
            }
        }

        public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!_inner.IsEnabled(logLevel))
            {
                return;
            }

            // Todo registro de la sesión se emite con el identificador de
            // sesión como ámbito, incluso si el llamador no abrió uno.
            IDisposable? scope = null;
            try
            {
                scope = _inner.BeginScope(new SessionLogScope(
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["SessionId"] = _sessionId,
                    }));
            }
            catch (Exception)
            {
                // El registro continúa sin ámbito si el proveedor falla.
            }

            try
            {
                _inner.Log(logLevel, eventId, state, exception, formatter);
            }
            finally
            {
                try
                {
                    scope?.Dispose();
                }
                catch (Exception)
                {
                    // La liberación del ámbito no debe alterar la operación.
                }
            }
        }
    }

    /// <summary>
    /// Ámbito estructurado con el identificador de sesión. Se expone como
    /// diccionario para los proveedores que lo consumen y como texto legible
    /// para el formateador de consola, que escribe <c>ToString()</c>.
    /// </summary>
    private sealed class SessionLogScope : IReadOnlyDictionary<string, object?>
    {
        private readonly IReadOnlyDictionary<string, object?> _values;

        public SessionLogScope(IReadOnlyDictionary<string, object?> values)
        {
            _values = values;
        }

        public object? this[string key] => _values[key];

        public IEnumerable<string> Keys => _values.Keys;

        public IEnumerable<object?> Values => _values.Values;

        public int Count => _values.Count;

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public bool TryGetValue(string key, out object? value) =>
            _values.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
            _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() =>
            string.Join(
                ", ",
                _values.Select(pair => $"{pair.Key}: {pair.Value}"));
    }
}
