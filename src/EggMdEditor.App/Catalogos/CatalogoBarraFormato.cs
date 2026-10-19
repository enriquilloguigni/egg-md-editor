namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Control de la barra de formato (DS-001). Se identifica por su posición en
/// el orden de escritura (el índice en la lista del catálogo) y una
/// <see cref="Clave"/> única y estable. El icono (VR-001) es un recurso visual
/// y el aviso flotante (VR-002) es texto i18n resuelto desde el catálogo de
/// cadenas (T-003) en el idioma activo; ambos se conectan en T-009.
/// </summary>
/// <param name="Clave">Identificador único y estable del control.</param>
public sealed record ControlDeBarra(string Clave);

/// <summary>
/// Catálogo de la barra de formato (DS-001): lista ordenada de los controles
/// de FR-012 en orden de escritura, como fuente única de recuento y de
/// construcción de la UI (T-009), con un icono y un aviso flotante por
/// control, sin conexión a acciones (ADR-001). En memoria, sin persistencia.
/// </summary>
public static class CatalogoBarraFormato
{
    /// <summary>Controles de FR-012 en orden de escritura.</summary>
    public static IReadOnlyList<ControlDeBarra> Controles { get; } =
    [
        new ControlDeBarra("barra.enfasis"),
        new ControlDeBarra("barra.encabezados"),
        new ControlDeBarra("barra.listas"),
        new ControlDeBarra("barra.cita"),
        new ControlDeBarra("barra.codigo"),
        new ControlDeBarra("barra.enlace"),
        new ControlDeBarra("barra.imagen"),
        new ControlDeBarra("barra.tabla"),
        new ControlDeBarra("barra.nota-al-pie"),
        new ControlDeBarra("barra.formula"),
        new ControlDeBarra("barra.diagrama"),
        new ControlDeBarra("barra.indice"),
    ];

    /// <summary>
    /// Total de controles de la barra. Derivado del catálogo: es la fuente
    /// única de recuento para la UI (T-009) y las pruebas (T-013).
    /// </summary>
    public static int TotalControles => Controles.Count;
}
