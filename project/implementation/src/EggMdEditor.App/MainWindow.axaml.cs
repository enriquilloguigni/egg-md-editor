using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private MenuItem? _lastOpenMenuItem;

    public MainWindow()
        : this(new MainWindowViewModel())
    {
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // OP-003/TR-004/TR-005 (COM-006): el cierre del menú se refleja en
        // cuanto la entrada deja de estar desplegada, tanto si lo cierra el
        // propio control (Esc/clic fuera) como si lo cierra la ventana.
        foreach (var item in MainMenu.Items)
        {
            if (item is MenuItem menuItem)
            {
                menuItem.PropertyChanged += OnTopLevelMenuItemPropertyChanged;
            }
        }

        // El túnel permite observar Esc y el clic fuera aunque el control
        // pulsado marque el evento como manejado en la fase de burbuja.
        AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// TR-004: abrir una entrada de menú de ENT-004 actualiza el estado del
    /// menú (ST-005 → ST-006) y deja el foco en la entrada. Las entradas de
    /// F-001 no tienen acciones de dominio conectadas (ADR-001).
    /// </summary>
    private void OnMenuEntryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item || !ReferenceEquals(e.Source, item))
        {
            return;
        }

        // Las entradas con submenú ya se abren por sí mismas; su ciclo de
        // vida se sigue con IsSubMenuOpen (SubmenuOpened/PropertyChanged).
        if (item.HasSubMenu)
        {
            return;
        }

        _viewModel.ToggleMenu(item.Tag as string);
        if (_viewModel.IsMenuOpen)
        {
            _lastOpenMenuItem = item;
            item.Focus();
        }
    }

    /// <summary>
    /// TR-004 (COM-006): una entrada con submenú se despliega; el estado de
    /// sesión pasa a ST-006 antes de que el usuario interactúe con él.
    /// </summary>
    private void OnMenuSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item
            || !ReferenceEquals(e.Source, item)
            || !IsTopLevelMenu(item))
        {
            return;
        }

        _lastOpenMenuItem = item;
        _viewModel.OpenMenu(item.Tag as string);
    }

    /// <summary>
    /// TR-005: al cerrarse la entrada desplegada (Esc o clic fuera) el estado
    /// vuelve a ST-005.
    /// </summary>
    private void OnTopLevelMenuItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != MenuItem.IsSubMenuOpenProperty
            || sender is not MenuItem item
            || item.IsSubMenuOpen
            || !IsTopLevelMenu(item))
        {
            return;
        }

        _viewModel.CloseMenu(item.Tag as string);
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
            && (ReferenceEquals(source, MainMenu) || MainMenu.IsVisualAncestorOf(source)))
        {
            return;
        }

        _viewModel.CloseMenu();
    }

    /// <summary>
    /// TR-005: Esc cierra el menú desplegado y devuelve el foco a la entrada
    /// que lo abrió, sin ejecutar ninguna acción de dominio (ADR-001).
    /// </summary>
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !_viewModel.IsMenuOpen)
        {
            return;
        }

        if (_lastOpenMenuItem is { IsSubMenuOpen: true } openItem)
        {
            openItem.Close();
        }

        _viewModel.CloseMenu();
        _lastOpenMenuItem?.Focus();
        e.Handled = true;
    }

    private bool IsTopLevelMenu(MenuItem item) => MainMenu.Items.Contains(item);
}
