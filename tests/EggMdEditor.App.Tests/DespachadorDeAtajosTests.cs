using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Despachador de atajos (COM-002): registro único de combinaciones asignadas
// en el Plan —Ctrl+Shift+L (macOS Cmd+Shift+L) alterna es/en vía COM-001 y
// Ctrl+Q (macOS Cmd+Q) cierra vía COM-004—, con cualquier otro atajo inerte
// (BR-F001-003, ADR-001) y sin respuesta para la acción de cierre mientras
// su contraparte no esté conectada (COM-002).
public class DespachadorDeAtajosTests
{
    private static readonly CombinacionDeTeclas AlternarIdioma =
        new(ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Mayusculas, 'L');

    private static readonly CombinacionDeTeclas CerrarAplicacion =
        new(ModificadoresDeTeclas.Primario, 'Q');

    [Fact]
    public void El_registro_unico_contiene_exactamente_las_combinaciones_asignadas_en_el_plan()
    {
        var registro = DespachadorDeAtajos.CombinacionesAsignadas;

        // Registro único y centralizado: dos asignaciones, sin duplicados.
        Assert.Equal(2, registro.Count);

        // Ctrl/Cmd+Shift+L → alternar es/en: el modificador primario es el
        // mismo en Windows/Linux (Ctrl) y macOS (Cmd), por lo que la forma
        // del Plan «Ctrl+Shift+L (macOS Cmd+Shift+L)» es una única entrada.
        Assert.True(registro.TryGetValue(AlternarIdioma, out var accionIdioma));
        Assert.Equal(AccionDeAtajo.AlternarIdioma, accionIdioma);

        // Ctrl/Cmd+Q → cerrar la aplicación.
        Assert.True(registro.TryGetValue(CerrarAplicacion, out var accionCierre));
        Assert.Equal(AccionDeAtajo.CerrarAplicacion, accionCierre);
    }

    [Fact]
    public void Despachar_ctrl_shift_l_alterna_de_espanol_a_english_via_com001()
    {
        var sesion = new SesionDeAplicacion
        {
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var servicioDeIdioma = new ServicioConmutacionIdioma(sesion);
        var despachador = new DespachadorDeAtajos(servicioDeIdioma);
        var notificaciones = new List<IdiomaConmutadoEventArgs>();
        servicioDeIdioma.IdiomaConmutado += (_, e) => notificaciones.Add(e);

        var resultado = despachador.Despachar(AlternarIdioma);

        Assert.Equal(EstadoDespachoAtajo.Despachado, resultado.Estado);
        Assert.Equal(AccionDeAtajo.AlternarIdioma, resultado.Accion);
        Assert.NotNull(resultado.Conmutacion);
        Assert.Equal(EstadoConmutacionIdioma.Conmutada, resultado.Conmutacion!.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, resultado.Conmutacion.IdiomaOrigen);
        Assert.Equal(IdiomaDeInterfaz.English, resultado.Conmutacion.IdiomaDestino);

        // COM-001 aplicó el cambio a la sesión con su única notificación DS-003.
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Single(notificaciones);
        Assert.Equal(IdiomaDeInterfaz.Español, notificaciones[0].IdiomaAnterior);
        Assert.Equal(IdiomaDeInterfaz.English, notificaciones[0].IdiomaNuevo);

        // BR-F001-005: el despacho solo toca el idioma vía COM-001; el resto
        // del estado de la sesión queda intacto.
        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);
    }

    [Fact]
    public void Despachar_ctrl_shift_l_dos_veces_alterna_y_vuelve_al_idioma_anterior()
    {
        var sesion = new SesionDeAplicacion();
        var despachador = new DespachadorDeAtajos(new ServicioConmutacionIdioma(sesion));

        despachador.Despachar(AlternarIdioma);
        despachador.Despachar(AlternarIdioma);

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
    }

    [Fact]
    public void Despachar_ctrl_q_sin_contraparte_conectada_no_produce_respuesta()
    {
        var sesion = new SesionDeAplicacion();
        var despachador = new DespachadorDeAtajos(new ServicioConmutacionIdioma(sesion));

        var resultado = despachador.Despachar(CerrarAplicacion);

        // COM-002: combinación asignada cuya acción aún no existe — sin
        // respuesta (T-011 conectará la finalización de la sesión, COM-004).
        Assert.Equal(EstadoDespachoAtajo.SinAccionConectada, resultado.Estado);
        Assert.Equal(AccionDeAtajo.CerrarAplicacion, resultado.Accion);
        Assert.Null(resultado.Conmutacion);

        // Sin efecto observable en la sesión.
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal(EstadoMenuPrincipal.Cerrado, sesion.EstadoMenuPrincipal);
        Assert.False(sesion.AvisoFlotanteVisible);
    }

    [Fact]
    public void Despachar_ctrl_q_con_contraparte_conectada_la_invoca_una_vez()
    {
        var sesion = new SesionDeAplicacion();
        var cierre = new CierreCapturado();
        var despachador = new DespachadorDeAtajos(
            new ServicioConmutacionIdioma(sesion), accionDeCierre: cierre);

        var resultado = despachador.Despachar(CerrarAplicacion);

        Assert.Equal(EstadoDespachoAtajo.Despachado, resultado.Estado);
        Assert.Equal(AccionDeAtajo.CerrarAplicacion, resultado.Accion);
        Assert.Null(resultado.Conmutacion);
        Assert.Equal(1, cierre.Llamadas);
    }

    [Theory]
    [InlineData(ModificadoresDeTeclas.Ninguno, 'L')]                              // sin primario
    [InlineData(ModificadoresDeTeclas.Mayusculas, 'L')]                           // Shift+L sin primario
    [InlineData(ModificadoresDeTeclas.Primario, 'L')]                             // falta Mayús
    [InlineData(ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Alternativa, 'L')]
    [InlineData(ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Alternativa, 'Q')]
    [InlineData(ModificadoresDeTeclas.Ninguno, 'Q')]                              // Q sin primario
    [InlineData(ModificadoresDeTeclas.Primario, 'B')]                             // futuro F-028
    [InlineData(ModificadoresDeTeclas.Primario, '8')]                             // dígito sin asignar
    public void Los_atajos_no_asignados_permanecen_inertes(
        ModificadoresDeTeclas modificadores, char tecla)
    {
        var sesion = new SesionDeAplicacion();
        var cierre = new CierreCapturado();
        var despachador = new DespachadorDeAtajos(
            new ServicioConmutacionIdioma(sesion), accionDeCierre: cierre);

        var resultado = despachador.Despachar(modificadores, tecla);

        // BR-F001-003 y ADR-001: sin efecto observable y sin respuesta.
        Assert.Equal(EstadoDespachoAtajo.Inerte, resultado.Estado);
        Assert.Null(resultado.Accion);
        Assert.Null(resultado.Conmutacion);
        Assert.Equal(0, cierre.Llamadas);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
    }

    [Fact]
    public void La_combinacion_se_normaliza_en_caja_y_modificadores()
    {
        var sesion = new SesionDeAplicacion();
        var despachador = new DespachadorDeAtajos(new ServicioConmutacionIdioma(sesion));

        // Tecla en minúscula y un bit de modificador fuera del modelo: la
        // combinación normalizada coincide con la asignada.
        var modificadoresConRuido =
            (ModificadoresDeTeclas)8 | ModificadoresDeTeclas.Primario | ModificadoresDeTeclas.Mayusculas;

        var resultado = despachador.Despachar(modificadoresConRuido, 'l');

        Assert.Equal(EstadoDespachoAtajo.Despachado, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
    }

    [Fact]
    public void La_alternancia_con_catalogo_no_disponible_aborta_sin_cambio_de_idioma()
    {
        var sesion = new SesionDeAplicacion();
        var servicioDeIdioma = new ServicioConmutacionIdioma(
            sesion, suministradorDeTextos: new SuministradorQueFalla());
        var despachador = new DespachadorDeAtajos(servicioDeIdioma);
        var notificaciones = new List<IdiomaConmutadoEventArgs>();
        servicioDeIdioma.IdiomaConmutado += (_, e) => notificaciones.Add(e);

        var resultado = despachador.Despachar(AlternarIdioma);

        // El despacho se produjo; el tratamiento del fallo es propio de
        // COM-001 (ERR-003): abortado sin cambio parcial ni notificación.
        Assert.Equal(EstadoDespachoAtajo.Despachado, resultado.Estado);
        Assert.Equal(AccionDeAtajo.AlternarIdioma, resultado.Accion);
        Assert.Equal(
            EstadoConmutacionIdioma.AbortadaPorCatalogoNoDisponible,
            resultado.Conmutacion!.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Empty(notificaciones);
    }

    [Fact]
    public void Despachar_no_acepta_un_servicio_de_idioma_ausente()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DespachadorDeAtajos(null!));
    }

    [Fact]
    public void La_combinacion_se_representa_en_el_formato_del_plan()
    {
        Assert.Equal("Ctrl/Cmd+Shift+L", AlternarIdioma.ToString());
        Assert.Equal("Ctrl/Cmd+Q", CerrarAplicacion.ToString());
    }

    /// <summary>Contraparte de cierre de prueba: cuenta sus invocaciones.</summary>
    private sealed class CierreCapturado : IAccionDeCierre
    {
        public int Llamadas { get; private set; }

        public void Ejecutar() => Llamadas++;
    }

    /// <summary>Suministrador de prueba: ningún catálogo de idioma carga (ERR-003).</summary>
    private sealed class SuministradorQueFalla : ISuministradorTextosI18n
    {
        private static readonly IReadOnlyDictionary<string, string> TextosVacios =
            new Dictionary<string, string>();

        public bool TryCargar(string codigoIdioma, out IReadOnlyDictionary<string, string> textos)
        {
            textos = TextosVacios;
            return false;
        }
    }
}
