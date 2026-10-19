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
/// salida real no está cableada. No escribe en ningún sitio; la salida por la
/// salida estándar del proceso se conecta con los registros de arranque y de
/// cierre (T-007) y en el cableado de SC-001 (T-011).
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
