using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    private readonly SesionDeAplicacion _sesion;
    private readonly GestorDeCierreDeSesion _gestorDeCierre;

    /// <summary>
    /// Constructor sin argumentos para el diseñador de XAML y para el
    /// arranque: crea la sesión en su defecto de arranque (idioma español,
    /// BR-007) con los servicios por defecto.
    /// </summary>
    public MainWindow() : this(new SesionDeAplicacion())
    {
    }

    /// <summary>
    /// Crea la ventana única de la aplicación (BR-003) con su barra de menú
    /// persistente al 100 % de FR-011 (T-008, T-010), su barra de formato al
    /// 100 % de FR-012 (T-009) y su área de documento vacía (AC-015), y
    /// cablea en ella los canales internos COM-001–COM-004 (T-011):
    ///
    /// <list type="bullet">
    /// <item>COM-001 — relabelizado en caliente (DS-003): suscripción única
    /// a la notificación del servicio de conmutación de idioma (T-004) que
    /// re-resuelve toda cadena visible desde el idioma activo ya aplicado a
    /// la sesión; la entrada de idioma del menú (T-010) conmuta al pulsar
    /// cada valor y restaura su marca de radio si la conmutación no llega a
    /// efectiva (ya activa o abortada, ERR-003).</item>
    /// <item>COM-002 — atajos (T-005): el teclado de la ventana traduce cada
    /// gesto al modelo de combinaciones y lo despacha; el registro único del
    /// despachador asigna <c>Ctrl/Cmd+Shift+L</c> a alternar es/en y
    /// <c>Ctrl/Cmd+Q</c> al cierre, y cualquier otro atajo permanece inerte
    /// (BR-F001-003, ADR-001).</item>
    /// <item>COM-003 — estado visual (T-009, T-010): el gestor espeja sobre
    /// la sesión el estado del menú {Cerrado | Desplegado con entrada activa}
    /// y la visibilidad del aviso flotante de la barra de formato.</item>
    /// <item>COM-004 — cierre: el atajo de cierre y la ventana cierran por el
    /// mismo camino; al cerrarse, se libera la sesión con su registro
    /// LOG-003 (T-007) y cesan los canales internos; ante fallo de cierre,
    /// ERR-005 con código de salida de error.</item>
    /// </list>
    ///
    /// Los servicios son inyectables para un cableado determinista en pruebas;
    /// por defecto se crean sobre la sesión dada, con la salida de registros
    /// formateada a la salida estándar del proceso (T-007, C-001).
    /// </summary>
    public MainWindow(
        SesionDeAplicacion sesion,
        ServicioConmutacionIdioma? servicioDeIdioma = null,
        DespachadorDeAtajos? despachadorDeAtajos = null,
        DisparadorDeSegundaVentana? disparadorDeSegundaVentana = null,
        RegistradorDeCicloDeVida? registradorDeCicloDeVida = null)
    {
        ArgumentNullException.ThrowIfNull(sesion);

        _sesion = sesion;

        InitializeComponent();

        ConstructorDeMenuPrincipal.Construir(BarraMenu, sesion);
        ConstructorDeBarraFormato.Construir(BarraFormato, sesion);

        // COM-001 (T-004): servicio de conmutación con la salida de registros
        // del proceso (LOG-002) y notificación única de relabelizado (DS-003).
        ServicioDeIdioma = servicioDeIdioma ?? new ServicioConmutacionIdioma(
            sesion,
            SuministradorCatalogoCadenasI18n.Instancia,
            new SalidaDeRegistrosConsola());
        ServicioDeIdioma.IdiomaConmutado += AlConmutarseElIdioma;
        CablearEntradaDeIdioma();

        // COM-003: espejo de estado visual de menú y avisos (VR-002).
        new GestorDeEstadoVisual(sesion).Conectar(BarraMenu, BarraFormato);

        // COM-004 (T-007): cierre de la ventana con liberación de la sesión
        // y registros LOG-003; el atajo de cierre del despachador (COM-002)
        // cierra por este mismo camino.
        _gestorDeCierre = new GestorDeCierreDeSesion(sesion, registradorDeCicloDeVida);
        DespachadorDeAtajos = despachadorDeAtajos
            ?? new DespachadorDeAtajos(ServicioDeIdioma, new AccionDeCierreDeVentana(this));
        AddHandler(InputElement.KeyDownEvent, AlPulsarUnaTecla, RoutingStrategies.Tunnel);
        Closed += AlCerrarLaVentana;

        // T-006: contraparte de enfoque de la ventana única, cableada sobre
        // esta ventana; el disparador atiende la solicitud sin crear otra.
        DisparadorDeSegundaVentana =
            disparadorDeSegundaVentana ?? new DisparadorDeSegundaVentana(new EnfocadorDeVentana(this));
    }

    /// <summary>Servicio de conmutación de idioma (COM-001) cableado a esta ventana.</summary>
    public ServicioConmutacionIdioma ServicioDeIdioma { get; }

    /// <summary>Despachador de atajos (COM-002) cableado a esta ventana.</summary>
    public DespachadorDeAtajos DespachadorDeAtajos { get; }

    /// <summary>
    /// Disparador de la solicitud de segunda ventana (AC-014, T-006) con la
    /// contraparte de enfoque de esta ventana: el punto de entrada del
    /// proceso le pasa la solicitud y la ventana única existente recibe el
    /// foco, sin crearse otra (BR-003).
    /// </summary>
    public DisparadorDeSegundaVentana DisparadorDeSegundaVentana { get; }

    /// <summary>
    /// COM-001 (DS-003): relabelizado en caliente. La notificación llega tras
    /// aplicar el idioma nuevo a la sesión —la fuente observable—, de modo
    /// que toda cadena visible se re-resuelve desde el estado ya aplicado,
    /// sin reinicio y conservando el resto del estado (BR-F001-005).
    /// </summary>
    private void AlConmutarseElIdioma(object? sender, IdiomaConmutadoEventArgs e) =>
        RelabelizadorDeInterfaz.Relabelizar(BarraMenu, BarraFormato, _sesion.IdiomaActivo);

    /// <summary>
    /// COM-001 desde el menú (T-010): pulsar un valor de la selección de
    /// idioma conmuta al idioma elegido. Si la conmutación no llega a
    /// efectiva —idioma ya activo u operación abortada por catálogo no
    /// disponible (ERR-003)—, el idioma activo de la sesión no cambió y la
    /// marca de radio se restaura desde el estado; si es efectiva, el
    /// relabelizado llega con la notificación única (DS-003) y la marca con
    /// él (el relabelizado re-marca la selección desde el estado).
    /// </summary>
    private void ConmutarDesdeMenu(IdiomaDeInterfaz idiomaDestino)
    {
        var resultado = ServicioDeIdioma.Conmutar(idiomaDestino);

        if (resultado.Estado != EstadoConmutacionIdioma.Conmutada)
        {
            RelabelizadorDeInterfaz.Relabelizar(BarraMenu, BarraFormato, _sesion.IdiomaActivo);
        }
    }

    /// <summary>
    /// Conecta los valores de la selección de idioma de la entrada
    /// Herramientas → Idioma de la interfaz (T-010) al canal COM-001: cada
    /// valor, identificado por su clave de catálogo, conmuta a su idioma.
    /// </summary>
    private void CablearEntradaDeIdioma()
    {
        foreach (var item in EnumerarElementosDeMenu(BarraMenu))
        {
            if (item.Tag is not string clave)
            {
                continue;
            }

            var idiomaDelValor = CatalogoIdiomas.Valores.FirstOrDefault(
                idioma => ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma(idioma) == clave);

            if (idiomaDelValor is IdiomaDeInterfaz idioma)
            {
                item.Click += (_, _) => ConmutarDesdeMenu(idioma);
            }
        }
    }

    /// <summary>COM-002: traduce el gesto al modelo y lo despacha (T-005). Los gestos no expresables —sin letra ni dígito— y las combinaciones no asignadas quedan inertes (BR-F001-003, ADR-001).</summary>
    private void AlPulsarUnaTecla(object? sender, KeyEventArgs e)
    {
        var combinacion = TraductorDeGestos.Traducir(e.Key, e.KeyModifiers);

        if (combinacion is null)
        {
            return;
        }

        var resultado = DespachadorDeAtajos.Despachar(combinacion.Value);

        if (resultado.Estado == EstadoDespachoAtajo.Despachado)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// COM-004: la ventana se cerró —sea por el atajo de cierre (COM-002), la
    /// orden del sistema o el cierre del usuario—. Cesan los canales internos
    /// de la ventana y la sesión se libera con su registro LOG-003 (T-007);
    /// ante un fallo de cierre o de liberación, ERR-005: el proceso queda con
    /// el código de salida de error para terminarse igualmente, sin procesos
    /// residuales.
    /// </summary>
    private void AlCerrarLaVentana(object? sender, EventArgs e)
    {
        ServicioDeIdioma.IdiomaConmutado -= AlConmutarseElIdioma;
        RemoveHandler(InputElement.KeyDownEvent, AlPulsarUnaTecla);

        try
        {
            _gestorDeCierre.Cerrar();
        }
        catch (Exception)
        {
            // ERR-005 ya registrado por el gestor (mensaje en español por la
            // salida de error y LOG-003 con nivel Error): el cierre sigue
            // hasta el final y el proceso termina con código de error.
            Environment.ExitCode = _gestorDeCierre.CodigoDeSalidaPendiente;
        }
    }

    private static IEnumerable<MenuItem> EnumerarElementosDeMenu(Menu barra) =>
        barra.Items.OfType<MenuItem>().SelectMany(EnumerarElementosDeMenu);

    private static IEnumerable<MenuItem> EnumerarElementosDeMenu(MenuItem item)
    {
        yield return item;

        foreach (var hijo in item.Items.OfType<MenuItem>())
        {
            foreach (var elemento in EnumerarElementosDeMenu(hijo))
            {
                yield return elemento;
            }
        }
    }

    /// <summary>Contraparte de cierre (COM-004) sobre la ventana: cerrarla dispara el cierre de la sesión.</summary>
    private sealed class AccionDeCierreDeVentana(MainWindow ventana) : IAccionDeCierre
    {
        public void Ejecutar() => ventana.Close();
    }

    /// <summary>Contraparte de enfoque (T-006) sobre la ventana única existente: traerla al frente con el foco, sin crear otra (BR-003).</summary>
    private sealed class EnfocadorDeVentana(MainWindow ventana) : IEnfocadorDeVentana
    {
        public void Enfocar() => ventana.Activate();
    }
}
