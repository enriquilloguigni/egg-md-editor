using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

// F-001/T-010: apertura/cierre del menú (OP-003, COM-006, TR-004/TR-005) y
// aviso flotante por control de la barra (OP-004, COM-004, TR-006/TR-007).
// Solo estado y foco: ninguna entrada ni control se conecta a una acción de
// dominio (ADR-001).
public class MenuAndNoticeTests
{
    [Fact]
    public void Session_MenuStateOnlyAdmitsTheDeclaredCatalog()
    {
        var session = new Session();

        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.True(session.TryOpenMenu());
        Assert.Equal(MenuState.Open, session.MenuState);

        // {Cerrado, Desplegado}: un valor ajeno al catálogo no altera el estado.
        Assert.False(session.TrySetMenuState((MenuState)42));
        Assert.Equal(MenuState.Open, session.MenuState);

        Assert.True(session.TryCloseMenu());
        Assert.Equal(MenuState.Closed, session.MenuState);

        // TR-005 solo aplica con el menú desplegado.
        Assert.False(session.TryCloseMenu());
    }

    [Fact]
    public void Session_OpenMenuRequiresTheWindowOpen()
    {
        var session = new Session();
        session.WindowOpen = false;

        Assert.False(session.TryOpenMenu());
        Assert.Equal(MenuState.Closed, session.MenuState);
    }

    [Fact]
    public void ViewModel_OpensAndClosesMenuEntriesPreservingTheState()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);

        Assert.False(viewModel.IsMenuOpen);
        Assert.Null(viewModel.OpenMenuKey);

        Assert.True(viewModel.OpenMenu("Menu.Tools"));
        Assert.True(viewModel.IsMenuOpen);
        Assert.Equal(MenuState.Open, session.MenuState);
        Assert.Equal("Menu.Tools", viewModel.OpenMenuKey);

        // Una entrada desconocida no altera el estado (ENT-004).
        Assert.False(viewModel.OpenMenu("Menu.NotDeclared"));
        Assert.Equal("Menu.Tools", viewModel.OpenMenuKey);

        // El cierre de otra entrada no cierra la que está desplegada.
        Assert.False(viewModel.CloseMenu("Menu.File"));
        Assert.True(viewModel.IsMenuOpen);

        Assert.True(viewModel.CloseMenu("Menu.Tools"));
        Assert.False(viewModel.IsMenuOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.Null(viewModel.OpenMenuKey);

        // Clic repetido sobre la misma entrada: abrir/cerrar.
        Assert.True(viewModel.ToggleMenu("Menu.File"));
        Assert.Equal("Menu.File", viewModel.OpenMenuKey);
        Assert.True(viewModel.ToggleMenu("Menu.File"));
        Assert.False(viewModel.IsMenuOpen);
    }

    [Fact]
    public void ViewModel_CannotOpenTheMenuWhenTheWindowIsClosed()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        session.WindowOpen = false;

        Assert.False(viewModel.OpenMenu("Menu.Tools"));
        Assert.False(viewModel.IsMenuOpen);
    }

    [Fact]
    public void ViewModel_ShowsAndHidesTheNoticeOfValidBarControls()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);

        Assert.False(viewModel.NoticeVisible);
        Assert.Equal(string.Empty, viewModel.VisibleNotice);

        Assert.True(viewModel.ShowNotice("barra.enfasis"));
        Assert.True(viewModel.NoticeVisible);
        Assert.True(session.NoticeVisible);
        Assert.Equal("barra.enfasis", viewModel.ActiveNoticeKey);
        Assert.Equal("Énfasis", viewModel.VisibleNotice);

        // OP-004: solo un control válido de ENT-005 muestra aviso.
        Assert.False(viewModel.ShowNotice("barra.no-declarado"));
        Assert.Equal("barra.enfasis", viewModel.ActiveNoticeKey);

        // Retirar el puntero de otro control no oculta el aviso visible.
        Assert.False(viewModel.HideNotice("barra.codigo"));
        Assert.True(viewModel.NoticeVisible);
        Assert.Equal("Énfasis", viewModel.VisibleNotice);

        Assert.True(viewModel.HideNotice("barra.enfasis"));
        Assert.False(viewModel.NoticeVisible);
        Assert.False(session.NoticeVisible);
        Assert.Null(viewModel.ActiveNoticeKey);
        Assert.Equal(string.Empty, viewModel.VisibleNotice);
    }

    [Fact]
    public void ViewModel_PreservesMenuAndNoticeOnLanguageSwitchAndRelabelsTheNotice()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);

        Assert.True(viewModel.OpenMenu("Menu.Help"));
        Assert.True(viewModel.ShowNotice("barra.enlace"));

        Assert.True(viewModel.TryChangeLanguage("en"));

        // BR-F001-005/AC-002: ventana, estado del menú y aviso se conservan.
        Assert.True(viewModel.IsMenuOpen);
        Assert.Equal("Menu.Help", viewModel.OpenMenuKey);
        Assert.Equal(MenuState.Open, session.MenuState);
        Assert.True(viewModel.NoticeVisible);
        Assert.True(session.NoticeVisible);
        Assert.Equal("barra.enlace", viewModel.ActiveNoticeKey);

        // COM-004/VR-002: el aviso se relabeliza en el idioma activo.
        Assert.Equal("Link", viewModel.VisibleNotice);
    }

    [AvaloniaFact]
    public void Window_TracksSubmenuOpeningAndClosingState()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var toolsMenu = window.FindControl<MenuItem>("ToolsMenu");
            Assert.NotNull(toolsMenu);

            // TR-004: la entrada con submenú se despliega y el estado pasa a
            // ST-006 sin ejecutar la acción de ninguna opción.
            toolsMenu.IsSubMenuOpen = true;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(viewModel.IsMenuOpen);
            Assert.Equal("Menu.Tools", viewModel.OpenMenuKey);

            // TR-005: al cerrarse el submenú el estado vuelve a ST-005.
            toolsMenu.IsSubMenuOpen = false;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
            Assert.Null(viewModel.OpenMenuKey);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_MenuEntryKeepsStateAndFocusUntilEscape()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var fileMenu = window.FindControl<MenuItem>("FileMenu");
            Assert.NotNull(fileMenu);

            fileMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(viewModel.IsMenuOpen);
            Assert.Equal("Menu.File", viewModel.OpenMenuKey);
            Assert.True(fileMenu.IsFocused);

            // TR-005: Esc cierra el menú y devuelve el foco a la entrada.
            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
            Assert.Null(viewModel.OpenMenuKey);
            Assert.True(fileMenu.IsFocused);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_ClickOutsideClosesTheOpenMenu()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var fileMenu = window.FindControl<MenuItem>("FileMenu");
            var documentArea = window.FindControl<Border>("DocumentArea");
            Assert.NotNull(fileMenu);
            Assert.NotNull(documentArea);

            fileMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(viewModel.IsMenuOpen);

            var point = documentArea.TranslatePoint(new Point(20, 20), window);
            Assert.NotNull(point);
            window.MouseDown(point.Value, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point.Value, MouseButton.Left, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_TooltipNoticeFollowsThePointerOverBarControls()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var bar = window.FindControl<ItemsControl>("FormatBar");
            Assert.NotNull(bar);

            var button = bar.GetVisualDescendants().OfType<Button>().First();
            var documentArea = window.FindControl<Border>("DocumentArea");
            Assert.NotNull(documentArea);

            var overButton = button.TranslatePoint(
                new Point(button.Bounds.Width / 2, button.Bounds.Height / 2),
                window);
            Assert.NotNull(overButton);

            // TR-006: el puntero entra en el control y el aviso se muestra.
            window.MouseMove(overButton.Value, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("barra.enfasis", button.Tag as string);
            Assert.True(viewModel.NoticeVisible);
            Assert.True(viewModel.IsNoticeVisible);
            Assert.Equal("barra.enfasis", viewModel.ActiveNoticeKey);
            Assert.Equal("Énfasis", viewModel.VisibleNotice);

            var away = documentArea.TranslatePoint(new Point(20, 20), window);
            Assert.NotNull(away);

            // TR-007: al retirar el puntero el aviso se oculta.
            window.MouseMove(away.Value, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.NoticeVisible);
            Assert.Null(viewModel.ActiveNoticeKey);
        }
        finally
        {
            window.Close();
        }
    }
}
