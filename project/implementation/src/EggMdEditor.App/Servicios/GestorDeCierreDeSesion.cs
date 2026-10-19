using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Cierre de SC-001 con liberación de la sesión (COM-004, T-011): contraparte
/// de la orden de cierre que finaliza la sesión liberando sus datos, síncrona
/// y en proceso.
///
/// <para>
/// La secuencia de cierre es la de LOG-003 (T-007): registrar el inicio del
/// cierre y de la liberación de los recursos de la sesión, liberar y
/// registrar el fin del proceso con la duración total de la sesión. Liberar
/// es soltar los datos de la sesión: estado efímero, sin recursos externos
/// ni escritura a disco (C-001) — la elección de idioma no sobrevive a la
/// sesión (BR-F001-006, FR-013 → F-030). La confirmación de liberación queda
/// en <see cref="SesionLiberada"/>, que el cierre de la ventana expone como
/// confirmación del canal COM-004; con la sesión liberada, la sesión ha
/// terminado (ST-004) y cesan todos los canales internos.
/// </para>
///
/// <para>
/// Si al cerrar o liberar se produce una excepción, se aplica ERR-005: el
/// registrador escribe en la salida estándar de error «Error al cerrar la
/// aplicación: <c>tipo y mensaje de la excepción</c>» y registra el fallo
/// (LOG-003, nivel Error); el proceso se termina igualmente — sin procesos
/// residuales — con el código de error que este componente expone en
/// <see cref="CodigoDeSalidaPendiente"/> para que el cableado lo aplique al
/// proceso (T-012). La elección de idioma tampoco se persiste en este camino
/// (BR-F001-006).
/// </para>
/// </summary>
public sealed class GestorDeCierreDeSesion
{
    private readonly RegistradorDeCicloDeVida? _registradorDeCicloDeVida;

    /// <summary>
    /// Crea el gestor de cierre sobre la sesión de la aplicación. El
    /// registrador del ciclo de vida es opcional: con él, el cierre registra
    /// LOG-003 y aplica ERR-005 ante fallo; sin él, el cierre solo libera y
    /// confirma.
    /// </summary>
    public GestorDeCierreDeSesion(SesionDeAplicacion sesion, RegistradorDeCicloDeVida? registradorDeCicloDeVida = null)
    {
        Sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
        _registradorDeCicloDeVida = registradorDeCicloDeVida;
    }

    /// <summary>
    /// Sesión de la aplicación que este gestor cierra. Tras una liberación
    /// confirmada queda expuesta solo como referencia de la sesión terminada
    /// (ST-004): sus datos no se persisten y no sobreviven a la sesión.
    /// </summary>
    public SesionDeAplicacion Sesion { get; }

    /// <summary>
    /// Confirmación de liberación de los datos de la sesión (COM-004):
    /// <c>true</c> desde que <see cref="Cerrar"/> completa la liberación.
    /// Con la sesión liberada, la elección de idioma queda descartada — no
    /// se persiste (FR-013 → F-030, BR-F001-006)—.
    /// </summary>
    public bool SesionLiberada { get; private set; }

    /// <summary>
    /// Código de salida con el que el proceso debe finalizar si el cierre
    /// falló (ERR-005): <c>0</c> mientras no haya fallo aplicado.
    /// </summary>
    public int CodigoDeSalidaPendiente { get; private set; }

    /// <summary>
    /// Ejecuta el cierre de la sesión (COM-004): registra el inicio del
    /// cierre (LOG-003), libera los datos de la sesión — efímeros, sin
    /// persistencia (FR-013) —, confirma la liberación y registra el fin del
    /// proceso (LOG-003). Ante una excepción en cualquiera de esos pasos,
    /// aplica ERR-005 — mensaje en español por la salida de error y registro
    /// LOG-003 con nivel Error, a cargo del registrador—, fija el código de
    /// salida pendiente y relanza, de modo que el proceso se termine
    /// igualmente garantizando que no queden procesos residuales.
    /// </summary>
    public void Cerrar()
    {
        try
        {
            _registradorDeCicloDeVida?.RegistrarInicioDeCierre();

            // Liberación de los datos de la sesión (COM-004): el estado es
            // efímero y sin recursos externos; confirmada la liberación, los
            // datos quedan sin dueño y no se persisten — la elección de
            // idioma muere con la sesión (FR-013 → F-030, BR-F001-006).
            SesionLiberada = true;

            _registradorDeCicloDeVida?.RegistrarFinDeProceso();
        }
        catch (Exception excepcion)
        {
            // ERR-005: el proceso se termina igualmente, garantizando que no
            // queden procesos residuales; la elección de idioma no se
            // persiste (BR-F001-006, FR-013 → F-030).
            CodigoDeSalidaPendiente = _registradorDeCicloDeVida is null
                ? RegistradorDeCicloDeVida.CodigoSalidaPorFallo
                : _registradorDeCicloDeVida.RegistrarFalloDeCierre(excepcion);

            throw;
        }
    }
}
