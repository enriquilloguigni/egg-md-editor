using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Entrada Herramientas → Idioma de la interfaz de SC-001 (T-010): submenú con
// los valores Español | English del catálogo de idiomas, con selección
// mutuamente excluyente (siempre una marcada, y es la del idioma activo de la
// sesión: AC-001, AC-007, AC-008) y sin acción de negocio conectada (ADR-001);
// la conmutación con relabelizado en caliente llega cableada en T-011
// (COM-001). El recuento de FR-011 no cambia: los valores son los valores de
// la selección de una entrada, no entradas nuevas (DS-001).
public class ConstructorDeMenuIdiomaTests
{
    [Fact]
    public void La_entrada_idioma_interfaz_despliega_los_valores_del_catalogo_en_orden()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionDeArranque());

        var entrada = EntradaIdiomaDeInterfaz(barra);

        Assert.Equal(
            CatalogoIdiomas.Valores.Select(ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma),
            entrada.Items.Cast<MenuItem>().Select(valor => valor.Tag as string));
    }

    [Fact]
    public void En_arranque_en_espanol_queda_marcado_solo_espanol()
    {
        // AC-001: la interfaz arranca en español, con Herramientas → Idioma
        // de la interfaz marcado en Español (BR-F001-004).
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionDeArranque());

        var (espanol, english) = ValoresDeIdioma(barra);

        Assert.True(espanol.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void En_sesion_en_english_queda_marcado_solo_english()
    {
        var sesion = SesionDeArranque();
        sesion.IdiomaActivo = IdiomaDeInterfaz.English;

        var barra = new Menu();
        ConstructorDeMenuPrincipal.Construir(barra, sesion);

        var (espanol, english) = ValoresDeIdioma(barra);

        Assert.False(espanol.IsChecked);
        Assert.True(english.IsChecked);
    }

    [Fact]
    public void La_seleccion_es_mutuamente_excluyente_siempre_una_marcada()
    {
        foreach (var idioma in CatalogoIdiomas.Valores)
        {
            var sesion = SesionDeArranque();
            sesion.IdiomaActivo = idioma;

            var barra = new Menu();
            ConstructorDeMenuPrincipal.Construir(barra, sesion);

            var marcadas = EntradaIdiomaDeInterfaz(barra).Items
                .Cast<MenuItem>()
                .Count(valor => valor.IsChecked);

            Assert.Equal(1, marcadas);
        }
    }

    [Fact]
    public void Los_valores_de_idioma_son_radio_del_mismo_grupo()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionDeArranque());

        var (espanol, english) = ValoresDeIdioma(barra);

        // Exclusión mutua en runtime (estado visual puro, sin negocio): ambos
        // valores son de radio y comparten grupo, con la marca pintada por el
        // tema para :checked:radio.
        Assert.Equal(MenuItemToggleType.Radio, espanol.ToggleType);
        Assert.Equal(MenuItemToggleType.Radio, english.ToggleType);
        Assert.Equal(espanol.GroupName, english.GroupName);
        Assert.False(string.IsNullOrEmpty(espanol.GroupName));
    }

    [Fact]
    public void Los_rotulos_de_los_valores_se_resuelven_en_el_idioma_activo()
    {
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionDeArranque());

        var (espanol, english) = ValoresDeIdioma(barra);

        // Nombres propios de cada idioma, iguales en es y en (SC-001).
        Assert.Equal("Español", espanol.Header as string);
        Assert.Equal("English", english.Header as string);
        foreach (var valor in new[] { espanol, english })
        {
            Assert.Equal(
                CatalogoCadenasI18n.Resolver((string)valor.Tag!, IdiomaDeInterfaz.English),
                valor.Header as string);
        }
    }

    [Fact]
    public void La_entrada_y_sus_valores_quedan_visibles_sin_accion_conectada()
    {
        // ADR-001: apariencia de entrada conectada, sin marcas de
        // deshabilitado y sin comando; pulsarlas no altera el documento.
        var barra = new Menu();

        ConstructorDeMenuPrincipal.Construir(barra, SesionDeArranque());

        var entrada = EntradaIdiomaDeInterfaz(barra);

        Assert.True(entrada.IsEnabled);
        Assert.Null(entrada.Command);
        Assert.All(entrada.Items.Cast<MenuItem>(), valor =>
        {
            Assert.True(valor.IsEnabled);
            Assert.Null(valor.Command);
        });
    }

    private static SesionDeAplicacion SesionDeArranque() => new();

    private static MenuItem EntradaIdiomaDeInterfaz(Menu barra) =>
        barra.Items.Cast<MenuItem>()
            .SelectMany(menu => menu.Items.Cast<MenuItem>())
            .Single(entrada => (string?)entrada.Tag == ConstructorDeMenuPrincipal.ClaveEntradaIdioma);

    private static (MenuItem Espanol, MenuItem English) ValoresDeIdioma(Menu barra)
    {
        var valores = EntradaIdiomaDeInterfaz(barra).Items.Cast<MenuItem>().ToList();

        Assert.Equal(2, valores.Count);

        return (valores[0], valores[1]);
    }
}
