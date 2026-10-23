using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ShortcutHandler _shortcutHandler;
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
        _shortcutHandler = new ShortcutHandler(viewModel);
        DataContext = viewModel;

        // OP-005/OP-006: el atajo de cierre disponible en F-001 solicita el
        // cierre de la ventana; el ViewModel no conoce la plataforma.
        _viewModel.CloseRequested += OnCloseRequested;

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
            if (!_viewModel.IsMenuOpen || !_viewModel.DispatchShortcut("Esc"))
            {
                return;
            }

            CloseOpenMenuItem();
            _lastOpenMenuItem?.Focus();
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

    private void CloseOpenMenuItem()
    {
        if (_lastOpenMenuItem is { IsSubMenuOpen: true } openItem)
        {
            openItem.Close();
        }
    }

    /// <summary>
    /// TR-004: deja el foco en la entrada de menú abierta por Alt+letra y,
    /// si tiene submenú, lo despliega para que el estado de sesión y la
    /// superficie visible coincidan.
    /// </summary>
    private void FocusMenuEntry(string? menuKey)
    {
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            return;
        }

        var item = MainMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(menuItem =>
                string.Equals(menuItem.Tag as string, menuKey, StringComparison.Ordinal));
        if (item is null)
        {
            return;
        }

        if (_lastOpenMenuItem is not null
            && !ReferenceEquals(_lastOpenMenuItem, item)
            && _lastOpenMenuItem.IsSubMenuOpen)
        {
            _lastOpenMenuItem.Close();
        }

        _lastOpenMenuItem = item;
        if (item.HasSubMenu)
        {
            item.IsSubMenuOpen = true;
        }

        item.Focus();
    }

    private bool IsTopLevelMenu(MenuItem item) => MainMenu.Items.Contains(item);
}
