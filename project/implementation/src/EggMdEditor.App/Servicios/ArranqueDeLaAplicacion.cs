using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Configuración de ejecución del proceso (T-012): secuencia de arranque de
/// la aplicación, fijada en el punto de entrada — <c>Program.Main</c> — con
/// estas propiedades invariables:
///
/// <list type="bullet">
/// <item>El proceso arranca siempre en español: la sesión se crea nueva y
/// efímera con sus defectos de la Spec (BR-F001-004, BR-007) y no se lee
/// ninguna preferencia de ningún sitio — ni del disco ni de variables de
/// entorno —, de modo que la elección de una sesión anterior nunca
/// condiciona un arranque nuevo (BR-F001-006, AC-001, AC-011).</item>
/// <item>Nada del arranque escribe a disco ni accede a la red (C-001): los
/// registros del ciclo de vida salen por la salida estándar del proceso
/// (T-007, NFR-003).</item>
/// <item>El comando de verificación es <c>dotnet test</c> en la raíz del
/// árbol de implementación (solution <c>egg-md-editor.slnx</c>).</item>
/// </list>
///
/// <para>
/// La secuencia es la de LOG-001 (T-007): registrar el inicio del proceso y,
/// cuando SC-001 se crea y se presenta completa, el fin de la inicialización
/// — ese segundo registro lo emite el cableado de la ventana
/// (<see cref="App"/>, T-012)—. Si la interfaz falla al arrancar, sea en la
/// inicialización del proceso o la creación de SC-001 (ERR-001), sea en su
/// pintado tras la creación (ERR-002), el fallo se registra y se devuelve el
/// código de error con el que el proceso debe finalizar, sin presentar
/// ninguna ventana y sin procesos residuales; la excepción no se relanza.
/// Con el arranque correcto, el código de salida es el que el cierre de la
/// ventana (COM-004, T-011) haya dejado en el proceso: 0 en cierre limpio o
/// el de ERR-005 ante fallo de cierre.
/// </para>
///
/// <para>
/// El registrador del ciclo de vida es opcional — como en el resto de
/// servicios, se crea sobre la sesión dada si no llega — y el lector del
/// código de salida del proceso es inyectable para pruebas deterministas.
/// </para>
/// </summary>
public sealed class ArranqueDeLaAplicacion
{
    private readonly RegistradorDeCicloDeVida _registradorDeCicloDeVida;
    private readonly Func<int> _lectorDeCodigoDeSalida;

    /// <summary>
    /// Crea la configuración de ejecución sobre la sesión nueva del proceso.
    /// Sin registrador, se crea uno por defecto sobre la sesión; sin lector
    /// de código de salida, se lee el del proceso
    /// (<see cref="Environment.ExitCode"/>, fijado por el cierre de T-011).
    /// </summary>
    public ArranqueDeLaAplicacion(
        SesionDeAplicacion sesion,
        RegistradorDeCicloDeVida? registradorDeCicloDeVida = null,
        Func<int>? lectorDeCodigoDeSalida = null)
    {
        ArgumentNullException.ThrowIfNull(sesion);

        Sesion = sesion;
        _registradorDeCicloDeVida = registradorDeCicloDeVida ?? new RegistradorDeCicloDeVida(sesion);
        _lectorDeCodigoDeSalida = lectorDeCodigoDeSalida ?? (() => Environment.ExitCode);
    }

    /// <summary>Sesión nueva y efímera del arranque: siempre en español (BR-F001-006).</summary>
    public SesionDeAplicacion Sesion { get; }

    /// <summary>
    /// Registrador del ciclo de vida del proceso (T-007): emite LOG-001 en el
    /// arranque y comparte la sesión con la ventana única, para que su cierre
    /// emita LOG-003 y aplique ERR-005 (COM-004, T-011).
    /// </summary>
    public RegistradorDeCicloDeVida RegistradorDeCicloDeVida => _registradorDeCicloDeVida;

    /// <summary>
    /// Ejecuta el arranque del proceso: registra el inicio (LOG-001) y lanza
    /// la interfaz — el delegado que construye y ejecuta Avalonia —. Si la
    /// interfaz falla, aplica ERR-001/ERR-002 con el registrador y devuelve
    /// el código de error; si arranca, devuelve el código de salida que el
    /// cierre (COM-004) haya dejado en el proceso. En ambos casos el proceso
    /// finaliza sin procesos residuales.
    /// </summary>
    public int Ejecutar(Action arrancarLaInterfaz)
    {
        ArgumentNullException.ThrowIfNull(arrancarLaInterfaz);

        // LOG-001: inicio del proceso — versión de la aplicación y del
        // runtime, e idioma activo español por defecto de la sesión (BR-007).
        _registradorDeCicloDeVida.RegistrarInicioDeProceso();

        try
        {
            arrancarLaInterfaz();
        }
        catch (Exception excepcion)
        {
            // ERR-001 / ERR-002: no se presenta ninguna ventana —ninguna
            // usable—, el mensaje en español sale por la salida de error, el
            // fallo queda registrado (LOG-001, nivel Error) y el proceso
            // finaliza con código de error, sin procesos residuales.
            return _registradorDeCicloDeVida.RegistrarFalloDeArranque(excepcion);
        }

        // Arranque correcto (LOG-001 fin de la inicialización emitido por la
        // ventana al presentarse completa): el código de salida lo dejó el
        // cierre (COM-004, T-011) — 0 en cierre limpio o el de ERR-005.
        return _lectorDeCodigoDeSalida();
    }
}
