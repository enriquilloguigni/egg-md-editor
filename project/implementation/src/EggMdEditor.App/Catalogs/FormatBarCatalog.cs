namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Control of the format bar (DS-001). It is identified by its position in
/// the writing order (the index in the catalog's list) and a unique, stable
/// <see cref="Key"/>. The icon (VR-001) is a visual resource and the tooltip
/// (VR-002) is i18n text resolved from the string catalog (T-003) in the
/// active language; both are wired in T-009.
/// </summary>
/// <param name="Key">Unique, stable identifier of the control.</param>
public sealed record FormatBarControl(string Key);

/// <summary>
/// Format bar catalog (DS-001): ordered list of the controls of FR-012 in
/// writing order, as the single source of count and of UI construction
/// (T-009), with one icon and one tooltip per control, with no action wiring
/// (ADR-001). In memory, no persistence.
/// </summary>
public static class FormatBarCatalog
{
    /// <summary>Controls of FR-012 in writing order.</summary>
    public static IReadOnlyList<FormatBarControl> Controls { get; } =
    [
        new FormatBarControl("barra.enfasis"),
        new FormatBarControl("barra.encabezados"),
        new FormatBarControl("barra.listas"),
        new FormatBarControl("barra.cita"),
        new FormatBarControl("barra.codigo"),
        new FormatBarControl("barra.enlace"),
        new FormatBarControl("barra.imagen"),
        new FormatBarControl("barra.tabla"),
        new FormatBarControl("barra.nota-al-pie"),
        new FormatBarControl("barra.formula"),
        new FormatBarControl("barra.diagrama"),
        new FormatBarControl("barra.indice"),
    ];

    /// <summary>
    /// Total controls of the bar. Derived from the catalog: it is the single
    /// source of count for the UI (T-009) and the tests (T-013).
    /// </summary>
    public static int TotalControls => Controls.Count;
}
