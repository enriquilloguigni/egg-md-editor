using System.Runtime.InteropServices;
using Avalonia.Input;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Traducción de gestos de teclado físicos al modelo de atajos (COM-002,
/// T-011): convierte la tecla y los modificadores que reporta la entrada de
/// teclado de SC-001 en la <see cref="CombinacionDeTeclas"/> del registro
/// único del despachador —su traducción es responsabilidad del cableado de la
/// ventana (COM-002)— o en nada si el gesto no es expresable en el modelo.
///
/// <para>
/// El modificador primario es <c>Ctrl</c> en Windows y Linux y <c>Cmd</c>
/// (⌘) en macOS, según el modelo del Plan: en Windows y Linux traduce
/// <c>Control</c>, y en macOS <c>Meta</c>; el otro no es primario y se
/// descarta —por ejemplo, <c>Ctrl+Q</c> físico en macOS no es el atajo de
/// cierre asignado, que es <c>Cmd+Q</c>—. <c>Shift</c> traduce a
/// <see cref="ModificadoresDeTeclas.Mayusculas"/> y <c>Alt</c> a
/// <see cref="ModificadoresDeTeclas.Alternativa"/>; cualquier otro
/// modificador no pertenece al modelo y se descarta. La tecla principal es
/// una letra (A–Z) o un dígito (números del bloque principal o del teclado
/// numérico) — la forma de las combinaciones asignadas en F-001 y de las que
/// conecten F-028/F-029, revisadas sobre el mismo registro único—; el resto
/// de teclas no es expresable y da gesto sin traducción, que el cableado
/// trata como atajo inerte (BR-F001-003, ADR-001).
/// </para>
/// </summary>
public static class TraductorDeGestos
{
    /// <summary>
    /// Traduce un gesto de teclado recibido de la ventana, según la plataforma
    /// en la que corre el proceso (macOS frente a Windows/Linux).
    /// </summary>
    public static CombinacionDeTeclas? Traducir(Key tecla, KeyModifiers modificadores) =>
        Traducir(tecla, modificadores, EnMacOS());

    /// <summary>
    /// Traduce un gesto con la plataforma dada, para un cableado
    /// determinista en pruebas: <c>enMacOS</c> elige qué modificador físico
    /// es el primario del modelo (<c>Meta</c> en macOS, <c>Control</c> en
    /// Windows y Linux).
    /// </summary>
    public static CombinacionDeTeclas? Traducir(Key tecla, KeyModifiers modificadores, bool enMacOS)
    {
        var caracter = CaracterDe(tecla);

        if (caracter is null)
        {
            return null;
        }

        var modificadoresDelModelo = ModificadoresDeTeclas.Ninguno;

        var primarioFisico = enMacOS ? KeyModifiers.Meta : KeyModifiers.Control;
        if (modificadores.HasFlag(primarioFisico))
        {
            modificadoresDelModelo |= ModificadoresDeTeclas.Primario;
        }

        if (modificadores.HasFlag(KeyModifiers.Shift))
        {
            modificadoresDelModelo |= ModificadoresDeTeclas.Mayusculas;
        }

        if (modificadores.HasFlag(KeyModifiers.Alt))
        {
            modificadoresDelModelo |= ModificadoresDeTeclas.Alternativa;
        }

        return new CombinacionDeTeclas(modificadoresDelModelo, caracter.Value);
    }

    /// <summary>¿Corre el proceso en macOS? El primario físico del modelo depende de ello.</summary>
    private static bool EnMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Carácter de la tecla principal si es una letra (A–Z), un dígito del
    /// bloque principal (D0–D9) o un dígito del teclado numérico
    /// (NumPad0–NumPad9), en mayúscula invariable; nulo para cualquier otra
    /// tecla, que no es expresable como combinación del modelo.
    /// </summary>
    private static char? CaracterDe(Key tecla)
    {
        if (tecla is >= Key.A and <= Key.Z)
        {
            return (char)('A' + tecla - Key.A);
        }

        if (tecla is >= Key.D0 and <= Key.D9)
        {
            return (char)('0' + tecla - Key.D0);
        }

        if (tecla is >= Key.NumPad0 and <= Key.NumPad9)
        {
            return (char)('0' + tecla - Key.NumPad0);
        }

        return null;
    }
}
