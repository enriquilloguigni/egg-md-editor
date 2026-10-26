using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EggMdEditor.VisualCapture;

/// <summary>
/// Superficies de captura declaradas en design-system.md. Cada una tiene un
/// único escenario base en F-001.
/// </summary>
internal enum CaptureSurface
{
    /// <summary>SC-001: ventana principal.</summary>
    MainWindow = 0,

    /// <summary>SC-002: superficie de diálogo modal sobre la ventana principal.</summary>
    Dialog = 1,

    /// <summary>SC-003: ventana de ayuda.</summary>
    Help = 2,
}

/// <summary>
/// Estado visual de la captura: A es el estado base claro y B el estado
/// oscuro declarado en plan/mockups.md (SC-001).
/// </summary>
internal enum CaptureState
{
    Base = 0,
    Dark = 1,
}

/// <summary>
/// Petición de captura resuelta a partir de la línea de comandos:
/// <c>&lt;superficie&gt; &lt;SC&gt; [A|B] &lt;salida&gt;</c> o, sin superficie,
/// <c>&lt;SC&gt; [A|B] &lt;salida&gt;</c>. El estado también se admite como
/// sufijo del escenario (<c>SC-001:B</c>).
/// </summary>
internal readonly record struct CaptureRequest(
    CaptureSurface Surface,
    string Scenario,
    CaptureState State,
    string OutputPath)
{
    public const string Usage =
        """
        Uso:
          dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- <superficie> <SC> [A|B] <salida>
          dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- <SC> [A|B] <salida>
          dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- <superficie> <SC>:<A|B> <salida>

        Superficies: ventana-principal, diálogo, ayuda
        Escenarios: SC-001 (ventana-principal), SC-002 (diálogo), SC-003 (ayuda)
        Estados: A (claro, por defecto), B (oscuro)
        """;

    public string SurfaceName => CaptureSurfaces.CanonicalName(Surface);

    public string StateName => CaptureStates.CanonicalName(State);

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

        if (args.Length <= scenarioIndex)
        {
            error = "Faltan el escenario y la ruta de salida.";
            return false;
        }

        if (!CaptureStates.TryParseScenario(args[scenarioIndex], out var scenario, out var scenarioState))
        {
            error = $"Superficie o escenario desconocido: '{args[scenarioIndex]}'. " +
                    "Superficies válidas: ventana-principal, diálogo, ayuda; " +
                    "escenarios válidos: SC-001, SC-002, SC-003; estado: A o B.";
            return false;
        }

        var remaining = args.Length - scenarioIndex - 1;
        if (remaining is < 1 or > 2)
        {
            error = hasSurface
                ? "Se esperaban la superficie, el escenario, el estado opcional y la ruta de salida."
                : "Se esperaban el escenario, el estado opcional y la ruta de salida.";
            return false;
        }

        var state = scenarioState ?? CaptureState.Base;
        string outputPath;

        if (remaining == 1)
        {
            outputPath = args[scenarioIndex + 1];
        }
        else
        {
            var first = args[scenarioIndex + 1];
            var second = args[scenarioIndex + 2];

            // El passthrough <escenario> puede llegar vacío cuando no se
            // declara estado; en ese caso se captura el estado base.
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            {
                outputPath = string.IsNullOrEmpty(first) ? second : first;
            }
            else
            {
                var firstIsState = CaptureStates.TryParse(first, out var firstState);
                var secondIsState = CaptureStates.TryParse(second, out var secondState);

                if (firstIsState && secondIsState)
                {
                    error = "Solo se admite un estado visual (A o B).";
                    return false;
                }

                if (firstIsState)
                {
                    if (scenarioState is { } fromScenario && fromScenario != firstState)
                    {
                        error = $"El estado del escenario ('{CaptureStates.CanonicalName(fromScenario)}') " +
                                $"no coincide con el estado indicado ('{CaptureStates.CanonicalName(firstState)}').";
                        return false;
                    }

                    state = firstState;
                    outputPath = second;
                }
                else if (secondIsState)
                {
                    if (scenarioState is { } fromScenario && fromScenario != secondState)
                    {
                        error = $"El estado del escenario ('{CaptureStates.CanonicalName(fromScenario)}') " +
                                $"no coincide con el estado indicado ('{CaptureStates.CanonicalName(secondState)}').";
                        return false;
                    }

                    state = secondState;
                    outputPath = first;
                }
                else
                {
                    error = "Se esperaba un estado visual (A o B) y una única ruta de salida.";
                    return false;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            error = "La ruta de salida no puede estar vacía.";
            return false;
        }

        var expectedSurface = CaptureSurfaces.ForScenario(scenario);
        if (hasSurface && surface != expectedSurface)
        {
            error = $"La superficie '{CaptureSurfaces.CanonicalName(surface)}' no corresponde a " +
                    $"{scenario}; se esperaba '{CaptureSurfaces.CanonicalName(expectedSurface)}'.";
            return false;
        }

        request = new CaptureRequest(expectedSurface, scenario, state, outputPath);
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

/// <summary>
/// Estados visuales admitidos y su relación con el escenario de captura. El
/// estado B aplica el tema oscuro de SC-001; A es el estado base claro.
/// </summary>
internal static class CaptureStates
{
    private static readonly Regex ScenarioPattern = new(
        @"^(?<scenario>SC[-_ ]?\d{3})(?:[:#/\\-]?(?<state>[A-Za-z]+))?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool TryParse(string value, out CaptureState state)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "a":
            case "base":
            case "claro":
            case "light":
                state = CaptureState.Base;
                return true;
            case "b":
            case "oscuro":
            case "dark":
                state = CaptureState.Dark;
                return true;
            default:
                state = CaptureState.Base;
                return false;
        }
    }

    public static bool TryParseScenario(
        string value,
        out string scenario,
        out CaptureState? state)
    {
        scenario = string.Empty;
        state = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var match = ScenarioPattern.Match(value.Trim());
        if (!match.Success)
        {
            return false;
        }

        var canonical = CaptureSurfaces.NormalizeScenario(match.Groups["scenario"].Value);
        if (canonical is null)
        {
            return false;
        }

        var stateText = match.Groups["state"].Value;
        if (stateText.Length > 0)
        {
            if (!TryParse(stateText, out var parsed))
            {
                return false;
            }

            state = parsed;
        }

        scenario = canonical;
        return true;
    }

    public static string CanonicalName(CaptureState state) => state switch
    {
        CaptureState.Base => "A",
        CaptureState.Dark => "B",
        _ => throw new ArgumentOutOfRangeException(
            nameof(state),
            state,
            "Estado visual no soportado."),
    };
}
