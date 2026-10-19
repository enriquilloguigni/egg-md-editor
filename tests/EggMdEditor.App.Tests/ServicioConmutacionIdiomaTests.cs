using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Servicio de conmutación de idioma (COM-001): relabelizado en caliente sin
// reinicio conservando ventana y estado (BR-F001-005, DS-003), abortado sin
// cambio parcial si el catálogo del idioma destino no carga (ERR-003,
// AC-010) y registro técnico LOG-002 con nivel Debug en éxito completo,
// Warning si quedan claves sin resolver (ERR-004, AC-009) y Error si el
// catálogo no carga.
public class ServicioConmutacionIdiomaTests
{
    private static readonly DateTimeOffset HoraFija =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void El_idioma_activo_del_servicio_es_el_de_la_sesion_y_arranca_en_espanol()
    {
        var sesion = new SesionDeAplicacion();
        var servicio = new ServicioConmutacionIdioma(sesion);

        Assert.Equal(IdiomaDeInterfaz.Español, servicio.IdiomaActivo);
        Assert.Equal(sesion.IdiomaActivo, servicio.IdiomaActivo);
    }

    [Fact]
    public void Conmutar_es_a_en_actualiza_la_sesion_notifica_una_vez_y_conserva_el_estado()
    {
        var sesion = new SesionDeAplicacion
        {
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(
            sesion, salidaDeRegistros: captura, reloj: () => HoraFija);
        var notificaciones = new List<IdiomaConmutadoEventArgs>();
        IdiomaDeInterfaz idiomaVistoEnLaNotificacion = IdiomaDeInterfaz.Español;
        servicio.IdiomaConmutado += (_, e) =>
        {
            notificaciones.Add(e);
            idiomaVistoEnLaNotificacion = servicio.IdiomaActivo;
        };

        var resultado = servicio.Conmutar(IdiomaDeInterfaz.English);

        Assert.Equal(EstadoConmutacionIdioma.Conmutada, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, resultado.IdiomaOrigen);
        Assert.Equal(IdiomaDeInterfaz.English, resultado.IdiomaDestino);
        Assert.Empty(resultado.ClavesSinResolver);
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal(IdiomaDeInterfaz.English, servicio.IdiomaActivo);

        // DS-003: única notificación, emitida con el estado ya aplicado.
        Assert.Single(notificaciones);
        Assert.Equal(IdiomaDeInterfaz.Español, notificaciones[0].IdiomaAnterior);
        Assert.Equal(IdiomaDeInterfaz.English, notificaciones[0].IdiomaNuevo);
        Assert.Equal(IdiomaDeInterfaz.English, idiomaVistoEnLaNotificacion);

        // BR-F001-005: solo cambia el idioma; el estado de la sesión queda intacto.
        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);

        // LOG-002 en éxito completo: un único registro Debug con hora,
        // sesión e idiomas origen → destino.
        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Debug, registro.Nivel);
        Assert.Contains(HoraFija.ToString("O"), registro.Mensaje);
        Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
        Assert.Contains("es → en", registro.Mensaje);
    }

    [Fact]
    public void Conmutar_de_en_de_vuelta_a_es_actualiza_y_notifica_en_sentido_inverso()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, salidaDeRegistros: captura);
        var notificaciones = new List<IdiomaConmutadoEventArgs>();
        servicio.IdiomaConmutado += (_, e) => notificaciones.Add(e);

        Assert.Equal(
            EstadoConmutacionIdioma.Conmutada,
            servicio.Conmutar(IdiomaDeInterfaz.English).Estado);
        Assert.Equal(
            EstadoConmutacionIdioma.Conmutada,
            servicio.Conmutar(IdiomaDeInterfaz.Español).Estado);

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal(2, notificaciones.Count);
        Assert.Equal((IdiomaDeInterfaz.Español, IdiomaDeInterfaz.English),
            (notificaciones[0].IdiomaAnterior, notificaciones[0].IdiomaNuevo));
        Assert.Equal((IdiomaDeInterfaz.English, IdiomaDeInterfaz.Español),
            (notificaciones[1].IdiomaAnterior, notificaciones[1].IdiomaNuevo));
        Assert.Contains("en → es", captura.Registros[1].Mensaje);
    }

    [Fact]
    public void Conmutar_al_idioma_ya_activo_es_una_operacion_nula()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, salidaDeRegistros: captura);
        var notificaciones = 0;
        servicio.IdiomaConmutado += (_, _) => notificaciones++;

        var resultado = servicio.Conmutar(IdiomaDeInterfaz.Español);

        Assert.Equal(EstadoConmutacionIdioma.YaActiva, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal(0, notificaciones);
        Assert.Empty(captura.Registros);
    }

    [Fact]
    public void Conmutar_se_aborta_sin_cambio_parcial_y_permite_reintentar_ERR003()
    {
        var sesion = new SesionDeAplicacion
        {
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var suministrador = new SuministradorDePrueba();
        suministrador.SimularFallo("en");
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, suministrador, captura);
        var notificaciones = 0;
        servicio.IdiomaConmutado += (_, _) => notificaciones++;

        var resultado = servicio.Conmutar(IdiomaDeInterfaz.English);

        // ERR-003: abortada sin cambio parcial, con la ventana y el estado intactos.
        Assert.Equal(EstadoConmutacionIdioma.AbortadaPorCatalogoNoDisponible, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal(IdiomaDeInterfaz.Español, servicio.IdiomaActivo);
        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);
        Assert.Equal(0, notificaciones);

        // LOG-002: el fallo solo se registra, con nivel Error, sin mensaje visible.
        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Error, registro.Nivel);
        Assert.Contains(sesion.IdentificadorDeSesion, registro.Mensaje);
        Assert.Contains("es → en", registro.Mensaje);

        // Reintento con el catálogo ya disponible: la conmutación procede.
        suministrador.Permitir("en");
        var reintento = servicio.Conmutar(IdiomaDeInterfaz.English);

        Assert.Equal(EstadoConmutacionIdioma.Conmutada, reintento.Estado);
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal(1, notificaciones);
        Assert.Equal(2, captura.Registros.Count);
        Assert.Equal(NivelRegistro.Debug, captura.Registros[1].Nivel);
    }

    [Fact]
    public void Claves_sin_resolver_en_el_destino_se_listan_y_registran_como_Warning_ERR004()
    {
        var sesion = new SesionDeAplicacion();
        var suministrador = new SuministradorDePrueba();
        suministrador.Registrar(
            "en",
            CatalogoCadenasI18n.TextosPorIdioma["en"]
                .Where(par => par.Key is not ("menu.archivo" or "barra.cita"))
                .ToDictionary(par => par.Key, par => par.Value));
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, suministrador, captura);
        var notificaciones = 0;
        servicio.IdiomaConmutado += (_, _) => notificaciones++;

        var resultado = servicio.Conmutar(IdiomaDeInterfaz.English);

        // La conmutación no se aborta: el catálogo carga, con claves sin
        // resolver que caen en el idioma defecto (ERR-004).
        Assert.Equal(EstadoConmutacionIdioma.Conmutada, resultado.Estado);
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal(1, notificaciones);
        Assert.Equal((string[])["barra.cita", "menu.archivo"], resultado.ClavesSinResolver);

        var registro = Assert.Single(captura.Registros);
        Assert.Equal(NivelRegistro.Warning, registro.Nivel);
        Assert.Contains("es → en", registro.Mensaje);
        Assert.Contains("menu.archivo", registro.Mensaje);
        Assert.Contains("barra.cita", registro.Mensaje);
    }

    [Fact]
    public void Con_el_catalogo_estatico_toda_conmutacion_es_en_es_completa_y_en_Debug()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, salidaDeRegistros: captura);

        var ida = servicio.Conmutar(IdiomaDeInterfaz.English);
        var vuelta = servicio.Conmutar(IdiomaDeInterfaz.Español);

        Assert.Empty(ida.ClavesSinResolver);
        Assert.Empty(vuelta.ClavesSinResolver);
        Assert.Equal(2, captura.Registros.Count);
        Assert.All(captura.Registros, registro => Assert.Equal(NivelRegistro.Debug, registro.Nivel));
    }

    [Fact]
    public void Conmutar_a_un_valor_fuera_del_catalogo_es_un_error_de_programacion()
    {
        var sesion = new SesionDeAplicacion();
        var captura = new SalidaCapturada();
        var servicio = new ServicioConmutacionIdioma(sesion, salidaDeRegistros: captura);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => servicio.Conmutar((IdiomaDeInterfaz)99));

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Empty(captura.Registros);
    }

    [Fact]
    public void Cada_sesion_tiene_identificador_propio_para_la_correlacion_de_registros()
    {
        Assert.NotEqual(
            new SesionDeAplicacion().IdentificadorDeSesion,
            new SesionDeAplicacion().IdentificadorDeSesion);
    }

    [Fact]
    public void El_suministrador_por_defecto_carga_es_y_en_y_rechaza_codigos_no_registrados()
    {
        var suministrador = SuministradorCatalogoCadenasI18n.Instancia;

        Assert.True(suministrador.TryCargar("es", out var espanol));
        Assert.Equal(CatalogoCadenasI18n.TotalCadenas, espanol.Count);
        Assert.True(suministrador.TryCargar(" EN ", out var ingles));
        Assert.Equal(CatalogoCadenasI18n.TotalCadenas, ingles.Count);

        // Caja y espacios no cuentan; vacío o nulo cae en el idioma defecto.
        Assert.True(suministrador.TryCargar("", out var vacio));
        Assert.Equal(CatalogoCadenasI18n.TotalCadenas, vacio.Count);
        Assert.True(suministrador.TryCargar(null!, out var nulo));
        Assert.Equal(CatalogoCadenasI18n.TotalCadenas, nulo.Count);

        // Un código no registrado no carga: abortado de la conmutación (ERR-003).
        Assert.False(suministrador.TryCargar("fr", out var ausente));
        Assert.Empty(ausente);
    }

    /// <summary>Salida de registros que captura lo emitido para su verificación.</summary>
    private sealed class SalidaCapturada : ISalidaDeRegistros
    {
        public List<(NivelRegistro Nivel, string Mensaje)> Registros { get; } = [];

        public void Escribir(NivelRegistro nivel, string mensaje) =>
            Registros.Add((nivel, mensaje));
    }

    /// <summary>
    /// Suministrador de textos de prueba: catálogo es/en completo por defecto,
    /// con fallos de carga y catálogos parciales simulables.
    /// </summary>
    private sealed class SuministradorDePrueba : ISuministradorTextosI18n
    {
        private static readonly IReadOnlyDictionary<string, string> TextosVacios =
            new Dictionary<string, string>();

        private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _textosPorCodigo;
        private readonly HashSet<string> _codigosQueFallan = [];

        public SuministradorDePrueba()
        {
            _textosPorCodigo = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["es"] = CatalogoCadenasI18n.TextosPorIdioma[CatalogoCadenasI18n.CodigoDefecto],
                ["en"] = CatalogoCadenasI18n.TextosPorIdioma["en"],
            };
        }

        /// <summary>Fuerza que el código de idioma dado no cargue (ERR-003).</summary>
        public void SimularFallo(string codigo) => _codigosQueFallan.Add(codigo);

        /// <summary>Deja de forzar el fallo del código de idioma dado.</summary>
        public void Permitir(string codigo) => _codigosQueFallan.Remove(codigo);

        /// <summary>Sustituye los textos del código de idioma dado.</summary>
        public void Registrar(string codigo, IReadOnlyDictionary<string, string> textos) =>
            _textosPorCodigo[codigo] = textos;

        public bool TryCargar(string codigoIdioma, out IReadOnlyDictionary<string, string> textos)
        {
            var codigo = codigoIdioma?.Trim().ToLowerInvariant() ?? CatalogoCadenasI18n.CodigoDefecto;

            if (_codigosQueFallan.Contains(codigo)
                || !_textosPorCodigo.TryGetValue(codigo, out var encontrados))
            {
                textos = TextosVacios;
                return false;
            }

            textos = encontrados;
            return true;
        }
    }
}
