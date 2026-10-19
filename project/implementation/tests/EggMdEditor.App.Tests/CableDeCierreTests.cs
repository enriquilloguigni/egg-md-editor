using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Cableado de COM-004 en SC-001 (T-011): el cierre de la ventana —por atajo,
// por la orden del sistema o por el usuario— libera la sesión con su registro
// LOG-003 (T-007), confirma la liberación y no persiste nada (AC-011,
// FR-013 → F-030, BR-F001-006); ante fallo de cierre o de liberación, ERR-005
// con mensaje en español por la salida de error y el proceso terminándose
// igualmente con código de error (AC-013).
public class CableDeCierreTests
{
    [Fact]
    public void Cerrar_registra_log003_y_confirma_la_liberacion_de_la_sesion()
    {
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var gestor = new GestorDeCierreDeSesion(
            sesion,
            new RegistradorDeCicloDeVida(sesion, registros, reloj: () => Instante));

        gestor.Cerrar();

        Assert.True(gestor.SesionLiberada);
        Assert.Equal(0, gestor.CodigoDeSalidaPendiente);

        // LOG-003: inicio del cierre y fin del proceso, correlacionados por
        // el identificador de sesión (NFR-003).
        Assert.Equal(2, registros.Lineas.Count);
        Assert.Contains("LOG-003: inicio del cierre de SC-001 y de la liberación", registros.Lineas[0]);
        Assert.Contains("LOG-003: fin del proceso; cierre limpio sin procesos residuales", registros.Lineas[1]);
        Assert.Contains(sesion.IdentificadorDeSesion, registros.Lineas[0]);
        Assert.Contains(sesion.IdentificadorDeSesion, registros.Lineas[1]);
    }

    [Fact]
    public void Cerrar_sin_registrador_libera_y_confirma_sin_registros()
    {
        var gestor = new GestorDeCierreDeSesion(new SesionDeAplicacion());

        gestor.Cerrar();

        Assert.True(gestor.SesionLiberada);
        Assert.Equal(0, gestor.CodigoDeSalidaPendiente);
    }

    [Fact]
    public void El_fallo_al_cerrar_aplica_err005_y_deja_codigo_de_salida_de_error()
    {
        var sesion = new SesionDeAplicacion();
        var errores = new StringWriter();
        var registros = new SalidaQueFallaALaPrimera();
        var gestor = new GestorDeCierreDeSesion(
            sesion,
            new RegistradorDeCicloDeVida(sesion, registros, errores, reloj: () => Instante));

        var fallida = Assert.Throws<IOException>(gestor.Cerrar);

        // AC-013 (ERR-005): mensaje en español por la salida de error, fallo
        // registrado (LOG-003, nivel Error) y el proceso se termina igualmente.
        Assert.Contains("Error al cerrar la aplicación:", errores.ToString());
        Assert.Contains(excepcionEsperadaDe(fallida), errores.ToString());
        Assert.Contains("cierre fallido", registros.Lineas[0]);

        Assert.False(gestor.SesionLiberada);
        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, gestor.CodigoDeSalidaPendiente);
    }

    [Fact]
    public void Cerrar_la_ventana_libera_la_sesion_con_su_registro_log003()
    {
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var ventana = new MainWindow(
            sesion,
            registradorDeCicloDeVida: new RegistradorDeCicloDeVida(sesion, registros, reloj: () => Instante));
        ventana.Show();
        var cerrada = false;
        ventana.Closed += (_, _) => cerrada = true;

        ventana.Close();

        // COM-004: confirmado el cierre, la sesión queda liberada (ST-004).
        Assert.True(cerrada);
        Assert.False(ventana.IsVisible);
        Assert.Equal(2, registros.Lineas.Count);
        Assert.Contains("LOG-003: inicio del cierre", registros.Lineas[0]);
        Assert.Contains("LOG-003: fin del proceso", registros.Lineas[1]);
    }

    [Fact]
    public void El_atajo_de_cierre_cierra_la_ventana_por_el_mismo_camino_de_com004()
    {
        // Cadena COM-002 → COM-004: Ctrl/Cmd+Q despacha el cierre y la
        // ventana cierra liberando la sesión con su registro.
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var ventana = new MainWindow(
            sesion,
            registradorDeCicloDeVida: new RegistradorDeCicloDeVida(sesion, registros, reloj: () => Instante));
        ventana.Show();
        var cerrada = false;
        ventana.Closed += (_, _) => cerrada = true;

        var resultado = ventana.DespachadorDeAtajos.Despachar(
            new CombinacionDeTeclas(ModificadoresDeTeclas.Primario, 'Q'));

        Assert.Equal(EstadoDespachoAtajo.Despachado, resultado.Estado);
        Assert.Equal(AccionDeAtajo.CerrarAplicacion, resultado.Accion);
        Assert.True(cerrada);
        Assert.Contains("LOG-003", string.Join("\n", registros.Lineas));
    }

    [Fact]
    public void La_sesion_liberada_no_persiste_la_eleccion_de_idioma()
    {
        // AC-011: cerrar con inglés activo no escribe nada a disco y la
        // elección muere con la sesión (FR-013 → F-030, BR-F001-006). En el
        // proceso: nada se escribe y una sesión nueva arranca en español.
        var sesion = new SesionDeAplicacion();
        var registros = new SalidaDeRegistrosEnMemoria();
        var ventana = new MainWindow(
            sesion,
            registradorDeCicloDeVida: new RegistradorDeCicloDeVida(sesion, registros, reloj: () => Instante));
        ventana.Show();

        ventana.ServicioDeIdioma.Conmutar(IdiomaDeInterfaz.English);
        ventana.Close();

        Assert.False(ventana.IsVisible);
        Assert.All(registros.Lineas, linea => Assert.DoesNotContain("persist", linea));
        Assert.Equal(IdiomaDeInterfaz.Español, new SesionDeAplicacion().IdiomaActivo);
    }

    // --- Ayudantes ---

    private static readonly DateTimeOffset Instante = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private static string excepcionEsperadaDe(Exception excepcion) =>
        $"{excepcion.GetType().FullName}: {excepcion.Message}";

    /// <summary>Salida de registros en memoria, para verificar las líneas emitidas.</summary>
    private sealed class SalidaDeRegistrosEnMemoria : ISalidaDeRegistros
    {
        public List<string> Lineas { get; } = [];

        public void Escribir(NivelRegistro nivel, string mensaje) => Lineas.Add($"[{nivel}] {mensaje}");
    }

    /// <summary>
    /// Salida cuyo primer registro falla —el canal de registros se rompe en
    /// el momento de registrar el inicio del cierre— y que captura los
    /// siguientes, para ejercitar el camino ERR-005 completo.
    /// </summary>
    private sealed class SalidaQueFallaALaPrimera : ISalidaDeRegistros
    {
        private int _llamadas;

        public List<string> Lineas { get; } = [];

        public void Escribir(NivelRegistro nivel, string mensaje)
        {
            if (++_llamadas == 1)
            {
                throw new IOException("La salida de registros no está disponible.");
            }

            Lineas.Add($"[{nivel}] {mensaje}");
        }
    }
}
