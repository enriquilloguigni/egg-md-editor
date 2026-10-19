using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Builder of SC-001's format bar (T-009): fills a horizontal panel with 100 %
/// of the controls of <see cref="FormatBarCatalog"/> —single source of count
/// and order (FR-012, DS-001)— in the declared writing order, with no extra
/// or missing controls. Each control shows its icon (VR-001: one glyph per
/// control, per the window-principal pattern of the design-system) and its
/// tooltip (VR-002): the control's name resolved from the i18n string catalog
/// (T-003) in the session's active language (Spanish at startup, BR-007),
/// shown at the pointer while it stays over the control and hidden when it
/// leaves, with no click interaction. Each button exposes its catalog key in
/// <see cref="Control.Tag"/>, with which the hot relabeling of the tooltips
/// (T-011, COM-001) re-resolves the texts. No control is connected to actions
/// (ADR-001): no command and no disabled marks, with the same appearance as a
/// connected control; pressing it does not alter the document.
/// </summary>
public static class FormatBarBuilder
{
    /// <summary>
    /// Glyph (VR-001) of each bar control, by catalog key: one glyph per
    /// control in writing order. F-001 does not include binary iconography
    /// resources: the textual glyph implements the design-system pattern
    /// (mockup Z3, [ic]) awaiting the definitive icon set (TK-004), with the
    /// control's size and spacing.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Glyphs { get; } =
        new Dictionary<string, string>
        {
            ["barra.enfasis"] = "B",
            ["barra.encabezados"] = "H▾",
            ["barra.listas"] = "≡",
            ["barra.cita"] = "“",
            ["barra.codigo"] = "</>",
            ["barra.enlace"] = "↗",
            ["barra.imagen"] = "▣",
            ["barra.tabla"] = "▦",
            ["barra.nota-al-pie"] = "†",
            ["barra.formula"] = "∑",
            ["barra.diagrama"] = "◇",
            ["barra.indice"] = "⋮",
        };

    /// <summary>
    /// Fills the given bar with the 12 controls of FR-012 in writing order
    /// (DS-001), built from the format bar catalog. The bar is cleared before
    /// filling, so the building can be repeated over the same bar.
    /// </summary>
    public static void Build(StackPanel bar, ApplicationSession session)
    {
        ArgumentNullException.ThrowIfNull(bar);
        ArgumentNullException.ThrowIfNull(session);

        bar.Children.Clear();

        foreach (var control in FormatBarCatalog.Controls)
        {
            bar.Children.Add(BuildControl(control, session));
        }
    }

    /// <summary>
    /// Builds the button of a catalog control: the glyph (VR-001) as content,
    /// the catalog key in <see cref="Control.Tag"/> for the relabeling
    /// (T-011, COM-001) and the tooltip (VR-002) resolved in the active
    /// language. No command: visible control with no connected action
    /// (ADR-001).
    /// </summary>
    private static Button BuildControl(FormatBarControl control, ApplicationSession session)
    {
        var button = new Button
        {
            Content = new TextBlock
            {
                Text = Glyphs[control.Key],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Tag = control.Key,
            Padding = new Thickness(10, 4),
        };

        // VR-002: tooltip with the control's name in the active language;
        // visible at the pointer and hidden when it leaves, without click.
        ToolTip.SetTip(button, I18nStringCatalog.Resolve(control.Key, session.ActiveLanguage));

        return button;
    }
}
