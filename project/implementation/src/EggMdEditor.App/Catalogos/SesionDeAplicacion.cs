namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Sesión de aplicación (DS-001): estado efímero desde el arranque hasta
/// cerrar la ventana SC-001; no persistente (FR-013 → F-030) y con defectos
/// de arranque según la Spec: idioma Español (BR-007), menú Cerrado y aviso
/// flotante oculto. Contiene exactamente una ventana SC-001 (BR-003), que no
/// se modela aquí (la ventana es responsabilidad de T-008).
/// </summary>
public sealed class SesionDeAplicacion
{
    /// <summary>
    /// Identificador único de la sesión, asignado al crearla (una sesión por
    /// ejecución): correlación de los registros técnicos, que lo comparten
    /// todos y salen por la salida estándar, sin escritura a disco (NFR-003).
    /// </summary>
    public string IdentificadorDeSesion { get; } = Guid.NewGuid().ToString("D");

    /// <summary>Idioma activo de la sesión. Siempre exactamente uno (BR-007).</summary>
    public IdiomaDeInterfaz IdiomaActivo { get; set; } = CatalogoIdiomas.Defecto;

    /// <summary>Estado del menú principal de SC-001.</summary>
    public EstadoMenuPrincipal EstadoMenuPrincipal { get; set; } = CatalogoEstadoMenuPrincipal.Defecto;

    /// <summary>
    /// Aviso flotante visible (VR-002). Booleano con defecto No; cuando es
    /// visible, está asociado siempre a un control de la barra de formato
    /// (identificable por la clave de <see cref="CatalogoBarraFormato"/>).
    /// </summary>
    public bool AvisoFlotanteVisible { get; set; }
}
