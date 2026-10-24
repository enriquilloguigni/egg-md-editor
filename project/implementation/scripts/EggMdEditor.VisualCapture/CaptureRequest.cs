using System;
using System.Globalization;
using System.Text;

namespace EggMdEditor.VisualCapture;

/// <summary>
/// Superficies de captura declaradas en design-system.md. Cada una tiene un
/// único escenario base en F-001.
/// </summary>
internal enum CaptureSurface
{
    /// <summary>SC-001: ventana principal en estado base.</summary>
    MainWindow = 0,

    /// <summary>SC-002: superficie de diálogo modal sobre la ventana principal.</summary>
    Dialog = 1,

    /// <summary>SC-003: ventana de ayuda en estado base.</summary>
    Help = 2,
}

/// <summary>
/// Petición de captura resuelta a partir de la línea de comandos:
/// <c>&lt;superficie&gt; &lt;SC&gt; &lt;salida&gt;</c> o, sin superficie,
/// <c>&lt;SC&gt; &lt;salida&gt;</c>.
/// </summary>
internal readonly record struct CaptureRequest(
    CaptureSurface Surface,
    string Scenario,
    string OutputPath)
{
    public const string Usage =
        """
        Uso:
          dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- <superficie> <SC> <salida>
          dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- <SC> <salida>

        Superficies: ventana-principal, diálogo, ayuda
        Escenarios: SC-001 (ventana-principal), SC-002 (diálogo), SC-003 (ayuda)
        """;

    public string SurfaceName => CaptureSurfaces.CanonicalName(Surface);

    public static bool TryParse(string[] args, out CaptureRequest request, out string? error)
    {
        ArgumentNullException.ThrowIfNull(args);

        request = default;
        error = null;

        if (args.Length == 0)
        {
            error = "Faltan argumentos.";
            return false;
        }

        var hasSurface = CaptureSurfaces.TryParse(args[0], out var surface);
        var scenarioIndex = hasSurface ? 1 : 0;

        if (!hasSurface && CaptureSurfaces.NormalizeScenario(args[0]) is null)
        {
            error = $"Superficie o escenario desconocido: '{args[0]}'. " +
                    "Superficies válidas: ventana-principal, diálogo, ayuda; " +
                    "escenarios válidos: SC-001, SC-002, SC-003.";
            return false;
        }

        if (args.Length - scenarioIndex != 2)
        {
            error = hasSurface
                ? "Se esperaban la superficie, el escenario y la ruta de salida."
                : "Se esperaban el escenario y la ruta de salida.";
            return false;
        }

        var scenario = CaptureSurfaces.NormalizeScenario(args[scenarioIndex]);
        if (scenario is null)
        {
            error = $"Escenario desconocido: '{args[scenarioIndex]}'. " +
                    "Escenarios válidos: SC-001, SC-002, SC-003.";
            return false;
        }

        var expectedSurface = CaptureSurfaces.ForScenario(scenario);
        if (hasSurface && surface != expectedSurface)
        {
            error = $"La superficie '{CaptureSurfaces.CanonicalName(surface)}' no corresponde a " +
                    $"{scenario}; se esperaba '{CaptureSurfaces.CanonicalName(expectedSurface)}'.";
            return false;
        }

        var outputPath = args[scenarioIndex + 1];
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            error = "La ruta de salida no puede estar vacía.";
            return false;
        }

        request = new CaptureRequest(expectedSurface, scenario, outputPath);
        return true;
    }
}

/// <summary>
/// Correspondencia única entre superficie y escenario. Las superficies se
/// aceptan con o sin tilde para que la invocación desde consola no dependa de
/// la normalización del terminal.
/// </summary>
internal static class CaptureSurfaces
{
    public static bool TryParse(string value, out CaptureSurface surface)
    {
        switch (NormalizeName(value).Replace('_', '-').Replace(' ', '-'))
        {
            case "ventana-principal":
                surface = CaptureSurface.MainWindow;
                return true;
            case "dialogo":
                surface = CaptureSurface.Dialog;
                return true;
            case "ayuda":
                surface = CaptureSurface.Help;
                return true;
            default:
                surface = default;
                return false;
        }
    }

    public static string? NormalizeScenario(string value)
    {
        var compact = NormalizeName(value)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();

        return compact switch
        {
            "SC001" => "SC-001",
            "SC002" => "SC-002",
            "SC003" => "SC-003",
            _ => null,
        };
    }

    public static CaptureSurface ForScenario(string scenario) => scenario switch
    {
        "SC-001" => CaptureSurface.MainWindow,
        "SC-002" => CaptureSurface.Dialog,
        "SC-003" => CaptureSurface.Help,
        _ => throw new ArgumentOutOfRangeException(
            nameof(scenario),
            scenario,
            "Escenario no soportado."),
    };

    public static string CanonicalName(CaptureSurface surface) => surface switch
    {
        CaptureSurface.MainWindow => "ventana-principal",
        CaptureSurface.Dialog => "diálogo",
        CaptureSurface.Help => "ayuda",
        _ => throw new ArgumentOutOfRangeException(
            nameof(surface),
            surface,
            "Superficie no soportada."),
    };

    private static string NormalizeName(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
