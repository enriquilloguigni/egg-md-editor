namespace EggMdEditor.App.Servicios;

/// <summary>
/// Niveles de los registros técnicos (logs.md): <c>Debug</c> para éxito
/// completo, <c>Warning</c> para éxito con degradación y <c>Error</c> para
/// fallo. Los nombres son los tokens literales que logs.md y
/// error-handling.md emplean al declarar el nivel de cada LOG-xxx.
/// </summary>
public enum NivelRegistro
{
    Debug = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
}

/// <summary>
/// Salida de los registros técnicos del proceso (NFR-003): todo registro
/// comparte el identificador de sesión, lleva marca de tiempo ISO 8601 y sale
/// por la salida estándar — nada se escribe a disco ni sale del equipo
/// (C-001). La implementación con formato y destino en la salida estándar se
/// cablea con los registros de arranque y cierre (LOG-001, LOG-003); los
/// servicios emiten el registro completo por esta interfaz.
/// </summary>
public interface ISalidaDeRegistros
{
    /// <summary>Escribe un registro ya compuesto, con su nivel.</summary>
    void Escribir(NivelRegistro nivel, string mensaje);
}

/// <summary>
/// Salida de registros nula: objeto por defecto de los servicios mientras la
/// salida real no está cableada. No escribe en ningún sitio; la salida
/// formateada a la salida estándar del proceso es <see cref="SalidaDeRegistrosConsola"/>
/// (T-007) y su cableado en SC-001 llega con T-011.
/// </summary>
public sealed class SalidaDeRegistrosNula : ISalidaDeRegistros
{
    /// <summary>Instancia única sin estado.</summary>
    public static SalidaDeRegistrosNula Instancia { get; } = new();

    private SalidaDeRegistrosNula()
    {
    }

    /// <summary>No hace nada: la salida no está cableada.</summary>
    public void Escribir(NivelRegistro nivel, string mensaje)
    {
    }
}

/// <summary>
/// Salida de registros formateada hacia la salida estándar del proceso
/// (T-007): la implementación del contrato que los registros de arranque y
/// de cierre (LOG-001, LOG-003) y el resto de servicios cablean al proceso.
/// El mensaje llega ya compuesto con su marca de tiempo ISO 8601 e
/// identificador de sesión (correlación, NFR-003); esta salida añade el
/// token literal del nivel (<c>Debug</c>, <c>Info</c>, <c>Warning</c>,
/// <c>Error</c>, los tokens de logs.md) y escribe una línea por registro.
/// Nada se escribe a disco ni sale del equipo (C-001). El escritor es
/// inyectable para las pruebas; por defecto es la salida estándar.
/// </summary>
public sealed class SalidaDeRegistrosConsola : ISalidaDeRegistros
{
    private readonly TextWriter _escritor;

    /// <summary>
    /// Crea la salida sobre el escritor dado; sin escritor, sobre la salida
    /// estándar del proceso (<see cref="Console.Out"/>).
    /// </summary>
    public SalidaDeRegistrosConsola(TextWriter? escritor = null) =>
        _escritor = escritor ?? Console.Out;

    /// <summary>Escribe el registro como una línea con su nivel delante.</summary>
    public void Escribir(NivelRegistro nivel, string mensaje)
    {
        ArgumentNullException.ThrowIfNull(mensaje);

        _escritor.WriteLine($"[{nivel}] {mensaje}");
    }
}
