using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Construcción de la barra de menú persistente de SC-001 (T-008) desde el
// catálogo del menú principal: recuento y orden exactos de FR-011 (DS-001),
// rótulos i18n resueltos en el idioma activo de la sesión (arranque español,
// BR-007) y todas las entradas visibles sin acción conectada (ADR-001).
public class ConstructorDeMenuPrincipalTests
{
    [Fact]
    public void Construye_los_6_menus_de_FR011_en_orden_del_catalogo()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());

        Assert.Equal(
            CatalogoMenuPrincipal.Menus.Select(menu => menu.Clave),
            ClavesDeMenus(barra));
    }

    [Fact]
    public void Construye_las_48_entradas_de_FR011_en_orden_del_catalogo()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());

        var entradas = EntradasDe(barra).ToList();

        Assert.Equal(CatalogoMenuPrincipal.TotalEntradas, entradas.Count);
        Assert.Equal(48, entradas.Count);
        Assert.Equal(
            CatalogoMenuPrincipal.Menus
                .SelectMany(menu => menu.Entradas.Select(entrada => entrada.Clave)),
            entradas.Select(entrada => entrada.Tag as string));
    }

    [Fact]
    public void Resuelve_cada_rotulo_en_el_idioma_activo_de_la_sesion_en_arranque()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());

        foreach (var menu in barra.Items.Cast<MenuItem>())
        {
            Assert.Equal(
                CatalogoCadenasI18n.Resolver((string)menu.Tag!, IdiomaDeInterfaz.Español),
                menu.Header as string);
            foreach (var entrada in menu.Items.Cast<MenuItem>())
            {
                Assert.Equal(
                    CatalogoCadenasI18n.Resolver((string)entrada.Tag!, IdiomaDeInterfaz.Español),
                    entrada.Header as string);
            }
        }

        // Muestra puntual del arranque en español (BR-007): primer menú y su
        // primera entrada.
        var menus = barra.Items.Cast<MenuItem>().ToList();
        Assert.Equal("Archivo", menus[0].Header as string);
        Assert.Equal("Nuevo", ((MenuItem)menus[0].Items[0]!).Header as string);
    }

    [Fact]
    public void Resuelve_los_rotulos_en_ingles_cuando_la_sesion_arranca_en_english()
    {
        var sesion = SesionEnEspanol();
        sesion.IdiomaActivo = IdiomaDeInterfaz.English;

        var barra = new Menu();
        ConstructorDeMenuPrincipal.Construir(barra, sesion);

        var menus = barra.Items.Cast<MenuItem>().ToList();
        Assert.Equal("File", menus[0].Header as string);
        Assert.Equal("Edit", menus[1].Header as string);
        foreach (var menu in menus)
        {
            Assert.Equal(
                CatalogoCadenasI18n.Resolver((string)menu.Tag!, IdiomaDeInterfaz.English),
                menu.Header as string);
            foreach (var entrada in menu.Items.Cast<MenuItem>())
            {
                Assert.Equal(
                    CatalogoCadenasI18n.Resolver((string)entrada.Tag!, IdiomaDeInterfaz.English),
                    entrada.Header as string);
            }
        }
    }

    [Fact]
    public void Deja_todas_las_entradas_visibles_sin_accion_conectada()
    {
        // ADR-001: apariencia de entrada conectada, sin marcas de
        // deshabilitado y sin comando: pulsarlas no altera el documento.
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());

        var elementos = barra.Items.Cast<MenuItem>()
            .Concat(EntradasDe(barra))
            .ToList();

        Assert.Equal(
            CatalogoMenuPrincipal.TotalMenus + CatalogoMenuPrincipal.TotalEntradas,
            elementos.Count);
        Assert.All(elementos, elemento =>
        {
            Assert.True(elemento.IsEnabled);
            Assert.Null(elemento.Command);
        });
    }

    [Fact]
    public void La_construccion_sobre_una_barra_existente_no_duplica_menus()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());
        ConstructorDeMenuPrincipal.Construir(barra, SesionEnEspanol());

        Assert.Equal(CatalogoMenuPrincipal.TotalMenus, barra.Items.Count);
        Assert.Equal(
            CatalogoMenuPrincipal.TotalEntradas,
            EntradasDe(barra).Count());
    }

    private static SesionDeAplicacion SesionEnEspanol() => new();

    private static IEnumerable<string> ClavesDeMenus(Menu barra) =>
        barra.Items.Cast<MenuItem>().Select(menu => menu.Tag as string ?? string.Empty);

    private static IEnumerable<MenuItem> EntradasDe(Menu barra) =>
        barra.Items.Cast<MenuItem>().SelectMany(menu => menu.Items.Cast<MenuItem>());
}
