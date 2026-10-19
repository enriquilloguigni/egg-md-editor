using System.IO;
using System.Runtime.InteropServices;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Errores y registros de arranque y de cierre del proceso (T-007):
/// componente del núcleo que materializa LOG-001 y LOG-003 y el manejo de
/// ERR-001, ERR-002 y ERR-005, sin tocar la interfaz — el cableado real del
/// arranque y del cierre de SC-001 corresponde a T-011 y T-012.
///
/// <para>
/// Arranque (LOG-001): registra el inicio del proceso y el fin de la
/// inicialización (ventana SC-001 creada, catálogo i18n cargado, idioma
/// activo), con la versión de la aplicación y del runtime .NET, el idioma
/// activo y el tiempo de arranque en ms. Cierre (LOG-003): registra el inicio
/// del cierre y el fin del proceso, con la duración total de la sesión en ms.
/// Todo registro comparte el identificador de sesión, lleva marca de tiempo
/// ISO 8601 y sale por la salida estándar — nada se escribe a disco ni sale
/// del equipo (C-001, NFR-003).
/// </para>
///
/// <para>
/// Errores (ERR-001, ERR-002, ERR-005): ante un fallo durante la
/// inicialización del proceso o la creación de SC-001 (ERR-001), al pintarla
/// durante el arranque (ERR-002) o al cerrarla y liberar la sesión (ERR-005),
/// escribe en la salida estándar de error el mensaje en español de la Spec y
/// registra el fallo con nivel <see cref="NivelRegistro.Error"/>, con el tipo
/// y el mensaje de la excepción. El proceso debe finalizar con el código de
/// error devuelto (<see cref="CodigoSalidaPorFallo"/>), sin procesos
/// residuales y, en el arranque, sin presentar ninguna ventana: aplicar ese
/// código y garantizar la finalización es del cableado (T-011, T-012), no de
/// este componente, que no termina el proceso por su cuenta.
/// </para>
/// </summary>
public sealed class RegistradorDeCicloDeVida
{
    /// <summary>
    /// Código de salida del proceso ante un fallo de arranque o de cierre
    /// (ERR-001, ERR-002, ERR-005): el usuario vuelve a lanzar la aplicación
    /// desde el código fuente (A-001); no hay estado que deshacer.
    /// </summary>
    public const int CodigoSalidaPorFallo = 1;

    private readonly SesionDeAplicacion _sesion;
    private readonly ISalidaDeRegistros _salidaDeRegistros;
    private readonly TextWriter _salidaDeError;
    private readonly Func<DateTimeOffset> _reloj;
    private readonly string _versionDeAplicacion;
    private readonly string _versionDeRuntime;

    /// <summary>
    /// Instante de referencia para el tiempo de arranque (LOG-001) y la
    /// duración total de la sesión (LOG-003): el de la creación de este
    /// componente, que acompaña a la creación de la sesión.
    /// </summary>
    private readonly DateTimeOffset _instanteDeArranque;

    /// <summary>
    /// Crea el registrador sobre la sesión de la aplicación. La salida de
    /// registros por defecto es la formateada a la salida estándar del
    /// proceso (<see cref="SalidaDeRegistrosConsola"/>) y la de error, la
    /// salida estándar de error; el reloj y las versiones de aplicación y de
    /// runtime son inyectables para un registro determinista en pruebas.
    /// </summary>
    public RegistradorDeCicloDeVida(
        SesionDeAplicacion sesion,
        ISalidaDeRegistros? salidaDeRegistros = null,
        TextWriter? salidaDeError = null,
        Func<DateTimeOffset>? reloj = null,
        string? versionDeAplicacion = null,
        string? versionDeRuntime = null)
    {
        _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
        _salidaDeRegistros = salidaDeRegistros ?? new SalidaDeRegistrosConsola();
        _salidaDeError = salidaDeError ?? Console.Error;
        _reloj = reloj ?? (() => DateTimeOffset.UtcNow);
        _versionDeAplicacion = string.IsNullOrWhiteSpace(versionDeAplicacion)
            ? VersionDeAplicacionPorDefecto()
            : versionDeAplicacion;
        _versionDeRuntime = string.IsNullOrWhiteSpace(versionDeRuntime)
            ? RuntimeInformation.FrameworkDescription
            : versionDeRuntime;
        _instanteDeArranque = _reloj();
    }

    /// <summary>Versión de la aplicación incluida en LOG-001.</summary>
    public string VersionDeAplicacion => _versionDeAplicacion;

    /// <summary>Versión del runtime .NET incluida en LOG-001.</summary>
    public string VersionDeRuntime => _versionDeRuntime;

    /// <summary>
    /// Registra el inicio del proceso (LOG-001, <see cref="NivelRegistro.Info"/>):
    /// hora, sesión, versión de la aplicación y del runtime .NET e idioma
    /// activo, español por defecto de sesión (BR-007).
    /// </summary>
    public void RegistrarInicioDeProceso() =>
        Registrar(
            NivelRegistro.Info,
            "LOG-001: inicio del proceso; versión de la aplicación " +
            $"{_versionDeAplicacion}; runtime {_versionDeRuntime}; " +
            $"idioma activo {CodigoDeIdiomaActivo} (defecto de sesión, BR-007).");

    /// <summary>
    /// Registra el fin de la inicialización (LOG-001,
    /// <see cref="NivelRegistro.Info"/>): ventana SC-001 creada, catálogo i18n
    /// cargado e idioma activo, con el tiempo de arranque en ms. El cableado
    /// lo invoca solo si la ventana se creó y se pintó completa; un fallo en
    /// ese camino se registra con <see cref="RegistrarFalloDeArranque"/>.
    /// </summary>
    public void RegistrarFinDeInicializacion() =>
        Registrar(
            NivelRegistro.Info,
            "LOG-001: fin de la inicialización: ventana SC-001 creada, catálogo " +
            $"i18n cargado e idioma activo {CodigoDeIdiomaActivo}; " +
            $"tiempo de arranque {MilisegundosDesde(_instanteDeArranque)} ms.");

    /// <summary>
    /// Registra un fallo del arranque (ERR-001: inicialización del proceso o
    /// creación de SC-001; ERR-002: excepción al pintarla, tras su creación):
    /// escribe en la salida estándar de error, en español, «Error al iniciar
    /// la aplicación: <c>tipo y mensaje de la excepción</c>», registra
    /// LOG-001 con nivel <see cref="NivelRegistro.Error"/> y devuelve el
    /// código de error con el que el proceso debe finalizar, sin procesos
    /// residuales y sin presentar ninguna ventana.
    /// </summary>
    public int RegistrarFalloDeArranque(Exception excepcion)
    {
        ArgumentNullException.ThrowIfNull(excepcion);

        var tipoYMensaje = TipoYMensajeDe(excepcion);
        _salidaDeError.WriteLine($"Error al iniciar la aplicación: {tipoYMensaje}");
        Registrar(
            NivelRegistro.Error,
            $"LOG-001: arranque fallido: {tipoYMensaje}; no se presenta ninguna " +
            $"ventana y el proceso finaliza con código de error {CodigoSalidaPorFallo}, " +
            "sin procesos residuales (ERR-001, ERR-002).");

        return CodigoSalidaPorFallo;
    }

    /// <summary>
    /// Registra el inicio del cierre de SC-001 y de la liberación de los
    /// recursos de la sesión (LOG-003, <see cref="NivelRegistro.Info"/>, COM-004).
    /// </summary>
    public void RegistrarInicioDeCierre() =>
        Registrar(
            NivelRegistro.Info,
            "LOG-003: inicio del cierre de SC-001 y de la liberación de los " +
            "recursos de la sesión (COM-004).");

    /// <summary>
    /// Registra el fin del proceso (LOG-003, <see cref="NivelRegistro.Info"/>):
    /// cierre limpio, sin procesos residuales, con la duración total de la
    /// sesión en ms. La elección de idioma no se registra como preferencia:
    /// no se persiste en F-001 (FR-013 → F-030).
    /// </summary>
    public void RegistrarFinDeProceso() =>
        Registrar(
            NivelRegistro.Info,
            "LOG-003: fin del proceso; cierre limpio sin procesos residuales; " +
            $"duración total de la sesión {MilisegundosDesde(_instanteDeArranque)} ms.");

    /// <summary>
    /// Registra un fallo al cerrar SC-001 o al liberar los recursos de la
    /// sesión (ERR-005): escribe en la salida estándar de error, en español,
    /// «Error al cerrar la aplicación: <c>tipo y mensaje de la excepción</c>»,
    /// registra LOG-003 con nivel <see cref="NivelRegistro.Error"/> y devuelve
    /// el código de error con el que el proceso se termina igualmente,
    /// garantizando que no queden procesos residuales; la elección de idioma
    /// no se persiste (BR-F001-006, FR-013 → F-030).
    /// </summary>
    public int RegistrarFalloDeCierre(Exception excepcion)
    {
        ArgumentNullException.ThrowIfNull(excepcion);

        var tipoYMensaje = TipoYMensajeDe(excepcion);
        _salidaDeError.WriteLine($"Error al cerrar la aplicación: {tipoYMensaje}");
        Registrar(
            NivelRegistro.Error,
            $"LOG-003: cierre fallido: {tipoYMensaje}; el proceso se termina " +
            "igualmente, garantizando que no queden procesos residuales (ERR-005).");

        return CodigoSalidaPorFallo;
    }

    private string CodigoDeIdiomaActivo => CatalogoCadenasI18n.CodigoDe(_sesion.IdiomaActivo);

    /// <summary>
    /// Tipo y mensaje de la excepción, como exige el mensaje de error de la
    /// Spec. El separador interno de reflexión de los tipos anidados se
    /// normaliza a punto, para una salida legible para el usuario.
    /// </summary>
    private static string TipoYMensajeDe(Exception excepcion) =>
        $"{excepcion.GetType().ToString().Replace('+', '.')}: {excepcion.Message}";

    /// <summary>
    /// Milisegundos enteros transcurridos desde el instante dado, con el reloj
    /// inyectado; nunca negativos ante un reloj que retrocede.
    /// </summary>
    private long MilisegundosDesde(DateTimeOffset instante) =>
        Math.Max(0, (long)Math.Round((_reloj() - instante).TotalMilliseconds, MidpointRounding.AwayFromZero));

    /// <summary>
    /// Versión de la aplicación por defecto: la del ensamblado de la aplicación.
    /// </summary>
    private static string VersionDeAplicacionPorDefecto() =>
        typeof(RegistradorDeCicloDeVida).Assembly.GetName().Version?.ToString()
            ?? "desconocida";

    /// <summary>
    /// Registra por la salida de registros: marca de tiempo ISO 8601 e
    /// identificador de sesión compartidos por todo registro (logs.md,
    /// NFR-003), sin escritura a disco ni salida del equipo (C-001).
    /// </summary>
    private void Registrar(NivelRegistro nivel, string mensaje) =>
        _salidaDeRegistros.Escribir(
            nivel,
            $"{_reloj().ToString("O")} [sesión {_sesion.IdentificadorDeSesion}] {mensaje}");
}
