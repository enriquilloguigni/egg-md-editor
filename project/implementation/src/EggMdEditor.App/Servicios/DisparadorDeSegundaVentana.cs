namespace EggMdEditor.App.Servicios;

/// <summary>
/// Contraparte de enfoque de la ventana única SC-001 (BR-003): orden síncrono
/// y en proceso de traer al frente la ventana existente y darle el foco. La
/// implementación real sobre la ventana de Avalonia se cablea con el resto de
/// canales de SC-001 (T-011); el disparador nunca crea ventanas, de modo que
/// esta abstracción solo expone el enfoque de la que ya existe.
/// </summary>
public interface IEnfocadorDeVentana
{
    /// <summary>Da el foco a la ventana única existente SC-001 (BR-003).</summary>
    void Enfocar();
}

/// <summary>
/// Estado de la atención de una solicitud de segunda ventana (AC-014).
/// </summary>
public enum EstadoSolicitudSegundaVentana
{
    /// <summary>
    /// SC-001 existente recibió el foco: no se creó ninguna ventana y sigue
    /// habiendo exactamente una por sesión, sin transición de estado
    /// (AC-014, BR-003).
    /// </summary>
    Enfocada = 1,

    /// <summary>
    /// Sin ventana conectada aún —SC-001 no existe o su contraparte de
    /// enfoque no está cableada (T-011)—: sin respuesta; el disparador no
    /// crea ninguna ventana para atender la solicitud.
    /// </summary>
    SinVentanaConectada = 2,
}

/// <summary>
/// Resultado detallado de la atención de una solicitud de segunda ventana
/// (AC-014).
/// </summary>
/// <param name="Estado">Estado final de la solicitud.</param>
public sealed record ResultadoSolicitudSegundaVentana(
    EstadoSolicitudSegundaVentana Estado);

/// <summary>
/// Disparador de la solicitud de segunda ventana (AC-014): coordinador
/// síncrono y en proceso que recibe la solicitud de una segunda ventana de la
/// aplicación y la atiende enfocando la ventana única existente SC-001, sin
/// crear otra (BR-003). Acotado a proceso único por la Spec (riesgo del
/// Plan): no atiende segundos procesos lanzados ni usa canal interprocesal
/// alguno — la solicitud le llega dentro del propio proceso, desde el
/// cableado de SC-001 (T-011). No toca la sesión: la solicitud de segunda
/// ventana mientras la sesión está en ST-002/ST-003 no produce transición de
/// estado (states-transitions.md) y no genera registro técnico (logs.md no
/// define LOG-xxx para este evento interno de SC-001). Mientras la
/// contraparte de enfoque no esté conectada, la solicitud no tiene respuesta
/// y jamás se crea una ventana para atenderla: la creación de la única
/// ventana es del arranque (T-008), nunca de este coordinador.
/// </summary>
public sealed class DisparadorDeSegundaVentana
{
    private readonly IEnfocadorDeVentana? _enfocadorDeVentana;

    /// <summary>
    /// Crea el disparador. La contraparte de enfoque de SC-001 es inyectable
    /// y por defecto no está conectada: su implementación sobre la ventana
    /// real se cablea con el resto de canales de SC-001 (T-011).
    /// </summary>
    public DisparadorDeSegundaVentana(IEnfocadorDeVentana? enfocadorDeVentana = null)
    {
        _enfocadorDeVentana = enfocadorDeVentana;
    }

    /// <summary>
    /// Atiende una solicitud de segunda ventana de la aplicación (AC-014):
    /// con SC-001 existente, le da el foco —exactamente una invocación por
    /// solicitud— y no crea otra ventana, de modo que la sesión sigue con
    /// exactamente una (BR-003) y sin transición de estado. Sin ventana
    /// conectada, no produce respuesta.
    /// </summary>
    public ResultadoSolicitudSegundaVentana SolicitarSegundaVentana()
    {
        if (_enfocadorDeVentana is null)
        {
            return new ResultadoSolicitudSegundaVentana(
                EstadoSolicitudSegundaVentana.SinVentanaConectada);
        }

        _enfocadorDeVentana.Enfocar();

        return new ResultadoSolicitudSegundaVentana(
            EstadoSolicitudSegundaVentana.Enfocada);
    }
}
