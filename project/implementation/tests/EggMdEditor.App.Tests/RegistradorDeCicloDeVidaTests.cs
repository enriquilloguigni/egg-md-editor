using System.IO;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Errores y registros de arranque y de cierre (T-007): LOG-001 para el
// arranque (inicio del proceso y fin de la inicialización), LOG-003 para el
// cierre (inicio del cierre y fin del proceso), todo registro con marca de
// tiempo ISO 8601 e identificador de sesión compartidos, por la salida
// estándar y sin escritura a disco (C-001, NFR-003); ERR-001, ERR-002 y
// ERR-005 con el mensaje en español por la salida estándar de error.
public class RegistradorDeCicloDeVidaTests
{
    private static readonly DateTimeOffset HoraFija =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void El_inicio_del_proceso_registra_LOG001_en_Info_con_version_runtime_e_idioma()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var errorEstandar = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, errorEstandar, reloj: () => HoraFija,
            versionDeAplicacion: "1.2.3.0", versionDeRuntime: ".NET 10.0.0");

        registrador.RegistrarInicioDeProceso();

        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Info, registro.Nivel);
        Assert.Contains("LOG-001", registro.Mensaje);
        Assert.Contains(HoraFija.ToString("O"), registro.Mensaje);
        Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
        Assert.Contains("1.2.3.0", registro.Mensaje);
        Assert.Contains(".NET 10.0.0", registro.Mensaje);
        Assert.Contains("idioma activo es", registro.Mensaje);

        // Sin fallo no hay salida de error.
        Assert.Empty(errorEstandar.ToString());
    }

    [Fact]
    public void El_fin_de_inicializacion_registra_el_tiempo_de_arranque_y_el_idioma_activo()
    {
        var sesion = new SesionDeAplicacion();
        var reloj = new RelojDePrueba(HoraFija);
        var captura = new SalidaCapturada();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, new StringWriter(), reloj.Reloj,
            versionDeAplicacion: "1.0.0.0", versionDeRuntime: ".NET 10.0.0");

        reloj.AvanzarTiempo(TimeSpan.FromMilliseconds(250));
        sesion.IdiomaActivo = IdiomaDeInterfaz.English;
        registrador.RegistrarFinDeInicializacion();

        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Info, registro.Nivel);
        Assert.Contains("LOG-001", registro.Mensaje);
        Assert.Contains("ventana SC-001 creada", registro.Mensaje);
        Assert.Contains("catálogo i18n cargado", registro.Mensaje);
        Assert.Contains("idioma activo en", registro.Mensaje);
        Assert.Contains("tiempo de arranque 250 ms", registro.Mensaje);
    }

    [Fact]
    public void El_fallo_de_arranque_sale_en_espanol_por_el_error_estandar_y_registra_LOG001_Error_ERR001_ERR002()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var errorEstandar = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, errorEstandar, reloj: () => HoraFija,
            versionDeAplicacion: "1.0.0.0", versionDeRuntime: ".NET 10.0.0");
        var excepcion = new InvalidOperationException("el catálogo i18n inicial no carga");

        var codigo = registrador.RegistrarFalloDeArranque(excepcion);

        // El proceso finaliza con código de error (ERR-001, ERR-002).
        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, codigo);
        Assert.Equal(1, codigo);

        // Mensaje en español, exactamente el de la Spec, por la salida
        // estándar de error: «Error al iniciar la aplicación: <tipo y
        // mensaje de la excepción>».
        var lineas = errorEstandar.ToString().Split('\n', StringSplitOptions.TrimEntries);
        var linea = Assert.Single(lineas, l => l.Length > 0);
        Assert.Equal(
            "Error al iniciar la aplicación: " +
            "System.InvalidOperationException: el catálogo i18n inicial no carga",
            linea);

        // Log requerido: LOG-001 en Error, con tipo y mensaje de la excepción.
        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Error, registro.Nivel);
        Assert.Contains("LOG-001", registro.Mensaje);
        Assert.Contains("System.InvalidOperationException: el catálogo i18n inicial no carga", registro.Mensaje);
        Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
        Assert.Contains(HoraFija.ToString("O"), registro.Mensaje);
    }

    [Fact]
    public void El_fallo_de_pintado_durante_el_arranque_usa_el_mismo_mensaje_y_registro_que_ERR001()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var errorEstandar = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, errorEstandar, reloj: () => HoraFija);

        var codigo = registrador.RegistrarFalloDeArranque(
            new ExcepcionDePrueba("fallo al pintar SC-001 durante el arranque"));

        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, codigo);
        Assert.Contains(
            "Error al iniciar la aplicación: " +
            "EggMdEditor.App.Tests.RegistradorDeCicloDeVidaTests.ExcepcionDePrueba: " +
            "fallo al pintar SC-001 durante el arranque",
            errorEstandar.ToString());
        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Error, registro.Nivel);
        Assert.Contains("(ERR-001, ERR-002)", registro.Mensaje);
    }

    [Fact]
    public void El_cierre_registra_inicio_y_fin_con_la_duracion_total_de_la_sesion_LOG003()
    {
        var sesion = new SesionDeAplicacion();
        var reloj = new RelojDePrueba(HoraFija);
        var captura = new SalidaCapturada();
        var errorEstandar = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, errorEstandar, reloj.Reloj);

        reloj.AvanzarTiempo(TimeSpan.FromMilliseconds(1250));
        registrador.RegistrarInicioDeCierre();
        registrador.RegistrarFinDeProceso();

        Assert.Equal(2, captura.Registros.Count);
        Assert.All(captura.Registros, r => Assert.Equal(NivelRegistro.Info, r.Nivel));
        Assert.Contains("LOG-003", captura.Registros[0].Mensaje);
        Assert.Contains("inicio del cierre de SC-001", captura.Registros[0].Mensaje);
        Assert.Contains("(COM-004)", captura.Registros[0].Mensaje);
        Assert.Contains("fin del proceso", captura.Registros[1].Mensaje);
        Assert.Contains("cierre limpio sin procesos residuales", captura.Registros[1].Mensaje);
        Assert.Contains("duración total de la sesión 1250 ms", captura.Registros[1].Mensaje);

        // Sin fallo no hay salida de error.
        Assert.Empty(errorEstandar.ToString());
    }

    [Fact]
    public void El_fallo_de_cierre_sale_en_espanol_por_el_error_estandar_y_registra_LOG003_Error_ERR005()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var errorEstandar = new StringWriter();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, errorEstandar, reloj: () => HoraFija);
        var excepcion = new InvalidOperationException("fallo al liberar los recursos de la sesión");

        var codigo = registrador.RegistrarFalloDeCierre(excepcion);

        // El proceso se termina igualmente, con código de error (ERR-005).
        Assert.Equal(RegistradorDeCicloDeVida.CodigoSalidaPorFallo, codigo);

        // Mensaje en español, exactamente el de la Spec, por la salida
        // estándar de error.
        var lineas = errorEstandar.ToString().Split('\n', StringSplitOptions.TrimEntries);
        var linea = Assert.Single(lineas, l => l.Length > 0);
        Assert.Equal(
            "Error al cerrar la aplicación: " +
            "System.InvalidOperationException: fallo al liberar los recursos de la sesión",
            linea);

        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Error, registro.Nivel);
        Assert.Contains("LOG-003", registro.Mensaje);
        Assert.Contains("System.InvalidOperationException: fallo al liberar los recursos de la sesión", registro.Mensaje);
        Assert.Contains("(ERR-005)", registro.Mensaje);
        Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
    }

    [Fact]
    public void Todo_el_ciclo_comparte_identificador_de_sesion_y_marca_de_tiempo_ISO8601()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var registrador = new RegistradorDeCicloDeVida(
            sesion, captura, new StringWriter(), reloj: () => HoraFija);

        registrador.RegistrarInicioDeProceso();
        registrador.RegistrarFinDeInicializacion();
        registrador.RegistrarInicioDeCierre();
        registrador.RegistrarFinDeProceso();

        Assert.Equal(4, captura.Registros.Count);
        Assert.All(captura.Registros, registro =>
        {
            // Correlación: sesión única y hora ISO 8601 en cada registro.
            Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
            Assert.Contains(HoraFija.ToString("O"), registro.Mensaje);
        });
    }

    [Fact]
    public void Por_defecto_expone_la_version_del_ensamblado_y_del_runtime()
    {
        var registrador = new RegistradorDeCicloDeVida(new SesionDeAplicacion());

        Assert.False(string.IsNullOrWhiteSpace(registrador.VersionDeAplicacion));
        Assert.False(string.IsNullOrWhiteSpace(registrador.VersionDeRuntime));
    }

    [Fact]
    public void La_sesion_nula_es_un_error_de_programacion()
    {
        Assert.Throws<ArgumentNullException>(
            () => new RegistradorDeCicloDeVida(null!));
    }

    [Fact]
    public void Una_excepcion_nula_es_un_error_de_programacion()
    {
        var registrador = new RegistradorDeCicloDeVida(
            new SesionDeAplicacion(), new SalidaCapturada(), new StringWriter());

        Assert.Throws<ArgumentNullException>(() => registrador.RegistrarFalloDeArranque(null!));
        Assert.Throws<ArgumentNullException>(() => registrador.RegistrarFalloDeCierre(null!));
    }

    /// <summary>Salida de registros que captura lo emitido para su verificación.</summary>
    private sealed class SalidaCapturada : ISalidaDeRegistros
    {
        public List<(NivelRegistro Nivel, string Mensaje)> Registros { get; } = [];

        public void Escribir(NivelRegistro nivel, string mensaje) =>
            Registros.Add((nivel, mensaje));
    }

    /// <summary>Reloj fijo con avance controlado, para tiempos deterministas.</summary>
    private sealed class RelojDePrueba
    {
        private DateTimeOffset _ahora;

        public RelojDePrueba(DateTimeOffset inicial) => _ahora = inicial;

        public Func<DateTimeOffset> Reloj => () => _ahora;

        public void AvanzarTiempo(TimeSpan intervalo) => _ahora = _ahora.Add(intervalo);
    }

    /// <summary>Excepción propia, para verificar el tipo y el mensaje en la salida.</summary>
    private sealed class ExcepcionDePrueba : Exception
    {
        public ExcepcionDePrueba(string mensaje)
            : base(mensaje)
        {
        }
    }
}
