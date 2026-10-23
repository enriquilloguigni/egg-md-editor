using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// ViewModel de la ventana principal SC-001 (T-006): expone los 6 menús
/// principales de FR-011 (ENT-004) en orden, con el rótulo resuelto desde el
/// catálogo i18n (ENT-006) en el idioma activo. Solo presentación: no hay
/// comandos ni acciones conectadas (ADR-001).
/// </summary>
public partial class MainWindowViewModel
{
    /// <summary>Claves i18n de los 6 menús de FR-011, en el orden declarado.</summary>
    public static IReadOnlyList<string> MenuKeys { get; } = Array.AsReadOnly(
    [
        "Menu.File",
        "Menu.Edit",
        "Menu.View",
        "Menu.Format",
        "Menu.Tools",
        "Menu.Help",
    ]);

    /// <summary>
    /// Controles de la barra de formato de FR-012 en orden de escritura
    /// (ENT-005): clave i18n estable y glyph de su icono (VR-001). El orden
    /// de la lista es el orden visible de la barra: no admite controles de
    /// más ni ausentes.
    /// </summary>
    private static readonly (string Key, string Glyph)[] FormatBarLayout =
    [
        ("barra.enfasis", "B"),
        ("barra.encabezados", "H▾"),
        ("barra.listas", "≡"),
        ("barra.cita", "“"),
        ("barra.codigo", "</>"),
        ("barra.enlace", "↗"),
        ("barra.imagen", "▣"),
        ("barra.tabla", "▦"),
        ("barra.nota-al-pie", "†"),
        ("barra.formula", "Σ"),
        ("barra.diagrama", "◇"),
        ("barra.indice", "⋮"),
    ];

    /// <summary>Claves i18n de los controles de la barra de formato, en orden.</summary>
    public static IReadOnlyList<string> FormatBarKeys { get; } = Array.AsReadOnly(
        FormatBarLayout.Select(control => control.Key).ToArray());

    /// <summary>Glyph del icono (VR-001) de cada control, por clave i18n.</summary>
    public static IReadOnlyDictionary<string, string> FormatBarGlyphs { get; } =
        FormatBarLayout.ToDictionary(
            control => control.Key,
            control => control.Glyph,
            StringComparer.Ordinal);

    /// <summary>
    /// Menús de primer nivel de SC-001, en el orden de FR-011. Cada elemento
    /// conserva su clave i18n y el rótulo resuelto en el idioma activo.
    /// </summary>
    public IReadOnlyList<MainMenuDefinition> Menus
    {
        get
        {
            var catalog = ResolveCatalog();
            var menus = new List<MainMenuDefinition>(MenuKeys.Count);
            foreach (var key in MenuKeys)
            {
                menus.Add(new MainMenuDefinition(key, catalog.Get(key)));
            }

            return new ReadOnlyCollection<MainMenuDefinition>(menus);
        }
    }

    /// <summary>
    /// Controles visibles de la barra de formato de SC-001 (ENT-005), en el
    /// orden de escritura de FR-012: clave i18n, glyph del icono (VR-001) y
    /// nombre resuelto en el idioma activo, que alimenta tanto el aviso
    /// flotante (VR-002) como el nombre accesible (NFR-009).
    /// </summary>
    public IReadOnlyList<FormatBarControlDefinition> FormatBarControls
    {
        get
        {
            var catalog = ResolveCatalog();
            var controls = new List<FormatBarControlDefinition>(FormatBarKeys.Count);
            foreach (var key in FormatBarKeys)
            {
                controls.Add(new FormatBarControlDefinition(key, FormatBarGlyphs[key], catalog.Get(key)));
            }

            return new ReadOnlyCollection<FormatBarControlDefinition>(controls);
        }
    }

    public string FileMenuHeader => Menus[0].Header;
    public string EditMenuHeader => Menus[1].Header;
    public string ViewMenuHeader => Menus[2].Header;
    public string FormatMenuHeader => Menus[3].Header;
    public string ToolsMenuHeader => Menus[4].Header;
    public string HelpMenuHeader => Menus[5].Header;

    private II18nCatalog ResolveCatalog() => Catalog ?? Session?.Catalog ?? I18nCatalog.Instance;
}

/// <summary>
/// Menú de primer nivel de la barra persistente de SC-001 (ENT-004): clave
/// estable del catálogo i18n y rótulo visible en el idioma activo.
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006).</param>
/// <param name="Header">Rótulo resuelto en el idioma activo.</param>
public sealed record MainMenuDefinition(string Key, string Header);

/// <summary>
/// Control de la barra de formato de SC-001 (ENT-005): clave i18n estable,
/// glyph del icono (VR-001) y nombre resuelto en el idioma activo, que se
/// muestra como aviso flotante (VR-002) y se expone como nombre accesible
/// (NFR-009). Sin acción conectada en F-001 (ADR-001).
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006); también identifica la posición.</param>
/// <param name="Glyph">Glyph visible del icono del control (VR-001).</param>
/// <param name="Name">Nombre del control en el idioma activo (VR-002, NFR-009).</param>
public sealed record FormatBarControlDefinition(string Key, string Glyph, string Name);
