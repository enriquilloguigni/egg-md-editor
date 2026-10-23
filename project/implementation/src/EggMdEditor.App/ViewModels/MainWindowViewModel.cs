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
    /// Menús de primer nivel de SC-001, en el orden de FR-011. Cada elemento
    /// conserva su clave i18n y el rótulo resuelto en el idioma activo.
    /// </summary>
    public IReadOnlyList<MainMenuDefinition> Menus
    {
        get
        {
            var catalog = Catalog ?? Session?.Catalog ?? I18nCatalog.Instance;
            var menus = new List<MainMenuDefinition>(MenuKeys.Count);
            foreach (var key in MenuKeys)
            {
                menus.Add(new MainMenuDefinition(key, catalog.Get(key)));
            }

            return new ReadOnlyCollection<MainMenuDefinition>(menus);
        }
    }

    public string FileMenuHeader => Menus[0].Header;
    public string EditMenuHeader => Menus[1].Header;
    public string ViewMenuHeader => Menus[2].Header;
    public string FormatMenuHeader => Menus[3].Header;
    public string ToolsMenuHeader => Menus[4].Header;
    public string HelpMenuHeader => Menus[5].Header;
}

/// <summary>
/// Menú de primer nivel de la barra persistente de SC-001 (ENT-004): clave
/// estable del catálogo i18n y rótulo visible en el idioma activo.
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006).</param>
/// <param name="Header">Rótulo resuelto en el idioma activo.</param>
public sealed record MainMenuDefinition(string Key, string Header);
