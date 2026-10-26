using System;
using System.Globalization;
using System.IO;

namespace EggMdEditor.App.Startup;

/// <summary>
/// Arnés de pruebas E2E de T-018 (AC-005, AC-006 y AC-007). Solo se activa
/// cuando la variable de entorno <c>EGGMD_TEST_MODE</c> tiene valor; sin ella
/// la aplicación se comporta exactamente igual que sin este componente.
///
/// El arnés no forma parte del comportamiento funcional de F-001: permite
/// observar a nivel de proceso la creación y presentación de SC-001, las
/// solicitudes de activación de una segunda instancia, los fallos de arranque
/// y el cierre sin persistencia, sin depender de un servidor gráfico
/// (Avalonia headless). Toda su salida se escribe en un fichero indicado por
/// <c>EGGMD_TEST_STATE_FILE</c>, nunca en la salida normal de la aplicación.
///
/// Variables reconocidas (solo con <c>EGGMD_TEST_MODE</c> activo):
/// <list type="bullet">
///   <item><c>EGGMD_TEST_APPLICATION_ID</c>: identificador de instancia única
///   aislado por prueba.</item>
///   <item><c>EGGMD_TEST_STATE_FILE</c>: ruta del fichero de evidencias.</item>
///   <item><c>EGGMD_TEST_FAIL_STARTUP</c>: simula ERR-001 antes de crear SC-001.</item>
///   <item><c>EGGMD_TEST_FAIL_RENDER</c>: simula ERR-002 tras crear SC-001 y
///   antes de presentarla por completo.</item>
///   <item><c>EGGMD_TEST_MINIMIZE_ON_READY</c>: deja SC-001 minimizada tras
///   presentarla, para observar la restauración de foco de AC-005.</item>
///   <item><c>EGGMD_TEST_SWITCH_LANGUAGE</c>: cambia el idioma en caliente
///   antes de cerrar (AC-007, no persistencia).</item>
///   <item><c>EGGMD_TEST_SWITCH_THEME</c>: cambia el tema en caliente antes
///   de cerrar (AC-007, no persistencia).</item>
///   <item><c>EGGMD_TEST_CLOSE_AFTER_MS</c>: cierra SC-001 tras ese margen,
///   usando el mismo camino que el cierre de la ventana.</item>
/// </list>
/// </summary>
internal static class StartupTestMode
{
    public const string ModeVariable = "EGGMD_TEST_MODE";
    public const string ApplicationIdVariable = "EGGMD_TEST_APPLICATION_ID";
    public const string StateFileVariable = "EGGMD_TEST_STATE_FILE";
    public const string FailStartupVariable = "EGGMD_TEST_FAIL_STARTUP";
    public const string FailRenderVariable = "EGGMD_TEST_FAIL_RENDER";
    public const string MinimizeOnReadyVariable = "EGGMD_TEST_MINIMIZE_ON_READY";
    public const string SwitchLanguageVariable = "EGGMD_TEST_SWITCH_LANGUAGE";
    public const string SwitchThemeVariable = "EGGMD_TEST_SWITCH_THEME";
    public const string CloseAfterVariable = "EGGMD_TEST_CLOSE_AFTER_MS";

    /// <summary>
    /// Mensaje de la excepción simulada de ERR-001. El mensaje de usuario que
    /// exige la Spec lo compone <see cref="ApplicationLifecycle"/> en español.
    /// </summary>
    public const string StartupFailureMessage =
        "fallo simulado de inicialización de SC-001 (arnés E2E)";

    /// <summary>
    /// Mensaje de la excepción simulada de ERR-002 (ventana creada que no
    /// llega a presentarse completa).
    /// </summary>
    public const string RenderFailureMessage =
        "fallo simulado de pintado de SC-001 (arnés E2E)";

    private static readonly object Sync = new();

    /// <summary>
    /// <c>true</c> cuando el proceso se ejecuta bajo el arnés E2E. En una
    /// ejecución normal todas las propiedades de esta clase devuelven sus
    /// valores por defecto y no se escribe ningún fichero.
    /// </summary>
    public static bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ModeVariable));

    /// <summary>Identificador de instancia única aislado para la prueba actual.</summary>
    public static string? ApplicationId => GetValue(ApplicationIdVariable);

    /// <summary>ERR-001 simulado antes de crear SC-001.</summary>
    public static bool FailStartup => IsTruthy(FailStartupVariable);

    /// <summary>ERR-002 simulado después de crear SC-001 y antes de presentarla.</summary>
    public static bool FailRender => IsTruthy(FailRenderVariable);

    /// <summary>Deja SC-001 minimizada tras presentarla (AC-005).</summary>
    public static bool MinimizeOnReady => IsTruthy(MinimizeOnReadyVariable);

    /// <summary>Idioma a conmutar tras presentar SC-001 (AC-007).</summary>
    public static string? SwitchLanguage => GetValue(SwitchLanguageVariable);

    /// <summary>Preferencia de tema a conmutar tras presentar SC-001 (AC-007).</summary>
    public static string? SwitchTheme => GetValue(SwitchThemeVariable);

    /// <summary>Margen para cerrar SC-001 tras presentarla, o <c>null</c>.</summary>
    public static int? CloseAfterMilliseconds
    {
        get
        {
            var value = GetValue(CloseAfterVariable);
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds)
                && milliseconds > 0
                    ? milliseconds
                    : null;
        }
    }

    /// <summary>
    /// Añade una línea de evidencia al fichero de estado. Es seguro entre
    /// hilos y nunca propaga errores: el diagnóstico de prueba no debe alterar
    /// el comportamiento de la aplicación.
    /// </summary>
    public static void Record(string eventName, string? details = null)
    {
        if (!IsEnabled)
        {
            return;
        }

        var path = GetValue(StateFileVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var line = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            + "|"
            + eventName;
        if (!string.IsNullOrEmpty(details))
        {
            line += "|" + details;
        }

        lock (Sync)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var stream = new FileStream(
                    fullPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(line);
                writer.Flush();
            }
            catch (Exception)
            {
                // El arnés de pruebas nunca debe alterar el resultado de la
                // aplicación bajo prueba.
            }
        }
    }

    private static string? GetValue(string variableName) =>
        IsEnabled ? Environment.GetEnvironmentVariable(variableName) : null;

    private static bool IsTruthy(string variableName)
    {
        var value = GetValue(variableName);
        return value is not null
            && (value.Equals("1", StringComparison.Ordinal)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }
}
