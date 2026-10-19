using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Cableado de COM-002 en SC-001 (T-011): el teclado de la ventana traduce
// cada gesto al modelo de combinaciones y lo despacha (T-005). El modificador
// primario es Ctrl en Windows/Linux y Cmd en macOS —la misma combinación
// lógica del registro único—, el resto de atajos queda inerte (BR-F001-003,
// ADR-001) y el gesto despachado no sigue propagándose.
public class CableDeAtajosTests
{
    private static readonly CombinacionDeTeclas AlternarIdioma =
        new(ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Mayusculas, 'L');

    private static readonly CombinacionDeTeclas CerrarAplicacion =
        new(ModificadoresDeTeclas.Primario, 'Q');

    // --- Traducción de gestos físicos al modelo (COM-002, T-011) ---

    [Fact]
    public void Ctrl_shift_l_se_traduce_a_la_combinacion_del_registro()
    {
        var combinacion = TraductorDeGestos.Traducir(
            Key.L, KeyModifiers.Control | KeyModifiers.Shift, enMacOS: false);

        Assert.Equal(AlternarIdioma, combinacion);
    }

    [Fact]
    public void Cmd_shift_l_en_macos_se_traduce_a_la_misma_combinacion_logica()
    {
        var combinacion = TraductorDeGestos.Traducir(
            Key.L, KeyModifiers.Meta | KeyModifiers.Shift, enMacOS: true);

        Assert.Equal(AlternarIdioma, combinacion);
    }

    [Fact]
    public void Ctrl_q_y_cmd_q_se_traducen_al_atajo_de_cierre()
    {
        Assert.Equal(
            CerrarAplicacion,
            TraductorDeGestos.Traducir(Key.Q, KeyModifiers.Control, enMacOS: false));
        Assert.Equal(
            CerrarAplicacion,
            TraductorDeGestos.Traducir(Key.Q, KeyModifiers.Meta, enMacOS: true));
    }

    [Fact]
    public void El_ctrl_fisico_en_macos_no_es_el_modificador_primario()
    {
        // En macOS el primario del modelo es Cmd: Ctrl+Shift+L físico no es
        // el atajo asignado y queda una combinación sin primario, inerte.
        var combinacion = TraductorDeGestos.Traducir(
            Key.L, KeyModifiers.Control | KeyModifiers.Shift, enMacOS: true);

        Assert.Equal(new CombinacionDeTeclas(ModificadoresDeTeclas.Mayusculas, 'L'), combinacion);
    }

    [Fact]
    public void Alt_se_traduce_al_modificador_alternativo()
    {
        var combinacion = TraductorDeGestos.Traducir(Key.L, KeyModifiers.Alt, enMacOS: false);

        Assert.Equal(new CombinacionDeTeclas(ModificadoresDeTeclas.Alternativa, 'L'), combinacion);
    }

    [Fact]
    public void Los_digitos_se_traducen_en_los_dos_bloques_de_teclado()
    {
        Assert.Equal(
            new CombinacionDeTeclas(ModificadoresDeTeclas.Ninguno, '1'),
            TraductorDeGestos.Traducir(Key.D1, KeyModifiers.None, enMacOS: false));
        Assert.Equal(
            new CombinacionDeTeclas(ModificadoresDeTeclas.Ninguno, '5'),
            TraductorDeGestos.Traducir(Key.NumPad5, KeyModifiers.None, enMacOS: false));
    }

    [Fact]
    public void Las_teclas_sin_letra_ni_digito_no_son_expresables()
    {
        Assert.Null(TraductorDeGestos.Traducir(Key.Enter, KeyModifiers.Control, enMacOS: false));
        Assert.Null(TraductorDeGestos.Traducir(Key.F5, KeyModifiers.None, enMacOS: false));
        Assert.Null(TraductorDeGestos.Traducir(Key.Left, KeyModifiers.None, enMacOS: false));
        Assert.Null(TraductorDeGestos.Traducir(Key.Escape, KeyModifiers.None, enMacOS: true));
    }

    // --- Cableado del teclado de SC-001 (COM-002) ---

    [Fact]
    public void Ctrl_shift_l_en_la_ventana_alterna_el_idioma_con_relabelizado()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();

        ventana.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control | RawInputModifiers.Shift);

        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal("File", HeaderDeMenu(ventana, "menu.archivo"));

        // Dos pulsaciones vuelven al español con sus rótulos (alternancia).
        ventana.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control | RawInputModifiers.Shift);

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal("Archivo", HeaderDeMenu(ventana, "menu.archivo"));
    }

    [Fact]
    public void Ctrl_q_en_la_ventana_cierra_la_aplicacion()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();
        var cerrada = false;
        ventana.Closed += (_, _) => cerrada = true;

        ventana.KeyPressQwerty(PhysicalKey.Q, RawInputModifiers.Control);

        Assert.True(cerrada);
    }

    [Fact]
    public void Las_combinaciones_no_asignadas_quedan_inertes()
    {
        // BR-F001-003 (ADR-001): sin efecto observable sobre la interfaz.
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();

        // Ctrl+X no está asignada; Ctrl+L sin Shift tampoco (solo altera
        // Ctrl+Shift+L); Shift+L sin primario, tampoco.
        ventana.KeyPressQwerty(PhysicalKey.X, RawInputModifiers.Control | RawInputModifiers.Shift);
        ventana.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control);
        ventana.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Shift);

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal("Archivo", HeaderDeMenu(ventana, "menu.archivo"));
    }

    private static string HeaderDeMenu(MainWindow ventana, string clave)
    {
        var barra = ventana.FindControl<Avalonia.Controls.Menu>("BarraMenu")!;
        var elementos = barra.Items
            .OfType<Avalonia.Controls.MenuItem>()
            .SelectMany(menu => menu.Items.OfType<Avalonia.Controls.MenuItem>())
            .Concat(barra.Items.OfType<Avalonia.Controls.MenuItem>());

        return elementos.Single(item => (string?)item.Tag == clave).Header as string
            ?? throw new InvalidOperationException($"El elemento {clave} no tiene rótulo.");
    }
}
