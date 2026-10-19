using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Resultado de una operación de conmutación de idioma (COM-001).
/// </summary>
public enum EstadoConmutacionIdioma
{
    /// <summary>
    /// Conmutación completada: el idioma activo de la sesión se actualizó y
    /// los suscriptores del relabelizado fueron notificados una única vez.
    /// </summary>
    Conmutada = 1,

    /// <summary>
    /// El idioma destino ya es el activo: operación nula, sin cambio, sin
    /// notificación y sin registro (siempre hay exactamente un idioma activo,
    /// BR-F001-004).
    /// </summary>
    YaActiva = 2,

    /// <summary>
    /// Conmutación abortada sin cambio parcial (ERR-003): el catálogo i18n
    /// del idioma destino no carga; permanece el idioma activo anterior, con
    /// la ventana y el estado intactos. El fallo solo se registra (LOG-002).
    /// </summary>
    AbortadaPorCatalogoNoDisponible = 3,
}

/// <summary>
/// Resultado detallado de una conmutación de idioma (COM-001), con los
/// idiomas origen y destino y las claves i18n sin resolver en el idioma
/// destino, que se resuelven en español por ser el idioma defecto (ERR-004).
/// </summary>
/// <param name="Estado">Estado final de la operación.</param>
/// <param name="IdiomaOrigen">Idioma activo antes de la operación.</param>
/// <param name="IdiomaDestino">Idioma solicitado.</param>
/// <param name="ClavesSinResolver">
/// Claves publicadas ausentes del catálogo del idioma destino, en orden
/// ordinal; vacío si el relabelizado queda completo.
/// </param>
public sealed record ResultadoConmutacionIdioma(
    EstadoConmutacionIdioma Estado,
    IdiomaDeInterfaz IdiomaOrigen,
    IdiomaDeInterfaz IdiomaDestino,
    IReadOnlyList<string> ClavesSinResolver);

/// <summary>
/// Notificación de relabelizado en caliente (DS-003): se emite una única vez
/// por conmutación, después de aplicar el nuevo idioma activo a la sesión, de
/// modo que los suscriptores resuelven toda cadena desde el estado ya
/// aplicado. Los valores son mutuamente excluyentes (BR-007).
/// </summary>
public sealed class IdiomaConmutadoEventArgs : EventArgs
{
    /// <summary>Idioma activo antes de la conmutación.</summary>
    public IdiomaDeInterfaz IdiomaAnterior { get; }

    /// <summary>Idioma activo tras la conmutación.</summary>
    public IdiomaDeInterfaz IdiomaNuevo { get; }

    /// <summary>Crea la notificación con el par de idiomas de la conmutación.</summary>
    public IdiomaConmutadoEventArgs(IdiomaDeInterfaz idiomaAnterior, IdiomaDeInterfaz idiomaNuevo)
    {
        IdiomaAnterior = idiomaAnterior;
        IdiomaNuevo = idiomaNuevo;
    }
}

/// <summary>
/// Servicio de conmutación del idioma de la interfaz (COM-001): canal síncrono
/// y en proceso que recibe el idioma elegido {es, en} desde la entrada
/// «Idioma de la interfaz» y hace efectivo el relabelizado en caliente sin
/// reinicio (BR-F001-005, NFR-006). Toda cadena visible se resuelve desde una
/// fuente observable notificada al conmutar (DS-003): los suscriptores de
/// <see cref="IdiomaConmutado"/> re-resuelven sus rótulos y avisos contra el
/// idioma activo ya aplicado a la sesión. Si el catálogo i18n del idioma
/// destino no carga, la conmutación se aborta sin cambio parcial: permanece
/// el idioma activo anterior, no se notifica nada y el fallo solo se registra
/// (ERR-003); el usuario puede reintentar. Cada operación termina registrando
/// LOG-002: <c>Debug</c> si es exitosa y completa, <c>Warning</c> si quedan
/// claves i18n sin resolver (ERR-004) y <c>Error</c> si el catálogo no carga.
/// La única mutación de la sesión es el idioma activo: el estado del menú y
/// el aviso flotante quedan intactos (BR-F001-005) y la elección no se
/// persiste (FR-013 → F-030). Síncrono y en proceso (C-001): todas las
/// operaciones se ejecutan en el hilo de la interfaz, sin bloqueos.
/// </summary>
public sealed class ServicioConmutacionIdioma
{
    private readonly SesionDeAplicacion _sesion;
    private readonly ISuministradorTextosI18n _suministrador;
    private readonly ISalidaDeRegistros _salidaDeRegistros;
    private readonly Func<DateTimeOffset> _reloj;

    /// <summary>
    /// Notificación única de relabelizado en caliente (DS-003), emitida tras
    /// aplicar el idioma nuevo a la sesión. Un suscriptor que lanza una
    /// excepción es un error de programación: no se captura.
    /// </summary>
    public event EventHandler<IdiomaConmutadoEventArgs>? IdiomaConmutado;

    /// <summary>
    /// Crea el servicio sobre la sesión de la aplicación. El suministrador de
    /// textos es por defecto el catálogo de cadenas i18n en memoria (T-003) y
    /// la salida de registros, la nula; el reloj permite fijar la marca de
    /// tiempo ISO 8601 de los registros LOG-002.
    /// </summary>
    public ServicioConmutacionIdioma(
        SesionDeAplicacion sesion,
        ISuministradorTextosI18n? suministradorDeTextos = null,
        ISalidaDeRegistros? salidaDeRegistros = null,
        Func<DateTimeOffset>? reloj = null)
    {
        _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
        _suministrador = suministradorDeTextos ?? SuministradorCatalogoCadenasI18n.Instancia;
        _salidaDeRegistros = salidaDeRegistros ?? SalidaDeRegistrosNula.Instancia;
        _reloj = reloj ?? RelojPorDefecto;
    }

    /// <summary>Idioma activo de la sesión: la fuente observable del relabelizado (BR-F001-004).</summary>
    public IdiomaDeInterfaz IdiomaActivo => _sesion.IdiomaActivo;

    private static DateTimeOffset RelojPorDefecto() => DateTimeOffset.UtcNow;

    /// <summary>
    /// Conmuta el idioma activo de la sesión al destino dado (COM-001). Carga
    /// primero el catálogo del idioma destino: si no carga, la operación se
    /// aborta sin cambio parcial y sin notificación (ERR-003); si carga,
    /// actualiza el idioma activo —única mutación de la sesión, con el estado
    /// intacto (BR-F001-005)— y notifica exactamente una vez a los
    /// suscriptores del relabelizado (DS-003), registrando LOG-002 al final.
    /// Conmutar al idioma ya activo es una operación nula. El destino debe
    /// pertenecer al catálogo de idiomas (BR-007); cualquier otro valor es un
    /// error de programación.
    /// </summary>
    public ResultadoConmutacionIdioma Conmutar(IdiomaDeInterfaz idiomaDestino)
    {
        if (!CatalogoIdiomas.Valores.Contains(idiomaDestino))
        {
            throw new ArgumentOutOfRangeException(
                nameof(idiomaDestino), idiomaDestino,
                "El idioma destino no pertenece al catálogo de idiomas de la interfaz (BR-007).");
        }

        var idiomaOrigen = _sesion.IdiomaActivo;
        var codigoOrigen = CatalogoCadenasI18n.CodigoDe(idiomaOrigen);
        var codigoDestino = CatalogoCadenasI18n.CodigoDe(idiomaDestino);

        if (idiomaOrigen == idiomaDestino)
        {
            return new ResultadoConmutacionIdioma(
                EstadoConmutacionIdioma.YaActiva, idiomaOrigen, idiomaDestino, []);
        }

        // ERR-003: el catálogo del idioma destino se carga antes de tocar el
        // estado, de modo que el abortado no deja ningún cambio parcial.
        if (!_suministrador.TryCargar(codigoDestino, out var textosDestino))
        {
            Registrar(
                NivelRegistro.Error,
                $"LOG-002: conmutación de idioma {codigoOrigen} → {codigoDestino} abortada: " +
                "el catálogo i18n del idioma destino no carga; se conserva el idioma activo " +
                $"{codigoOrigen} con la ventana y el estado intactos (ERR-003).");

            return new ResultadoConmutacionIdioma(
                EstadoConmutacionIdioma.AbortadaPorCatalogoNoDisponible,
                idiomaOrigen, idiomaDestino, []);
        }

        // ERR-004: claves publicadas ausentes del catálogo destino; se
        // resolverán en español (idioma defecto) durante el relabelizado.
        var clavesSinResolver = ClavesSinResolverEn(textosDestino);

        // Única mutación de la sesión: el idioma activo (BR-F001-005).
        _sesion.IdiomaActivo = idiomaDestino;

        // DS-003: única notificación por conmutación, tras aplicar el estado.
        IdiomaConmutado?.Invoke(this, new IdiomaConmutadoEventArgs(idiomaOrigen, idiomaDestino));

        if (clavesSinResolver.Count > 0)
        {
            Registrar(
                NivelRegistro.Warning,
                $"LOG-002: conmutación de idioma {codigoOrigen} → {codigoDestino} completada " +
                "tras el relabelizado; claves i18n sin resolver, resueltas en español " +
                $"(ERR-004): {string.Join(", ", clavesSinResolver)}.");
        }
        else
        {
            Registrar(
                NivelRegistro.Debug,
                $"LOG-002: conmutación de idioma {codigoOrigen} → {codigoDestino} completada " +
                "tras el relabelizado, sin claves i18n sin resolver.");
        }

        return new ResultadoConmutacionIdioma(
            EstadoConmutacionIdioma.Conmutada, idiomaOrigen, idiomaDestino, clavesSinResolver);
    }

    /// <summary>
    /// Claves publicadas por los catálogos (la fuente única es el conjunto de
    /// claves del idioma defecto, BR-F001-007) ausentes del catálogo destino,
    /// en orden ordinal para un registro determinista.
    /// </summary>
    private List<string> ClavesSinResolverEn(IReadOnlyDictionary<string, string> textosDestino) =>
        CatalogoCadenasI18n.TextosPorIdioma[CatalogoCadenasI18n.CodigoDefecto].Keys
            .Where(clave => !textosDestino.ContainsKey(clave))
            .OrderBy(clave => clave, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Registra LOG-002 por la salida de registros: marca de tiempo ISO 8601
    /// e identificador de sesión compartidos por todo registro (logs.md,
    /// NFR-003), sin escritura a disco ni salida del equipo (C-001).
    /// </summary>
    private void Registrar(NivelRegistro nivel, string mensaje) =>
        _salidaDeRegistros.Escribir(
            nivel,
            $"{_reloj().ToString("O")} [sesión {_sesion.IdentificadorDeSesion}] {mensaje}");
}
