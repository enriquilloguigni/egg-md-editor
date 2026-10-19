using EggMdEditor.App.Catalogs;
using Xunit;

namespace EggMdEditor.App.Tests;

// Exact, ordered structure of the main menu catalog (FR-011, DS-001), per
// SC-001's canonical enumeration. The keys are the stable identifier of each
// entry; 100 % of this list feeds the count of T-008/T-013: there can be no
// extra or missing entries.
public class MainMenuCatalogTests
{
    private static readonly string[] MenuKeys =
    [
        "menu.archivo",
        "menu.edicion",
        "menu.ver",
        "menu.formato",
        "menu.herramientas",
        "menu.ayuda",
    ];

    [Fact]
    public void Has_the_6_menus_of_FR011_in_order()
    {
        Assert.Equal(6, MainMenuCatalog.TotalMenus);
        Assert.Equal(
            MenuKeys,
            MainMenuCatalog.Menus.Select(menu => menu.Key));
    }

    [Fact]
    public void File_has_the_7_entries_of_FR011_in_order()
    {
        Assert.Equal(
            [
                "menu.archivo.nuevo",
                "menu.archivo.abrir",
                "menu.archivo.guardar",
                "menu.archivo.guardar-como",
                "menu.archivo.recientes",
                "menu.archivo.autoguardado",
                "menu.archivo.exportar-pdf",
            ],
            KeysOf("menu.archivo"));
    }

    [Fact]
    public void Edit_has_the_9_entries_of_FR007_in_order()
    {
        Assert.Equal(
            [
                "menu.edicion.deshacer",
                "menu.edicion.rehacer",
                "menu.edicion.cortar",
                "menu.edicion.copiar",
                "menu.edicion.pegar",
                "menu.edicion.seleccionar-todo",
                "menu.edicion.buscar",
                "menu.edicion.reemplazar",
                "menu.edicion.ir-a-linea",
            ],
            KeysOf("menu.edicion"));
    }

    [Fact]
    public void View_has_the_10_options_of_FR009_in_order()
    {
        Assert.Equal(
            [
                "menu.ver.mostrar-ocultar-vistas",
                "menu.ver.mostrar-ocultar-barra-formato",
                "menu.ver.plegado-secciones",
                "menu.ver.zoom",
                "menu.ver.ajuste-linea",
                "menu.ver.desplazamiento-sincronizado",
                "menu.ver.pantalla-completa",
                "menu.ver.modo-foco",
                "menu.ver.tema",
                "menu.ver.fuente-markdown",
            ],
            KeysOf("menu.ver"));
    }

    [Fact]
    public void Format_has_the_16_entries_of_FR002_in_order()
    {
        Assert.Equal(
            [
                "menu.formato.enfasis",
                "menu.formato.encabezados",
                "menu.formato.listas",
                "menu.formato.citas",
                "menu.formato.codigo",
                "menu.formato.enlaces",
                "menu.formato.imagenes",
                "menu.formato.tablas",
                "menu.formato.notas-al-pie",
                "menu.formato.formulas",
                "menu.formato.diagramas",
                "menu.formato.escape-sintaxis",
                "menu.formato.abrazo-delimitadores",
                "menu.formato.alineacion-columnas",
                "menu.formato.tab-en-tablas",
                "menu.formato.anclas-internas",
            ],
            KeysOf("menu.formato"));
    }

    [Fact]
    public void Tools_has_the_3_entries_of_FR011_in_order()
    {
        Assert.Equal(
            [
                "menu.herramientas.idioma-interfaz",
                "menu.herramientas.contador-palabras",
                "menu.herramientas.revision-ortografica",
            ],
            KeysOf("menu.herramientas"));
    }

    [Fact]
    public void Help_has_the_3_entries_of_FR011_in_order()
    {
        Assert.Equal(
            [
                "menu.ayuda.guia-markdown",
                "menu.ayuda.atajos",
                "menu.ayuda.acercade",
            ],
            KeysOf("menu.ayuda"));
    }

    [Fact]
    public void Total_entries_is_the_sum_of_100_percent_of_FR011()
    {
        Assert.Equal(7 + 9 + 10 + 16 + 3 + 3, MainMenuCatalog.TotalEntries);
        Assert.Equal(48, MainMenuCatalog.TotalEntries);
    }

    [Fact]
    public void Each_entry_belongs_to_exactly_one_menu_and_keys_are_unique()
    {
        var keys = MainMenuCatalog.Menus
            .SelectMany(menu => menu.Entries.Select(entry => entry.Key))
            .ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
        Assert.All(MainMenuCatalog.Menus, menu => Assert.NotEmpty(menu.Entries));
    }

    private static IReadOnlyList<string> KeysOf(string menuKey)
    {
        var menu = Assert.Single(
            MainMenuCatalog.Menus,
            candidate => candidate.Key == menuKey);

        return menu.Entries.Select(entry => entry.Key).ToList();
    }
}
