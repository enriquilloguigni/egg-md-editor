using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Configuración de ejecución del proceso (T-012): el punto de entrada fija
// que la aplicación arranca siempre en español sin escribir nada a disco
// (BR-F001-006) y sin acceso a red (C-001) — la sesión nace nueva con sus
// defectos de la Spec y no se lee preferencia alguna —, y cablea el arranque
// que T-011 difirió: LOG-001 con inicio del proceso y fin de la
// inicialización cuando SC-001 se presenta completa (AC-001), ERR-001/ERR-002
// con código de salida de error y sin ventana presentada (AC-012), y el
// código de salida que el cierre (COM-004, T-011) deja en el proceso.
public class ConfiguracionDeEjecucionTests
{
    [Fact]
    public void El_arranque_registra_log001_con_inicio_y_fin_de_inicializacion()
    {
        // LOG-001 (T-007, cableado T-012): el proceso registra el inicio y,
        // presentada SC-001 completa por el cableado de la App, el fin de la
        // inicialización — ambos con el identificador de sesión compartido.
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var errores = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(sesion, registros, errores, reloj: () => Instante);
        var arranque = new ArranqueDeLaAplicacion(sesion, registrador, lectorDeCodigoDeSalida: () => 0);

        var codigo = arranque.Ejecutar(() => CablearYPresentarVentana(sesion, registrador));

        Assert.Equal(0, codigo);
        Assert.Equal(2, registros.Lineas.Count);
        Assert.Contains("[Info]", registros.Lineas[0]);
        Assert.Contains("LOG-001: inicio del proceso", registros.Lineas[0]);
        Assert.Contains("idioma activo es (defecto de sesión", registros.Lineas[0]);
        Assert.Contains(sesion.IdentificadorDeSesion, registros.Lineas[0]);
        Assert.Contains("[Info]", registros.Lineas[1]);
        Assert.Contains("LOG-001: fin de la inicialización", registros.Lineas[1]);
        Assert.Contains("catálogo i18n cargado", registros.Lineas[1]);
        Assert.Contains("tiempo de arranque", registros.Lineas[1]);
        Assert.Contains(sesion.IdentificadorDeSesion, registros.Lineas[1]);

        // C-001: nada va a la salida de error en un arranque correcto; todo
        // registro sale por la salida estándar, sin escritura a disco.
        Assert.Equal(string.Empty, errores.ToString());
    }

    [Fact]
    public void Cada_arranque_parte_del_defecto_espanol_sin_leer_nada_del_disco()
    {
        // BR-F001-006 (AC-001, AC-011): una sesión que terminó en inglés no
        // condiciona el arranque siguiente — no hay nada escrito que leer — y
        // la ventana nueva se presenta en español con Español marcado.
        var sesionAnterior = new SesionDeAplicacion();
        var arranqueAnterior = CrearArranque(sesionAnterior, new SalidaDeRegistrosEnMemoria());
        arranqueAnterior.Ejecutar(() =>
        {
            var (_, ventanaAnterior) = CablearYPresentarVentana(
                sesionAnterior, arranqueAnterior.RegistradorDeCicloDeVida);
            ventanaAnterior.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);
        });
        Assert.Equal(IdiomaDeInterfaz.English, sesionAnterior.IdiomaActivo);

        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var arranque = CrearArranque(sesion, registros);

        arranque.Ejecutar(() =>
        {
            var (_, ventana) = CablearYPresentarVentana(sesion, arranque.RegistradorDeCicloDeVida);
            ventana.Show();

            var (espanol, english) = ValoresDeIdioma(ventana);

            // AC-001: interfaz en español, un único idioma activo con
            // Herramientas → Idioma de la interfaz marcado en Español.
            Assert.True(espanol.IsChecked);
            Assert.False(english.IsChecked);
            Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        });

        // El nuevo LOG-001 nace del defecto de sesión, no de la sesión anterior.
        Assert.Contains(
            $"idioma activo es (defecto de sesión",
            registros.Lineas[0]);
    }

    [Fact]
    public void La_ventana_del_proceso_comparte_la_sesion_y_emite_log003_al_cerrar()
    {
        // El cableado del proceso (T-012) pasa a la ventana única la sesión
        // del arranque y su registrador: conmutar se ve en la sesión y el
        // cierre emite LOG-003 correlacionado (COM-004, T-011).
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var arranque = CrearArranque(sesion, registros);

        arranque.Ejecutar(() =>
        {
            var (_, ventana) = CablearYPresentarVentana(sesion, arranque.RegistradorDeCicloDeVida);
            ventana.Show();

            ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);
            Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);

            ventana.Close();
        });

        Assert.Contains(registros.Lineas, linea => linea.Contains("LOG-003: inicio del cierre"));
        Assert.Contains(registros.Lineas, linea => linea.Contains("LOG-003: fin del proceso"));
        Assert.All(registros.Lineas, linea => Assert.Contains(sesion.IdentificadorDeSesion, linea));
    }

    [Fact]
    public void El_fallo_de_inicializacion_aplica_err001_y_devuelve_codigo_de_error()
    {
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var errores = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(sesion, registros, errores, reloj: () => Instante);
        var arranque = new ArranqueDeLaAplicacion(sesion, registrador, lectorDeCodigoDeSalida: () => 0);
        var excepcion = new InvalidOperationException("el catálogo i18n inicial no carga");

        var codigo = arranque.Ejecutar(() => throw excepcion);

        // AC-012 (ERR-001): el proceso finaliza con código de error, con el
        // mensaje en español en la salida de error y el fallo registrado
        // (LOG-001, nivel Error); no se presenta ninguna ventana.
        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, codigo);
        Assert.Contains("Error al iniciar la aplicación:", errores.ToString());
        Assert.Contains(excepcionEsperadaDe(excepcion), errores.ToString());
        Assert.Equal(2, registros.Lineas.Count);
        Assert.Contains("LOG-001: inicio del proceso", registros.Lineas[0]);
        Assert.Contains("[Error]", registros.Lineas[1]);
        Assert.Contains("LOG-001: arranque fallido", registros.Lineas[1]);
        Assert.DoesNotContain("fin de la inicialización", registros.Lineas[1]);
    }

    [Fact]
    public void El_fallo_al_pintar_la_ventana_tras_su_creacion_aplica_err002()
    {
        // ERR-002: ventana creada que no llega a presentarse completa — la
        // excepción aparece al presentarla, tras su creación —. El tratamiento
        // es el mismo resultado de LOG-001: código de error y sin ventana
        // usable.
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var errores = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(sesion, registros, errores, reloj: () => Instante);
        var arranque = new ArranqueDeLaAplicacion(sesion, registrador, lectorDeCodigoDeSalida: () => 0);

        var codigo = arranque.Ejecutar(() =>
        {
            var ventana = new MainWindow(sesion, registradorDeCicloDeVida: registrador);
            ventana.Opened += (_, _) =>
                throw new IOException("fallo al pintar SC-001 al presentarse");
            ventana.Show();
        });

        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, codigo);
        Assert.Contains("Error al iniciar la aplicación:", errores.ToString());
        Assert.Contains(typeof(IOException).FullName + ": fallo al pintar SC-001 al presentarse", errores.ToString());
        Assert.Equal(2, registros.Lineas.Count);
        Assert.Contains("LOG-001: inicio del proceso", registros.Lineas[0]);
        Assert.Contains("[Error]", registros.Lineas[1]);
        Assert.Contains("LOG-001: arranque fallido", registros.Lineas[1]);

        // La ventana no llegó a presentarse completa: no hay fin de la
        // inicialización registrado.
        Assert.DoesNotContain(
            registros.Lineas,
            linea => linea.Contains("fin de la inicialización"));
    }

    [Fact]
    public void El_arranque_correcto_devuelve_el_codigo_de_salida_que_dejo_el_cierre()
    {
        // El cierre (COM-004, T-011) fija el código de salida del proceso: 0
        // en cierre limpio o el de ERR-005 si el cierre o la liberación
        // fallaron; el punto de entrada lo devuelve tal cual.
        var sesion = new SesionDeAplicacion();
        var arranqueLimpio = CrearArranque(
            sesion, new SalidaDeRegistrosEnMemoria(), lectorDeCodigoDeSalida: () => 0);
        var arranqueConFalloDeCierre = CrearArranque(
            sesion,
            new SalidaDeRegistrosEnMemoria(),
            lectorDeCodigoDeSalida: () => RegistradorDeCicloDeVida.CodigoSalidaPorFallo);

        Assert.Equal(0, arranqueLimpio.Ejecutar(() => { }));
        Assert.Equal(
            RegistradorDeCicloDeVida.CodigoSalidaPorFallo,
            arranqueConFalloDeCierre.Ejecutar(() => { }));
    }

    // --- Ayudantes ---

    private static readonly DateTimeOffset Instante = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private static ArranqueDeLaAplicacion CrearArranque(
        SesionDeAplicacion sesion,
        SalidaDeRegistrosEnMemoria registros,
        Func<int>? lectorDeCodigoDeSalida = null) =>
        new(
            sesion,
            new RegistradorDeCicloDeVida(sesion, registros, reloj: () => Instante),
            lectorDeCodigoDeSalida);

    /// <summary>
    /// Cablea la App del proceso sobre un lifetime de escritorio en la
    /// plataforma headless y presenta la ventana única, como hace el arranque
    /// real de Avalonia (T-012).
    /// </summary>
    private static (ClassicDesktopStyleApplicationLifetime Lifetime, MainWindow Ventana)
        CablearYPresentarVentana(SesionDeAplicacion sesion, RegistradorDeCicloDeVida registrador)
    {
        var app = new App(sesion, registrador);
        app.Initialize();

        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        app.ApplicationLifetime = lifetime;
        app.OnFrameworkInitializationCompleted();

        var ventana = Assert.IsType<MainWindow>(lifetime.MainWindow);
        ventana.Show();

        return (lifetime, ventana);
    }

    private static (MenuItem Espanol, MenuItem English) ValoresDeIdioma(MainWindow ventana)
    {
        var barra = ventana.FindControl<Menu>("BarraMenu")
            ?? throw new InvalidOperationException("SC-001 sin barra de menú.");

        var valores = barra.Items
            .Cast<MenuItem>()
            .SelectMany(menu => menu.Items.Cast<MenuItem>())
            .Single(entrada => (string?)entrada.Tag == ConstructorDeMenuPrincipal.ClaveEntradaIdioma)
            .Items
            .Cast<MenuItem>()
            .ToList();

        return (valores[0], valores[1]);
    }

    private static string excepcionEsperadaDe(Exception excepcion) =>
        $"{excepcion.GetType().FullName}: {excepcion.Message}";

    /// <summary>Salida de registros en memoria, para verificar las líneas emitidas.</summary>
    private sealed class SalidaDeRegistrosEnMemoria : ISalidaDeRegistros
    {
        public List<string> Lineas { get; } = [];

        public void Escribir(NivelRegistro nivel, string mensaje) => Lineas.Add($"[{nivel}] {mensaje}");
    }
}
