using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using EggMdEditor.App.Startup;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ShortcutHandler _shortcutHandler;
    private Button? _lastOpenMenuButton;
    private HelpWindow? _helpWindow;

    /// <summary>
    /// Ciclo de vida técnico del proceso (LOG-003, ERR-005). La composición de
    /// la aplicación lo inyecta; sin él el cierre mantiene el mismo
    /// comportamiento, solo que sin registro técnico.
    /// </summary>
    public ApplicationLifecycle? Lifecycle { get; set; }

    public MainWindow()
        : this(new MainWindowViewModel(), null)
    {
    }

    public MainWindow(MainWindowViewModel viewModel)
        : this(viewModel, null)
    {
    }

    public MainWindow(MainWindowViewModel viewModel, ApplicationLifecycle? lifecycle)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Lifecycle = lifecycle;
        InitializeComponent();
        _viewModel = viewModel;
        _shortcutHandler = new ShortcutHandler(viewModel);
        DataContext = viewModel;

        // OP-005/OP-006: el atajo de cierre disponible en F-001 solicita el
        // cierre de la ventana; el ViewModel no conoce la plataforma.
        _viewModel.CloseRequested += OnCloseRequested;

        // SC-003: la superficie de ayuda abierta sigue la conmutación de
        // idioma de SC-001 en caliente (BR-013, COM-002).
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // El túnel permite observar Esc y el clic fuera aunque el control
        // pulsado marque el evento como manejado en la fase de burbuja.
        AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// TR-004/COM-006: activar un rótulo de ENT-004 lo selecciona y abre o
    /// cierra el menú (ST-005 ↔ ST-006), dejando el foco en el botón. Las
    /// entradas de F-001 no tienen acciones de dominio conectadas (ADR-001).
    /// </summary>
    private void OnMenuLabelClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string menuKey)
        {
            return;
        }

        _lastOpenMenuButton = button;
        _viewModel.ToggleMenu(menuKey);
        button.Focus();
    }

    /// <summary>
    /// OP-007/TR-010/TR-011: los botones «Anterior»/«Siguiente» cambian el
    /// rótulo seleccionado. El manejador marca el clic como atendido para que
    /// el comando enlazado no lo ejecute por segunda vez; así la acción es
    /// idéntica tanto con interacción real como en pruebas de superficie.
    /// </summary>
    private void OnMenuNavigationClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        switch (button.Tag as string)
        {
            case "Navigation.Previous":
                _viewModel.SelectPreviousMenu();
                break;
            case "Navigation.Next":
                _viewModel.SelectNextMenu();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    /// <summary>
    /// SC-003 (T-014): ventana de ayuda abierta, o <c>null</c> si no hay
    /// ninguna. La superficie es única por SC-001 y se reutiliza al navegar
    /// entre secciones.
    /// </summary>
    public HelpWindow? HelpWindow => _helpWindow;

    /// <summary>
    /// Abre SC-003 en la sección indicada. Si ya está abierta, la activa y
    /// navega a la sección sin crear otra ventana. La superficie se muestra
    /// no modal para que la conmutación de idioma de SC-001 la relabelice en
    /// caliente (BR-F001-005).
    /// </summary>
    public HelpWindow OpenHelp(HelpSection section)
    {
        if (_helpWindow is { } existing)
        {
            existing.SelectSection(section);
            if (existing.WindowState == WindowState.Minimized)
            {
                existing.WindowState = WindowState.Normal;
            }

            existing.Activate();
            return existing;
        }

        var viewModel = new HelpWindowViewModel(
            _viewModel.Session,
            _viewModel.Catalog,
            section,
            () => _viewModel.AvailableShortcuts);
        var window = new HelpWindow(viewModel);
        window.Closed += OnHelpWindowClosed;
        _helpWindow = window;
        window.Show(this);
        return window;
    }

    /// <summary>
    /// Entrada del menú Ayuda (SC-003, FR-010): abre la superficie en la
    /// sección correspondiente. El contenido estático no altera el documento
    /// ni el resto del estado de la sesión (ADR-001).
    /// </summary>
    private void OnHelpEntryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var section = button.Tag switch
        {
            "Help.Guide" => HelpSection.Guide,
            "Help.Shortcuts" => HelpSection.Shortcuts,
            _ => HelpSection.About,
        };

        _viewModel.CloseMenu();
        OpenHelp(section);
        e.Handled = true;
    }

    private void OnHelpWindowClosed(object? sender, EventArgs e)
    {
        if (sender is not HelpWindow window)
        {
            return;
        }

        window.Closed -= OnHelpWindowClosed;
        if (ReferenceEquals(_helpWindow, window))
        {
            _helpWindow = null;
        }
    }

    /// <summary>
    /// SC-003/BR-013: al conmutar el idioma activo se relabeliza también la
    /// superficie de ayuda si está abierta, sin recrearla.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentLanguage))
        {
            _helpWindow?.ViewModel.RefreshLanguage();
        }
    }

    /// <summary>
    /// OP-004/TR-006 (COM-004): el puntero entra en un control válido de la
    /// barra de formato (ENT-005) y se muestra su aviso flotante.
    /// </summary>
    private void OnFormatControlPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control control && control.Tag is string key)
        {
            _viewModel.ShowNotice(key);
        }
    }

    /// <summary>
    /// OP-004/TR-007: al retirar el puntero del control se oculta el aviso;
    /// el control que sale debe ser el que originó el aviso visible.
    /// </summary>
    private void OnFormatControlPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Control control && control.Tag is string key)
        {
            _viewModel.HideNotice(key);
        }
    }

    /// <summary>
    /// TR-005: un clic fuera del menú cierra la entrada desplegada. El foco
    /// sigue al elemento pulsado, nunca a una entrada ya cerrada.
    /// </summary>
    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_viewModel.IsMenuOpen)
        {
            return;
        }

        if (e.Source is Visual source
            && (IsWithinMenuBar(source) || IsWithinMenuDropdown(source)))
        {
            return;
        }

        _viewModel.CloseMenu();
    }

    /// <summary>
    /// OP-005/BR-F001-003: handler de atajos de SC-001. Alt+letra abre la
    /// entrada correspondiente de ENT-004 (TR-004), Esc cierra el menú
    /// desplegado (TR-005) y Ctrl+W cierra la ventana (TR-001). Cualquier
    /// otro atajo o uno cuya acción no exista en F-001 queda inerte, sin
    /// efecto observable sobre el documento (ADR-001).
    /// </summary>
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            // SC-002: con la superficie modal abierta, Esc la cierra o
            // cancela la confirmación de descarte antes que el menú.
            if (_viewModel.Dialog.IsOpen)
            {
                _viewModel.Dialog.RequestClose();
                e.Handled = true;
                return;
            }

            if (!_viewModel.IsMenuOpen || !_viewModel.DispatchShortcut("Esc"))
            {
                return;
            }

            _lastOpenMenuButton?.Focus();
            e.Handled = true;
            return;
        }

        if (!_shortcutHandler.TryHandle(e, out var shortcut))
        {
            // Atajo no registrado, inactivo o de una acción que pertenece a
            // otra Feature: se deja pasar sin efecto (BR-F001-003).
            return;
        }

        if (string.Equals(shortcut?.Command, CommandNames.OpenMenu, StringComparison.OrdinalIgnoreCase))
        {
            FocusMenuEntry(shortcut?.Argument);
        }

        e.Handled = true;
    }

    private void OnCloseRequested(object? sender, EventArgs e) => Close();

    /// <summary>
    /// OP-006/TR-001: el cierre de SC-001 finaliza la sesión (ST-001 →
    /// ST-002), también cuando lo origina el control de la barra de título y
    /// no el atajo o el ViewModel. No queda estado transitorio activo.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        // LOG-003 (inicio del cierre): debe quedar registrado antes de
        // liberar la sesión. En un arranque fallido no hay cierre que anotar.
        Lifecycle?.BeginShutdown();

        try
        {
            _viewModel.Session?.CloseWindow();
        }
        catch (Exception exception)
        {
            // ERR-005: el cierre falla, pero el proceso termina igualmente y
            // sin rollback; el mensaje en español y LOG-003 quedan a cargo
            // del ciclo de vida.
            Lifecycle?.ReportShutdownFailure(exception);
        }
        finally
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            try
            {
                _helpWindow?.Close();
            }
            catch (Exception)
            {
                // La ventana auxiliar no impide la finalización de SC-001.
            }

            base.OnClosed(e);
        }
    }

    /// <summary>
    /// TR-004: deja el foco en el rótulo abierto por Alt+letra. El estado del
    /// menú ya lo actualizó el dispatcher; la superficie visible se enlaza al
    /// ViewModel.
    /// </summary>
    private void FocusMenuEntry(string? menuKey)
    {
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            return;
        }

        var button = MenuLabelButtons.FirstOrDefault(menuButton =>
            string.Equals(menuButton.Tag as string, menuKey, StringComparison.Ordinal));
        if (button is null)
        {
            return;
        }

        _lastOpenMenuButton = button;
        button.Focus();
    }

    private IEnumerable<Button> MenuLabelButtons
    {
        get
        {
            yield return FileMenuButton;
            yield return EditMenuButton;
            yield return ViewMenuButton;
            yield return FormatMenuButton;
            yield return ToolsMenuButton;
            yield return HelpMenuButton;
        }
    }

    private bool IsWithinMenuBar(Visual source) =>
        ReferenceEquals(source, MainMenuBar) || MainMenuBar.IsVisualAncestorOf(source);

    private bool IsWithinMenuDropdown(Visual source) =>
        ReferenceEquals(source, MenuDropdown) || MenuDropdown.IsVisualAncestorOf(source);
}
