using EggMdEditor.App.Catalogos;
using Xunit;

namespace EggMdEditor.App.Tests;

// Estructura exacta y ordenada del catálogo del menú principal (FR-011,
// DS-001), según la enumeración canónica de SC-001. Las claves son el
// identificador estable de cada entrada; el 100 % de esta lista alimenta el
// recuento de T-008/T-013: no puede haber entradas de más ni ausentes.
public class CatalogoMenuPrincipalTests
{
    private static readonly string[] ClavesMenus =
    [
        "menu.archivo",
        "menu.edicion",
        "menu.ver",
        "menu.formato",
        "menu.herramientas",
        "menu.ayuda",
    ];

    [Fact]
    public void Tiene_los_6_menus_de_FR011_en_orden()
    {
        Assert.Equal(6, CatalogoMenuPrincipal.TotalMenus);
        Assert.Equal(
            ClavesMenus,
            CatalogoMenuPrincipal.Menus.Select(menu => menu.Clave));
    }

    [Fact]
    public void Archivo_tiene_las_7_entradas_de_FR011_en_orden()
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
            ClavesDe("menu.archivo"));
    }

    [Fact]
    public void Edicion_tiene_las_9_entradas_de_FR007_en_orden()
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
            ClavesDe("menu.edicion"));
    }

    [Fact]
    public void Ver_tiene_las_10_opciones_de_FR009_en_orden()
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
            ClavesDe("menu.ver"));
    }

    [Fact]
    public void Formato_tiene_las_16_entradas_de_FR002_en_orden()
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
            ClavesDe("menu.formato"));
    }

    [Fact]
    public void Herramientas_tiene_las_3_entradas_de_FR011_en_orden()
    {
        Assert.Equal(
            [
                "menu.herramientas.idioma-interfaz",
                "menu.herramientas.contador-palabras",
                "menu.herramientas.revision-ortografica",
            ],
            ClavesDe("menu.herramientas"));
    }

    [Fact]
    public void Ayuda_tiene_las_3_entradas_de_FR011_en_orden()
    {
        Assert.Equal(
            [
                "menu.ayuda.guia-markdown",
                "menu.ayuda.atajos",
                "menu.ayuda.acercade",
            ],
            ClavesDe("menu.ayuda"));
    }

    [Fact]
    public void Total_entradas_es_la_suma_del_100_por_ciento_de_FR011()
    {
        Assert.Equal(7 + 9 + 10 + 16 + 3 + 3, CatalogoMenuPrincipal.TotalEntradas);
        Assert.Equal(48, CatalogoMenuPrincipal.TotalEntradas);
    }

    [Fact]
    public void Cada_entrada_pertenece_a_exactamente_un_menu_y_las_claves_son_unicas()
    {
        var claves = CatalogoMenuPrincipal.Menus
            .SelectMany(menu => menu.Entradas.Select(entrada => entrada.Clave))
            .ToList();

        Assert.Equal(claves.Count, claves.Distinct().Count());
        Assert.All(CatalogoMenuPrincipal.Menus, menu => Assert.NotEmpty(menu.Entradas));
    }

    private static IReadOnlyList<string> ClavesDe(string claveMenu)
    {
        var menu = Assert.Single(
            CatalogoMenuPrincipal.Menus,
            candidato => candidato.Clave == claveMenu);

        return menu.Entradas.Select(entrada => entrada.Clave).ToList();
    }
}
