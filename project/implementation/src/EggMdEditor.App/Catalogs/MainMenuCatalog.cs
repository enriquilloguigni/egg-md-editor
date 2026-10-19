namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Entry of a menu of the main menu catalog (DS-001). It is identified by a
/// unique, stable <see cref="Key"/>, whose position within the menu is given
/// by the order of the list; the visible label is i18n text resolved from the
/// string catalog (T-003) in the active language.
/// </summary>
/// <param name="Key">Unique, stable identifier of the entry.</param>
public sealed record MenuEntry(string Key);

/// <summary>
/// First-level menu of the persistent menu bar (FR-011). Its entries go in
/// declared order and belong exactly to this menu (DS-001). The position of
/// each entry is its index in <see cref="Entries"/>.
/// </summary>
/// <param name="Key">Unique, stable identifier of the menu.</param>
/// <param name="Entries">Entries of the menu, in declared order.</param>
public sealed record MainMenu(string Key, IReadOnlyList<MenuEntry> Entries);

/// <summary>
/// Main menu catalog (DS-001): ordered list of the 6 menus of FR-011 with
/// 100 % of their entries in order, as the single source of count and of UI
/// construction (T-008) with no action wiring (ADR-001). In memory, no
/// persistence. The Edit context menu is not part of this catalog (SC-001).
/// </summary>
public static class MainMenuCatalog
{
    /// <summary>Menus of FR-011 in order, each with its entries in order.</summary>
    public static IReadOnlyList<MainMenu> Menus { get; } =
    [
        new("menu.archivo",
        [
            new MenuEntry("menu.archivo.nuevo"),
            new MenuEntry("menu.archivo.abrir"),
            new MenuEntry("menu.archivo.guardar"),
            new MenuEntry("menu.archivo.guardar-como"),
            new MenuEntry("menu.archivo.recientes"),
            new MenuEntry("menu.archivo.autoguardado"),
            new MenuEntry("menu.archivo.exportar-pdf"),
        ]),
        new("menu.edicion",
        [
            new MenuEntry("menu.edicion.deshacer"),
            new MenuEntry("menu.edicion.rehacer"),
            new MenuEntry("menu.edicion.cortar"),
            new MenuEntry("menu.edicion.copiar"),
            new MenuEntry("menu.edicion.pegar"),
            new MenuEntry("menu.edicion.seleccionar-todo"),
            new MenuEntry("menu.edicion.buscar"),
            new MenuEntry("menu.edicion.reemplazar"),
            new MenuEntry("menu.edicion.ir-a-linea"),
        ]),
        new("menu.ver",
        [
            new MenuEntry("menu.ver.mostrar-ocultar-vistas"),
            new MenuEntry("menu.ver.mostrar-ocultar-barra-formato"),
            new MenuEntry("menu.ver.plegado-secciones"),
            new MenuEntry("menu.ver.zoom"),
            new MenuEntry("menu.ver.ajuste-linea"),
            new MenuEntry("menu.ver.desplazamiento-sincronizado"),
            new MenuEntry("menu.ver.pantalla-completa"),
            new MenuEntry("menu.ver.modo-foco"),
            new MenuEntry("menu.ver.tema"),
            new MenuEntry("menu.ver.fuente-markdown"),
        ]),
        new("menu.formato",
        [
            new MenuEntry("menu.formato.enfasis"),
            new MenuEntry("menu.formato.encabezados"),
            new MenuEntry("menu.formato.listas"),
            new MenuEntry("menu.formato.citas"),
            new MenuEntry("menu.formato.codigo"),
            new MenuEntry("menu.formato.enlaces"),
            new MenuEntry("menu.formato.imagenes"),
            new MenuEntry("menu.formato.tablas"),
            new MenuEntry("menu.formato.notas-al-pie"),
            new MenuEntry("menu.formato.formulas"),
            new MenuEntry("menu.formato.diagramas"),
            new MenuEntry("menu.formato.escape-sintaxis"),
            new MenuEntry("menu.formato.abrazo-delimitadores"),
            new MenuEntry("menu.formato.alineacion-columnas"),
            new MenuEntry("menu.formato.tab-en-tablas"),
            new MenuEntry("menu.formato.anclas-internas"),
        ]),
        new("menu.herramientas",
        [
            new MenuEntry("menu.herramientas.idioma-interfaz"),
            new MenuEntry("menu.herramientas.contador-palabras"),
            new MenuEntry("menu.herramientas.revision-ortografica"),
        ]),
        new("menu.ayuda",
        [
            new MenuEntry("menu.ayuda.guia-markdown"),
            new MenuEntry("menu.ayuda.atajos"),
            new MenuEntry("menu.ayuda.acercade"),
        ]),
    ];

    /// <summary>Total menus (6, FR-011). Derived from the list: never a fixed number.</summary>
    public static int TotalMenus => Menus.Count;

    /// <summary>
    /// Total entries of the main menu. Derived from the catalog: it is the
    /// single source of count for the UI (T-008) and the tests (T-013).
    /// </summary>
    public static int TotalEntries => Menus.Sum(menu => menu.Entries.Count);
}
