using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Cableado de COM-001 en SC-001 (T-011): relabelizado en caliente desde la
// notificación única del servicio de conmutación (T-004), con toda cadena
// re-resuelta desde el idioma activo ya aplicado a la sesión —la fuente
// observable— y sin reinicio (DS-003, AC-007, AC-008, NFR-006), marca de la
// selección de idioma re-pintada desde el estado (T-010) y estado de la
// sesión —menú y aviso flotante— intacto (BR-F001-005). La conmutación
// abortada (ERR-003) no notifica: rótulos y marca permanecen en el idioma
// anterior (AC-010).
public class CableDeRelabelizadoTests
{
    [Fact]
    public void La_conmutacion_relabeliza_los_rotulos_de_menu_en_caliente()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);

        var barra = barraMenu(ventana);
        Assert.Equal("File", HeaderDe(barra, "menu.archivo"));
        Assert.Equal("Edit", HeaderDe(barra, "menu.edicion"));
        Assert.Equal("New", HeaderDe(barra, "menu.archivo.nuevo"));
        Assert.Equal("Interface language", HeaderDe(barra, ConstructorDeMenuPrincipal.ClaveEntradaIdioma));
    }

    [Fact]
    public void La_conmutacion_relabeliza_los_avisos_flotantes_de_la_barra_de_formato()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);

        Assert.Equal("Emphasis", AvisoDe(ventana, 0));
        Assert.Equal("Table of contents", AvisoDe(ventana, 11));
    }

    [Fact]
    public void La_conmutacion_remarca_la_seleccion_de_idioma_desde_el_estado()
    {
        // Camino por atajo (sin pulsación del menú): la marca acompaña al
        // idioma activo porque el relabelizado la re-pinta desde la sesión.
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);

        var (espanol, english) = ValoresDeIdioma(ventana);
        Assert.False(espanol.IsChecked);
        Assert.True(english.IsChecked);

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.Español);

        Assert.True(espanol.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void La_conmutacion_conserva_el_estado_de_la_sesion()
    {
        // AC-007: un menú puede estar desplegado y un aviso flotante visible
        // al conmutar; el relabelizado solo toca textos y marcas (BR-F001-005).
        var sesion = new SesionDeAplicacion
        {
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var ventana = new MainWindow(sesion);

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);

        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
    }

    [Fact]
    public void La_conmutacion_abortada_conserva_rotulos_marca_e_idioma_anteriores()
    {
        // AC-010 (ERR-003): el catálogo del idioma destino no carga; sin
        // notificación no hay relabelizado y toda la UI permanece como estaba.
        var sesion = new SesionDeAplicacion();
        var suministradorRoto = new SuministradorRoto();
        var ventana = new MainWindow(
            sesion,
            new ServicioConmutacionIdioma(sesion, suministradorRoto));

        var resultado = ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);

        Assert.Equal(EstadoConmutacionIdioma.AbortadaPorCatalogoNoDisponible, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal("Archivo", HeaderDe(barraMenu(ventana), "menu.archivo"));
        Assert.Equal("Énfasis", AvisoDe(ventana, 0));

        var (espanol, english) = ValoresDeIdioma(ventana);
        Assert.True(espanol.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void Pulsar_el_valor_english_del_menu_conmuta_y_relabeliza()
    {
        // COM-001 desde la entrada de idioma (T-010): la pulsación del valor
        // conmuta y la notificación única (DS-003) relabeliza.
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        ValorDeIdioma(ventana, IdiomaDeInterfaz.English)
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal("File", HeaderDe(barraMenu(ventana), "menu.archivo"));

        var (espanol, english) = ValoresDeIdioma(ventana);
        Assert.False(espanol.IsChecked);
        Assert.True(english.IsChecked);
    }

    [Fact]
    public void Pulsar_el_valor_ya_activo_deja_la_marca_y_los_rotulos_intactos()
    {
        // Operación nula (BR-F001-004): sin conmutación, sin notificación y
        // con la marca restaurada desde el estado (solo una marcada).
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        var notificaciones = new List<IdiomaConmutadoEventArgs>();
        ventana.ServicioDeIdioma.IdiomaConmutado += (_, e) => notificaciones.Add(e);

        ValorDeIdioma(ventana, IdiomaDeInterfaz.Español)
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Empty(notificaciones);
        Assert.Equal("Archivo", HeaderDe(barraMenu(ventana), "menu.archivo"));

        var (espanol, english) = ValoresDeIdioma(ventana);
        Assert.True(espanol.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void Pulsar_un_valor_con_conmutacion_abortada_restaura_la_marca_del_idioma_activo()
    {
        // ERR-003 desde la entrada de idioma: la pulsación no deja cambio
        // parcial; la marca vuelve al idioma activo aunque el tema o la
        // pulsación la hubieran movido (AC-010).
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(
            sesion,
            new ServicioConmutacionIdioma(sesion, new SuministradorRoto()));

        var english = ValorDeIdioma(ventana, IdiomaDeInterfaz.English);
        english.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal("Archivo", HeaderDe(barraMenu(ventana), "menu.archivo"));

        var (espanol, englishTrasAborto) = ValoresDeIdioma(ventana);
        Assert.True(espanol.IsChecked);
        Assert.False(englishTrasAborto.IsChecked);
    }

    // --- Ayudantes ---

    private static Menu barraMenu(MainWindow ventana) =>
        ventana.FindControl<Menu>("BarraMenu") ?? throw new InvalidOperationException("SC-001 sin barra de menú.");

    private static string HeaderDe(Menu barra, string clave) =>
        EnumerarElementos(barra).Single(item => (string?)item.Tag == clave).Header as string
        ?? throw new InvalidOperationException($"El elemento {clave} no tiene rótulo.");

    private static MenuItem ValorDeIdioma(MainWindow ventana, IdiomaDeInterfaz idioma) =>
        EnumerarElementos(barraMenu(ventana))
            .Single(item => (string?)item.Tag == ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma(idioma));

    private static (MenuItem Espanol, MenuItem English) ValoresDeIdioma(MainWindow ventana) =>
        (ValorDeIdioma(ventana, IdiomaDeInterfaz.Español), ValorDeIdioma(ventana, IdiomaDeInterfaz.English));

    private static IEnumerable<MenuItem> EnumerarElementos(Menu barra) =>
        barra.Items.OfType<MenuItem>().SelectMany(EnumerarElementos);

    private static IEnumerable<MenuItem> EnumerarElementos(MenuItem item)
    {
        yield return item;

        foreach (var hijo in item.Items.OfType<MenuItem>())
        {
            foreach (var nieto in EnumerarElementos(hijo))
            {
                yield return nieto;
            }
        }
    }

    private static string AvisoDe(MainWindow ventana, int indice) =>
        ToolTip.GetTip(botonDeBarra(ventana, indice)) as string
        ?? throw new InvalidOperationException("El control de la barra no tiene aviso flotante.");

    private static Button botonDeBarra(MainWindow ventana, int indice) =>
        ventana.FindControl<StackPanel>("BarraFormato")!.Children.OfType<Button>().ElementAt(indice);

    /// <summary>Suministrador cuyo catálogo nunca carga: provoca el abortado ERR-003.</summary>
    private sealed class SuministradorRoto : ISuministradorTextosI18n
    {
        public bool TryCargar(string codigoIdioma, out IReadOnlyDictionary<string, string> textos)
        {
            textos = new Dictionary<string, string>();
            return false;
        }
    }
}
