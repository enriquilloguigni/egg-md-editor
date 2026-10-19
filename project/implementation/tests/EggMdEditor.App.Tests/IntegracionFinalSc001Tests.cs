using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Integración final de F-001 (T-013): la SC-001 completa —ventana real con
// sus catálogos construidos y los canales COM-001–COM-004 cableados (T-011)—
// frente a las métricas de la Spec: recuento del 100 % de FR-011/FR-012 tras
// el arranque sin ausencias ni elementos de más (DS-001), tasa de claves i18n
// resueltas del 100 % tras el arranque y tras cada conmutación (DS-002) y
// conmutaciones es/en completadas en la misma sesión conservando ventana y
// estado, sin reinicios ni cambios parciales (DS-003). Complementa —sin
// duplicar— las pruebas unitarias y de cableado de T-001–T-012, que cubren
// cada pieza por separado.
public class IntegracionFinalSc001Tests
{
    // --- DS-001: recuento del 100 % en SC-001 tras el arranque ---

    [Fact]
    public void Sc001_arranca_con_el_100_por_ciento_de_FR011_y_FR012_visible_sin_ausencias_ni_extras()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        var barra = barraMenu(ventana);
        var barraFormato = BarraFormato(ventana);

        // Barras persistentes y visibles desde el arranque (AC-002, AC-003).
        Assert.True(barra.IsVisible);
        Assert.True(barraFormato.IsVisible);

        // FR-011 al 100 %: los 6 menús y las 48 entradas, en el orden
        // declarado del catálogo, sin ausencias ni elementos de más
        // (umbral 0/0). Los dos valores de idioma de la entrada
        // Herramientas → Idioma de la interfaz (T-010) también forman parte
        // de la SC-001 declarada.
        var menus = barra.Items.OfType<MenuItem>().ToList();
        Assert.Equal(CatalogoMenuPrincipal.TotalMenus, menus.Count);
        Assert.Equal(6, menus.Count);
        Assert.Equal(
            CatalogoMenuPrincipal.Menus.Select(menu => menu.Clave),
            menus.Select(menu => menu.Tag as string));

        var entradas = menus.SelectMany(menu => menu.Items.OfType<MenuItem>()).ToList();
        Assert.Equal(CatalogoMenuPrincipal.TotalEntradas, entradas.Count);
        Assert.Equal(48, entradas.Count);
        Assert.Equal(
            CatalogoMenuPrincipal.Menus.SelectMany(menu => menu.Entradas.Select(entrada => entrada.Clave)),
            entradas.Select(entrada => entrada.Tag as string));

        var valoresDeIdioma = entradas.Single(
                entrada => (string?)entrada.Tag == ConstructorDeMenuPrincipal.ClaveEntradaIdioma)
            .Items.OfType<MenuItem>().ToList();
        Assert.Equal(
            CatalogoIdiomas.Valores.Select(ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma),
            valoresDeIdioma.Select(valor => valor.Tag as string));

        // 0 elementos no declarados: todo MenuItem de la ventana lleva una
        // clave de los catálogos —menús, entradas o valores de idioma—.
        var clavesDeclaradas = new HashSet<string>(
            CatalogoMenuPrincipal.Menus.Select(menu => menu.Clave)
                .Concat(CatalogoMenuPrincipal.Menus.SelectMany(menu => menu.Entradas.Select(entrada => entrada.Clave)))
                .Concat(CatalogoIdiomas.Valores.Select(ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma)));
        var todos = ElementosDeMenu(barra).ToList();
        Assert.Equal(CatalogoMenuPrincipal.TotalMenus + CatalogoMenuPrincipal.TotalEntradas + valoresDeIdioma.Count, todos.Count);
        Assert.All(todos, elemento => Assert.True(
            clavesDeclaradas.Contains((string?)elemento.Tag ?? string.Empty),
            $"Elemento de menú no declarado: {elemento.Tag}"));

        // FR-012 al 100 %: los 12 controles en orden de escritura, sin
        // ausencias ni elementos de más (umbral 0/0), cada uno visible,
        // activable, con icono (VR-001) y aviso flotante (VR-002).
        var controles = barraFormato.Children.OfType<Button>().ToList();
        Assert.Equal(CatalogoBarraFormato.TotalControles, controles.Count);
        Assert.Equal(12, controles.Count);
        Assert.Equal(
            CatalogoBarraFormato.Controles.Select(control => control.Clave),
            controles.Select(control => control.Tag as string));
        Assert.All(controles, control =>
        {
            Assert.True(control.IsVisible);
            Assert.True(control.IsEnabled);
            Assert.False(string.IsNullOrEmpty(Assert.IsType<TextBlock>(control.Content).Text));
            Assert.False(string.IsNullOrWhiteSpace(ToolTip.GetTip(control) as string));
        });

        // Arranque en español de la ventana única (AC-001): idioma activo del
        // defecto de la sesión y marca de radio en Español, solo en él.
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.True(valorDeIdioma(ventana, IdiomaDeInterfaz.Español).IsChecked);
        Assert.False(valorDeIdioma(ventana, IdiomaDeInterfaz.English).IsChecked);
    }

    // --- DS-002: tasa de claves resueltas del 100 % en SC-001 ---

    [Fact]
    public void Sc001_resuelve_el_100_por_ciento_de_sus_textos_tras_el_arranque_y_tras_cada_conmutacion()
    {
        // Umbral 0 rótulos ni avisos sin traducir (BR-F001-007): tras el
        // arranque en español y tras cada conmutación es/en, todos los
        // rótulos de menú —incluidos los valores de idioma— y todos los
        // avisos flotantes de la SC-001 real están resueltos, sin nulos, sin
        // vacíos y sin restos del idioma anterior (ningún cambio parcial).
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);

        foreach (var idioma in new[]
                 {
                     IdiomaDeInterfaz.Español, // arranque (BR-007)
                     IdiomaDeInterfaz.English, // AC-007: es → en
                     IdiomaDeInterfaz.Español, // AC-008: en → es
                 })
        {
            if (idioma != sesion.IdiomaActivo)
            {
                ventana.ServicioDeIdioma.Conmutar(idioma);
            }

            Assert.All(textosVisibles(ventana), texto =>
            {
                Assert.False(string.IsNullOrWhiteSpace(texto.Valor));
                Assert.Equal(CatalogoCadenasI18n.Resolver(texto.Clave, idioma), texto.Valor);
            });
        }
    }

    // --- DS-003: conmutación en caliente sin reinicio ni cambio parcial ---

    [Fact]
    public void Las_conmutaciones_es_en_son_completas_en_la_misma_sesion_sin_reinicio_ni_cambio_parcial()
    {
        // 100 % de las conmutaciones completadas en la misma sesión
        // conservando ventana y estado: dos ciclos es→en→es sobre la misma
        // ventana y la misma sesión, cada conmutación efectiva y completa
        // (todos los textos re-resueltos en el idioma nuevo), con el estado
        // del menú y el aviso flotante intactos en cada salto y sin reinicio
        // —misma sesión, la del arranque— al final del ciclo (BR-F001-005).
        var sesion = new SesionDeAplicacion
        {
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var ventana = new MainWindow(sesion);
        var identificadorDeArranque = sesion.IdentificadorDeSesion;

        for (var ciclo = 0; ciclo < 2; ciclo++)
        {
            Assert.Equal(EstadoConmutacionIdioma.Conmutada, ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English).Estado);
            AssertConservaVentanaYEstado(ventana, sesion, IdiomaDeInterfaz.English);

            Assert.Equal(EstadoConmutacionIdioma.Conmutada, ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.Español).Estado);
            AssertConservaVentanaYEstado(ventana, sesion, IdiomaDeInterfaz.Español);
        }

        Assert.Equal(identificadorDeArranque, sesion.IdentificadorDeSesion);
    }

    // --- AC-004, AC-005: entrada y control sin acción, documento intacto ---

    [Fact]
    public void Pulsar_una_entrada_y_un_control_sin_accion_no_altera_el_documento_ni_el_estado()
    {
        // ADR-001 integrado en la SC-001 real: pulsar una entrada cuya acción
        // pertenece a otra Feature (Archivo → Nuevo, F-028) y pulsar un
        // control de la barra abre y cierra sin efecto sobre el documento —
        // sigue vacío— ni sobre el estado de la sesión (AC-004, AC-005).
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        var estadoAntes = (sesion.IdiomaActivo, sesion.EstadoMenuPrincipal, sesion.AvisoFlotanteVisible);

        var nuevo = ElementosDeMenu(barraMenu(ventana))
            .Single(entrada => (string?)entrada.Tag == "menu.archivo.nuevo");
        nuevo.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        var control = BarraFormato(ventana).Children.OfType<Button>().First();
        control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(estadoAntes, (sesion.IdiomaActivo, sesion.EstadoMenuPrincipal, sesion.AvisoFlotanteVisible));
        Assert.Null(ventana.FindControl<Border>("AreaDeDocumento")!.Child);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
    }

    // --- Ayudantes ---

    private static Menu barraMenu(MainWindow ventana) =>
        ventana.FindControl<Menu>("BarraMenu") ?? throw new InvalidOperationException("SC-001 sin barra de menú.");

    private static StackPanel BarraFormato(MainWindow ventana) =>
        ventana.FindControl<StackPanel>("BarraFormato") ?? throw new InvalidOperationException("SC-001 sin barra de formato.");

    private static MenuItem valorDeIdioma(MainWindow ventana, IdiomaDeInterfaz idioma) =>
        ElementosDeMenu(barraMenu(ventana)).Single(
            item => (string?)item.Tag == ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma(idioma));

    private static IEnumerable<MenuItem> ElementosDeMenu(Menu barra) =>
        barra.Items.OfType<MenuItem>().SelectMany(ElementosDeMenu);

    private static IEnumerable<MenuItem> ElementosDeMenu(MenuItem item)
    {
        yield return item;

        foreach (var hijo in item.Items.OfType<MenuItem>())
        {
            foreach (var elemento in ElementosDeMenu(hijo))
            {
                yield return elemento;
            }
        }
    }

    /// <summary>Los 68 textos visibles de la SC-001: rótulos de menús, entradas y valores de idioma, más los avisos flotantes de la barra.</summary>
    private static IEnumerable<(string Clave, string Valor)> textosVisibles(MainWindow ventana)
    {
        foreach (var item in ElementosDeMenu(barraMenu(ventana)))
        {
            yield return ((string)item.Tag!, (string?)item.Header
                ?? throw new InvalidOperationException($"El elemento {item.Tag} no tiene rótulo."));
        }

        foreach (var control in BarraFormato(ventana).Children.OfType<Button>())
        {
            yield return ((string)control.Tag!, ToolTip.GetTip(control) as string
                ?? throw new InvalidOperationException($"El control {control.Tag} no tiene aviso flotante."));
        }
    }

    /// <summary>Textos de la SC-001 que no están en el idioma dado: 0 es el umbral de DS-002 y el detector de cambios parciales de DS-003.</summary>
    private static IReadOnlyList<(string Clave, string Valor)> textosDivergentes(MainWindow ventana, IdiomaDeInterfaz idioma) =>
        textosVisibles(ventana)
            .Where(texto => CatalogoCadenasI18n.Resolver(texto.Clave, idioma) != texto.Valor)
            .ToList();

    private static void AssertConservaVentanaYEstado(
        MainWindow ventana, SesionDeAplicacion sesion, IdiomaDeInterfaz idiomaEsperado)
    {
        Assert.Equal(idiomaEsperado, sesion.IdiomaActivo);
        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);

        // Sin cambio parcial: los 68 textos de la ventana quedan en el idioma
        // nuevo, ninguno en el anterior.
        Assert.Empty(textosDivergentes(ventana, idiomaEsperado));

        // La ventana sigue servida por la misma sesión (sin reinicio): el
        // servicio de la ventana y la sesión coinciden en el idioma activo.
        Assert.Equal(sesion.IdiomaActivo, ventana.ServicioDeIdioma.IdiomaActivo);
    }
}
