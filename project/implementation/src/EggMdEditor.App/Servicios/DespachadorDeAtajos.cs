using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Acciones conectables a un atajo en F-001, según el Plan: alternar el
/// idioma de la interfaz (vía COM-001) y cerrar la aplicación (vía COM-004).
/// La asignación combinación → acción vive únicamente en el registro único
/// del despachador; su revisión al conectar cada acción nueva corresponde a
/// F-028/F-029.
/// </summary>
public enum AccionDeAtajo
{
    /// <summary>Alterna el idioma de la interfaz entre Español y English (COM-001, BR-F001-005).</summary>
    AlternarIdioma = 1,

    /// <summary>Cierra la aplicación: finalización de la sesión (COM-004).</summary>
    CerrarAplicacion = 2,
}

/// <summary>
/// Contraparte del atajo de cierre (COM-004): orden de cierre que finaliza la
/// sesión de la aplicación liberando sus datos, síncrono y en proceso. La
/// lógica de finalización y su cableado se implementan con el cierre de
/// SC-001 (T-011): el despachador solo la invoca si está conectada y, mientras
/// no lo esté, el atajo <c>Ctrl/Cmd+Q</c> no produce respuesta (COM-002,
/// ADR-001).
/// </summary>
public interface IAccionDeCierre
{
    /// <summary>Ejecuta el cierre de la aplicación (COM-004).</summary>
    void Ejecutar();
}

/// <summary>
/// Estado del despacho de un gesto de teclado (COM-002): la acción se ejecutó
/// si la combinación está asignada y su acción está conectada; no hay
/// respuesta si la combinación está asignada pero su acción aún no existe; y
/// el gesto es inerte si su combinación no figura en el registro único
/// (BR-F001-003, ADR-001).
/// </summary>
public enum EstadoDespachoAtajo
{
    /// <summary>Combinación asignada en el registro único y acción conectada: la acción se ejecutó.</summary>
    Despachado = 1,

    /// <summary>Combinación asignada cuya acción aún no está conectada: sin respuesta (COM-002, ADR-001).</summary>
    SinAccionConectada = 2,

    /// <summary>Combinación no asignada: atajo inerte, sin efecto observable (BR-F001-003, ADR-001).</summary>
    Inerte = 3,
}

/// <summary>
/// Resultado detallado del despacho de un gesto de teclado (COM-002).
/// </summary>
/// <param name="Estado">Estado del despacho.</param>
/// <param name="Accion">Acción asignada a la combinación; nula si el atajo es inerte.</param>
/// <param name="Conmutacion">
/// Resultado del canal COM-001 cuando la acción fue
/// <see cref="AccionDeAtajo.AlternarIdioma"/> — incluida una conmutación
/// abortada por catálogo no disponible, cuyo tratamiento es propio de
/// COM-001 (ERR-003, LOG-002)—; nulo en los demás casos.
/// </param>
public sealed record ResultadoDespachoAtajo(
    EstadoDespachoAtajo Estado,
    AccionDeAtajo? Accion,
    ResultadoConmutacionIdioma? Conmutacion);

/// <summary>
/// Despachador de atajos (COM-002): canal síncrono y en proceso que recibe el
/// gesto de teclado de SC-001 y ejecuta la acción asignada a su combinación
/// de teclas. El registro de combinaciones es único y centralizado en esta
/// clase (riesgo del Plan): asigna <c>Ctrl+Shift+L</c> —su forma macOS es
/// <c>Cmd+Shift+L</c>, la misma combinación lógica sobre el modificador
/// primario— a alternar es/en vía COM-001, y <c>Ctrl+Q</c> —forma macOS
/// <c>Cmd+Q</c>— al cierre de la aplicación vía COM-004; cualquier otra
/// combinación permanece inerte (BR-F001-003, ADR-001). El despachador no
/// mantiene estado propio ni emite registros: el único efecto observable de
/// un despacho es el de la acción conectada que lo recibe (el LOG-002 de
/// COM-001, o el de la finalización cuando el cierre se implemente). El
/// gesto llega ya traducido del teclado físico; traducir teclas a
/// modificadores y tecla principal es responsabilidad del cableado de la
/// ventana (T-011).
/// </summary>
public sealed class DespachadorDeAtajos
{
    /// <summary>
    /// Registro único de combinaciones asignadas en F-001 (riesgo del Plan):
    /// la única fuente de la asignación combinación → acción, revisada al
    /// conectar cada acción futura (F-028/F-029).
    /// </summary>
    private static readonly IReadOnlyDictionary<CombinacionDeTeclas, AccionDeAtajo> RegistroDeCombinaciones =
        new Dictionary<CombinacionDeTeclas, AccionDeAtajo>
        {
            [new(ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Mayusculas, 'L')] =
                AccionDeAtajo.AlternarIdioma,
            [new(ModificadoresDeTeclas.Primario, 'Q')] =
                AccionDeAtajo.CerrarAplicacion,
        };

    private readonly ServicioConmutacionIdioma _servicioDeIdioma;
    private readonly IAccionDeCierre? _accionDeCierre;

    /// <summary>
    /// Crea el despachador sobre el servicio de conmutación de idioma
    /// (COM-001, T-004). La acción de cierre (COM-004) es inyectable y por
    /// defecto no está conectada: su lógica de finalización de la sesión se
    /// implementa después (T-011).
    /// </summary>
    public DespachadorDeAtajos(
        ServicioConmutacionIdioma servicioDeIdioma,
        IAccionDeCierre? accionDeCierre = null)
    {
        _servicioDeIdioma = servicioDeIdioma ?? throw new ArgumentNullException(nameof(servicioDeIdioma));
        _accionDeCierre = accionDeCierre;
    }

    /// <summary>
    /// Registro único de combinaciones asignadas, expuesto para su revisión
    /// al conectar cada acción nueva (F-028/F-029): claves normalizadas y sin
    /// duplicados por construcción.
    /// </summary>
    public static IReadOnlyDictionary<CombinacionDeTeclas, AccionDeAtajo> CombinacionesAsignadas =>
        RegistroDeCombinaciones;

    /// <summary>
    /// Recibe un gesto de teclado y despacha la acción asignada a su
    /// combinación (COM-002): alterna el idioma vía COM-001, invoca la
    /// contraparte de cierre si está conectada o no produce efecto alguno,
    /// tanto si la combinación no está asignada (atajo inerte, BR-F001-003)
    /// como si su acción aún no existe (sin respuesta, ADR-001).
    /// </summary>
    public ResultadoDespachoAtajo Despachar(CombinacionDeTeclas combinacion)
    {
        if (!RegistroDeCombinaciones.TryGetValue(combinacion, out var accion))
        {
            return new ResultadoDespachoAtajo(EstadoDespachoAtajo.Inerte, null, null);
        }

        return accion switch
        {
            AccionDeAtajo.AlternarIdioma => AlternarIdioma(),
            AccionDeAtajo.CerrarAplicacion => CerrarAplicacion(),

            // Inalcanzable mientras el registro solo contenga acciones del
            // catálogo; un valor ajeno sería un error de programación.
            _ => throw new InvalidOperationException(
                "El registro único de combinaciones contiene una acción de atajo no reconocida."),
        };
    }

    /// <summary>
    /// Sobrecarga de conveniencia para el cableado del teclado (T-011):
    /// normaliza el gesto recibido —modificadores reducidos al modelo y tecla
    /// en mayúscula invariable— y despacha.
    /// </summary>
    public ResultadoDespachoAtajo Despachar(ModificadoresDeTeclas modificadores, char tecla) =>
        Despachar(CombinacionDeTeclas.De(modificadores, tecla));

    /// <summary>
    /// Alterna el idioma de la interfaz (COM-001): con exactamente dos
    /// idiomas mutuamente excluyentes (BR-007), el destino de la alternancia
    /// es el distinto del activo. La conmutación y su notificación única de
    /// relabelizado (DS-003) son enteramente de COM-001; el resultado viaja
    /// en el despacho para el cableado (T-011).
    /// </summary>
    private ResultadoDespachoAtajo AlternarIdioma()
    {
        var idiomaActivo = _servicioDeIdioma.IdiomaActivo;
        var idiomaDestino = CatalogoIdiomas.Valores.Single(idioma => idioma != idiomaActivo);
        var conmutacion = _servicioDeIdioma.Conmutar(idiomaDestino);

        return new ResultadoDespachoAtajo(
            EstadoDespachoAtajo.Despachado, AccionDeAtajo.AlternarIdioma, conmutacion);
    }

    /// <summary>
    /// Cierre de la aplicación (COM-004): invoca la contraparte inyectada una
    /// única vez, o no produce respuesta mientras no esté conectada.
    /// </summary>
    private ResultadoDespachoAtajo CerrarAplicacion()
    {
        if (_accionDeCierre is null)
        {
            return new ResultadoDespachoAtajo(
                EstadoDespachoAtajo.SinAccionConectada, AccionDeAtajo.CerrarAplicacion, null);
        }

        _accionDeCierre.Ejecutar();

        return new ResultadoDespachoAtajo(
            EstadoDespachoAtajo.Despachado, AccionDeAtajo.CerrarAplicacion, null);
    }
}
