using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Construcción de la barra de formato de SC-001 (T-009) desde el catálogo de
// la barra: recuento y orden de escritura exactos de FR-012 (DS-001), un
// icono (VR-001) y un aviso flotante (VR-002) por control, avisos i18n
// resueltos en el idioma activo de la sesión (arranque español, BR-007) y
// todos los controles visibles sin acción conectada (ADR-001).
public class ConstructorDeBarraFormatoTests
{
    [Fact]
    public void Construye_los_12_controles_de_FR012_en_orden_de_escritura()
    {
        var barra = new StackPanel();

        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());

        Assert.Equal(CatalogoBarraFormato.TotalControles, barra.Children.Count);
        Assert.Equal(12, barra.Children.Count);
        Assert.Equal(
            CatalogoBarraFormato.Controles.Select(control => control.Clave),
            ControlesDe(barra).Select(control => control.Tag as string));
    }

    [Fact]
    public void Cada_clave_del_catalogo_tiene_glifo_definido()
    {
        // VR-001 al 100 %: no hay control de FR-012 sin icono.
        Assert.Equal(
            CatalogoBarraFormato.Controles.Select(control => control.Clave).OrderBy(clave => clave),
            ConstructorDeBarraFormato.Glifos.Keys.OrderBy(clave => clave));
    }

    [Fact]
    public void Pone_un_icono_glifo_por_control()
    {
        var barra = new StackPanel();

        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());

        var controles = ControlesDe(barra).ToList();
        Assert.All(controles, control =>
        {
            var glifo = Assert.IsType<TextBlock>(control.Content);
            Assert.False(string.IsNullOrEmpty(glifo.Text));
        });
        Assert.Equal(
            CatalogoBarraFormato.Controles.Select(control => ConstructorDeBarraFormato.Glifos[control.Clave]),
            controles.Select(control => ((TextBlock)control.Content!).Text));
    }

    [Fact]
    public void Resuelve_cada_aviso_flotante_en_el_idioma_activo_de_la_sesion_en_arranque()
    {
        // VR-002: el nombre de cada control, resuelto en el idioma activo.
        var barra = new StackPanel();

        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());

        var controles = ControlesDe(barra).ToList();
        Assert.All(controles, control =>
            Assert.Equal(
                CatalogoCadenasI18n.Resolver((string)control.Tag!, IdiomaDeInterfaz.Español),
                ToolTip.GetTip(control) as string));

        // Muestra puntual del arranque en español (BR-007): primer y último control.
        Assert.Equal("Énfasis", ToolTip.GetTip(controles[0]) as string);
        Assert.Equal("Índice", ToolTip.GetTip(controles[^1]) as string);
    }

    [Fact]
    public void Resuelve_los_avisos_en_ingles_cuando_la_sesion_arranca_en_english()
    {
        var sesion = SesionEnEspanol();
        sesion.IdiomaActivo = IdiomaDeInterfaz.English;

        var barra = new StackPanel();
        ConstructorDeBarraFormato.Construir(barra, sesion);

        var controles = ControlesDe(barra).ToList();
        Assert.All(controles, control =>
            Assert.Equal(
                CatalogoCadenasI18n.Resolver((string)control.Tag!, IdiomaDeInterfaz.English),
                ToolTip.GetTip(control) as string));
        Assert.Equal("Emphasis", ToolTip.GetTip(controles[0]) as string);
        Assert.Equal("Table of contents", ToolTip.GetTip(controles[^1]) as string);
    }

    [Fact]
    public void Deja_todos_los_controles_visibles_sin_accion_conectada()
    {
        // ADR-001: apariencia de control conectado, sin marcas de
        // deshabilitado y sin comando: pulsarlo no altera el documento.
        var barra = new StackPanel();

        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());

        var controles = ControlesDe(barra).ToList();
        Assert.Equal(CatalogoBarraFormato.TotalControles, controles.Count);
        Assert.All(controles, control =>
        {
            Assert.True(control.IsEnabled);
            Assert.True(control.IsVisible);
            Assert.Null(control.Command);
        });
    }

    [Fact]
    public void La_construccion_sobre_una_barra_existente_no_duplica_controles()
    {
        var barra = new StackPanel();

        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());
        ConstructorDeBarraFormato.Construir(barra, SesionEnEspanol());

        Assert.Equal(CatalogoBarraFormato.TotalControles, barra.Children.Count);
        Assert.Equal(
            CatalogoBarraFormato.Controles.Select(control => control.Clave),
            ControlesDe(barra).Select(control => control.Tag as string));
    }

    private static SesionDeAplicacion SesionEnEspanol() => new();

    private static IEnumerable<Button> ControlesDe(StackPanel barra) =>
        barra.Children.OfType<Button>();
}
