namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Modificadores de teclado de un gesto (COM-002), normalizados al modelo de
/// atajos del Plan: el modificador primario es <c>Ctrl</c> en Windows y Linux
/// y <c>Cmd</c> en macOS, de modo que <c>Ctrl+Shift+L</c> y su forma macOS
/// <c>Cmd+Shift+L</c> son la misma combinación lógica del registro único del
/// despachador (ídem <c>Ctrl+Q</c> y <c>Cmd+Q</c>). El mapeo de las teclas
/// físicas del teclado a estos modificadores es responsabilidad del cableado
/// de la ventana (T-011).
/// </summary>
[Flags]
public enum ModificadoresDeTeclas
{
    Ninguno = 0,

    /// <summary>Modificador primario de comando: <c>Ctrl</c> (Windows/Linux) o <c>Cmd</c> (macOS).</summary>
    Primario = 1,

    /// <summary>Modificador <c>Shift</c> (Mayús).</summary>
    Mayusculas = 2,

    /// <summary>Modificador <c>Alt</c> (Option en macOS).</summary>
    Alternativa = 4,

    /// <summary>
    /// Máscara de los modificadores del modelo; los restantes bits de un
    /// gesto recibido se descartan al normalizar la combinación.
    /// </summary>
    Todos = Primario | Mayusculas | Alternativa,
}

/// <summary>
/// Combinación de teclas de un gesto de teclado (COM-002): modificadores más
/// la tecla principal, admitida como letra o dígito — las combinaciones
/// asignadas en F-001 son de esa forma y el registro único del despachador
/// será revisado con la misma forma al conectar las acciones de F-028/F-029.
/// El valor se normaliza al construirse: la tecla queda en mayúscula
/// invariable y los modificadores, reducidos a los del modelo. Una
/// combinación ausente del registro único del despachador es un atajo inerte
/// (BR-F001-003, ADR-001).
/// </summary>
public readonly record struct CombinacionDeTeclas
{
    /// <summary>Modificadores normalizados del gesto.</summary>
    public ModificadoresDeTeclas Modificadores { get; }

    /// <summary>Tecla principal en mayúscula invariable.</summary>
    public char Tecla { get; }

    /// <summary>Crea la combinación normalizada desde sus partes.</summary>
    public CombinacionDeTeclas(ModificadoresDeTeclas modificadores, char tecla)
    {
        Modificadores = modificadores & ModificadoresDeTeclas.Todos;
        Tecla = char.ToUpperInvariant(tecla);
    }

    /// <summary>Construye la combinación normalizada desde un gesto recibido.</summary>
    public static CombinacionDeTeclas De(ModificadoresDeTeclas modificadores, char tecla) =>
        new(modificadores, tecla);

    /// <summary>
    /// Representación de diagnóstico en el formato del Plan, con el
    /// modificador primario en su doble forma de plataforma.
    /// </summary>
    public override string ToString()
    {
        var partes = new List<string>(4);
        if (Modificadores.HasFlag(ModificadoresDeTeclas.Primario))
        {
            partes.Add("Ctrl/Cmd");
        }

        if (Modificadores.HasFlag(ModificadoresDeTeclas.Mayusculas))
        {
            partes.Add("Shift");
        }

        if (Modificadores.HasFlag(ModificadoresDeTeclas.Alternativa))
        {
            partes.Add("Alt");
        }

        partes.Add(Tecla.ToString());
        return string.Join("+", partes);
    }
}
