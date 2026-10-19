using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    /// <summary>
    /// Constructor sin argumentos para el diseñador de XAML: crea la sesión
    /// en su defecto de arranque (idioma español, BR-007).
    /// </summary>
    public MainWindow() : this(new SesionDeAplicacion())
    {
    }

    /// <summary>
    /// Crea la ventana única de la aplicación (BR-003) y construye su barra
    /// de menú persistente desde <see cref="CatalogoMenuPrincipal"/> —fuente
    /// única de recuento y de orden de FR-011 (DS-001)— con los rótulos en el
    /// idioma activo de la sesión y todas las entradas visibles sin acción
    /// conectada (ADR-001). Sin barra de formato (T-009), sin entrada de
    /// idioma conmutada (T-010) y sin servicios cableados (T-011): T-011
    /// inyectará la sesión compartida y conectará COM-001–COM-004.
    /// </summary>
    public MainWindow(SesionDeAplicacion sesion)
    {
        ArgumentNullException.ThrowIfNull(sesion);

        InitializeComponent();

        ConstructorDeMenuPrincipal.Construir(BarraMenu, sesion);
    }
}
