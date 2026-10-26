using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

// F-001/T-015 (AC-009, OP-007, TR-010/TR-011, BR-F001-010): los 6 rótulos de
// ENT-004 son botones y los botones «Anterior»/«Siguiente» recorren la
// selección cíclicamente sin abrir menús ni alterar el documento.
public class MenuNavigationTests
{
    private static readonly string[] ExpectedSpanishHeaders =
    [
        "Archivo",
        "Edición",
        "Ver",
        "Formato",
        "Herramientas",
        "Ayuda",
    ];

    private static MainWindowViewModel CreateViewModel(out Session session)
    {
        session = new Session(new I18nCatalog());
        return new MainWindowViewModel(session);
    }

    [Fact]
    public void ViewModel_StartsWithArchivoSelected()
    {
        var viewModel = CreateViewModel(out var session);

        Assert.Equal(1, viewModel.SelectedMenuIndex);
        Assert.Equal("Menu.File", viewModel.SelectedMenuKey);
        Assert.Equal("Archivo", viewModel.SelectedMenuHeader);
        Assert.True(viewModel.IsFileMenuSelected);
        Assert.Equal(1, session.SelectedMenuIndex);
    }

    [Fact]
    public void NextMenuAdvancesExactlyOneLabelAndWrapsFromAyudaToArchivo()
    {
        var viewModel = CreateViewModel(out var session);

        for (var expected = 1; expected <= MainWindowViewModel.MenuKeys.Count; expected++)
        {
            Assert.Equal(expected, viewModel.SelectedMenuIndex);
            Assert.Equal(MainWindowViewModel.MenuKeys[expected - 1], viewModel.SelectedMenuKey);
            Assert.True(viewModel.SelectNextMenu());
        }

        // Ayuda → Archivo: no hay tope ni error (TR-010, AC-009).
        Assert.Equal(1, viewModel.SelectedMenuIndex);
        Assert.Equal("Menu.File", viewModel.SelectedMenuKey);
        Assert.Equal(1, session.SelectedMenuIndex);
        Assert.False(viewModel.IsMenuOpen);
    }

    [Fact]
    public void PreviousMenuRetreatsExactlyOneLabelAndWrapsFromArchivoToAyuda()
    {
        var viewModel = CreateViewModel(out var session);

        Assert.True(viewModel.SelectPreviousMenu());
        Assert.Equal(6, viewModel.SelectedMenuIndex);
        Assert.Equal("Menu.Help", viewModel.SelectedMenuKey);
        Assert.Equal("Ayuda", viewModel.SelectedMenuHeader);
        Assert.True(viewModel.IsHelpMenuSelected);
        Assert.Equal(6, session.SelectedMenuIndex);

        for (var expected = 5; expected >= 1; expected--)
        {
            Assert.True(viewModel.SelectPreviousMenu());
            Assert.Equal(expected, viewModel.SelectedMenuIndex);
            Assert.Equal(MainWindowViewModel.MenuKeys[expected - 1], viewModel.SelectedMenuKey);
        }

        // Archivo → Ayuda: sin tope ni error (TR-011, AC-009).
        Assert.True(viewModel.SelectPreviousMenu());
        Assert.Equal(6, viewModel.SelectedMenuIndex);
        Assert.Equal("Menu.Help", viewModel.SelectedMenuKey);
    }

    [Fact]
    public void SelectionRejectsPositionsOutsideTheDeclaredCatalog()
    {
        var session = new Session(new I18nCatalog());

        Assert.Equal(1, session.SelectedMenuIndex);
        Assert.True(session.TrySelectMenu(6));
        Assert.Equal(6, session.SelectedMenuIndex);
        Assert.False(session.TrySelectMenu(0));
        Assert.False(session.TrySelectMenu(7));
        Assert.Equal(6, session.SelectedMenuIndex);
    }

    [Fact]
    public void NavigationDoesNotOpenTheMenuAndClosesOnlyADeployedLabel()
    {
        var viewModel = CreateViewModel(out var session);

        Assert.False(viewModel.IsMenuOpen);
        Assert.True(viewModel.SelectNextMenu());
        Assert.False(viewModel.IsMenuOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);

        // Si había un rótulo desplegado, la navegación no abre otro menú:
        // cierra el anterior para no dejar una superficie sobre otro rótulo.
        Assert.True(viewModel.OpenMenu("Menu.Tools"));
        Assert.True(viewModel.IsMenuOpen);

        Assert.True(viewModel.SelectPreviousMenu());
        Assert.False(viewModel.IsMenuOpen);
        Assert.Null(viewModel.OpenMenuKey);
        Assert.Equal(MenuState.Closed, session.MenuState);
    }

    [AvaloniaFact]
    public void Window_MarksTheSelectedLabelAndItsNavigationButtonsAreAccessible()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var labels = Assert.IsType<StackPanel>(window.FindControl<StackPanel>("MenuLabels"));
            var buttons = labels.Children.OfType<ToggleButton>().ToList();
            Assert.Equal(6, buttons.Count);
            Assert.Equal(ExpectedSpanishHeaders, buttons.Select(button => AutomationProperties.GetName(button)));
            Assert.True(buttons[0].IsChecked);
            Assert.All(buttons.Skip(1), button => Assert.False(button.IsChecked));

            var previous = Assert.IsType<Button>(window.FindControl<Button>("PreviousMenuButton"));
            var next = Assert.IsType<Button>(window.FindControl<Button>("NextMenuButton"));
            Assert.Equal("Anterior", AutomationProperties.GetName(previous));
            Assert.Equal("Siguiente", AutomationProperties.GetName(next));

            // «Siguiente» aumenta exactamente uno y actualiza la marca visible.
            next.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, viewModel.SelectedMenuIndex);
            Assert.Equal("Edición", viewModel.SelectedMenuHeader);
            Assert.False(buttons[0].IsChecked);
            Assert.True(buttons[1].IsChecked);
            Assert.False(viewModel.IsMenuOpen);

            // «Anterior» disminuye exactamente uno.
            previous.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, viewModel.SelectedMenuIndex);
            Assert.True(buttons[0].IsChecked);

            // En el extremo, «Anterior» salta cíclicamente a Ayuda.
            previous.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(6, viewModel.SelectedMenuIndex);
            Assert.Equal("Ayuda", viewModel.SelectedMenuHeader);
            Assert.True(buttons[5].IsChecked);
            Assert.False(buttons[0].IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_NavigationButtonsAndSelectionFollowTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        catalog.TrySetLanguage("en");
        var viewModel = new MainWindowViewModel(catalog);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("Previous", viewModel.PreviousMenuLabel);
            Assert.Equal("Next", viewModel.NextMenuLabel);
            Assert.Equal("File", viewModel.SelectedMenuHeader);

            var previous = Assert.IsType<Button>(window.FindControl<Button>("PreviousMenuButton"));
            var next = Assert.IsType<Button>(window.FindControl<Button>("NextMenuButton"));
            Assert.Equal("Previous", previous.Content as string);
            Assert.Equal("Next", next.Content as string);
            Assert.Equal("Previous", AutomationProperties.GetName(previous));
            Assert.Equal("Next", AutomationProperties.GetName(next));

            // El relabelizado no pierde la selección (BR-F001-005).
            viewModel.SelectNextMenu();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("Edit", viewModel.SelectedMenuHeader);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_MouseClickingALabelSelectsItAndKeepsTheMarkWhenToggling()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var fileButton = Assert.IsType<ToggleButton>(window.FindControl<ToggleButton>("FileMenuButton"));
            var toolsButton = Assert.IsType<ToggleButton>(window.FindControl<ToggleButton>("ToolsMenuButton"));

            Click(window, fileButton);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // El primer clic sobre Archivo abre el menú, pero no desmarca el
            // rótulo seleccionado (ST-011).
            Assert.True(viewModel.IsMenuOpen);
            Assert.Equal(1, viewModel.SelectedMenuIndex);
            Assert.True(fileButton.IsChecked);

            Click(window, toolsButton);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(5, viewModel.SelectedMenuIndex);
            Assert.True(toolsButton.IsChecked);
            Assert.False(fileButton.IsChecked);
            Assert.True(viewModel.IsMenuDropdownOpen);

            // Un segundo clic sobre Herramientas cierra el menú y conserva la
            // marca visual del rótulo seleccionado.
            Click(window, toolsButton);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
            Assert.Equal(5, viewModel.SelectedMenuIndex);
            Assert.True(toolsButton.IsChecked);

            // El clic real en «Siguiente» avanza una sola posición: el
            // manejador marca el clic como atendido y el comando no se
            // ejecuta por segunda vez.
            var next = Assert.IsType<Button>(window.FindControl<Button>("NextMenuButton"));
            Click(window, next);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(6, viewModel.SelectedMenuIndex);
            Assert.True(viewModel.IsHelpMenuSelected);

            Click(window, next);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, viewModel.SelectedMenuIndex);
            Assert.True(fileButton.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_OpeningAMenuDoesNotMoveTheFormatBarOrTheDocumentArea()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var menuBar = Assert.IsType<Border>(window.FindControl<Border>("MainMenuBar"));
            var toolsButton = Assert.IsType<ToggleButton>(window.FindControl<ToggleButton>("ToolsMenuButton"));
            var dropdown = Assert.IsType<Border>(window.FindControl<Border>("MenuDropdown"));
            var formatBar = Assert.IsType<Border>(window.FindControl<Border>("FormatBarContainer"));
            var documentArea = Assert.IsType<Border>(window.FindControl<Border>("DocumentArea"));

            var formatTopBefore = formatBar.TranslatePoint(new Point(0, 0), window);
            var documentTopBefore = documentArea.TranslatePoint(new Point(0, 0), window);
            Assert.NotNull(formatTopBefore);
            Assert.NotNull(documentTopBefore);

            toolsButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // BR-F001-001: la barra de menú es persistente y el submenú no
            // desplaza la barra de formato ni el área de documento.
            Assert.True(viewModel.IsMenuOpen);
            Assert.Equal(formatTopBefore, formatBar.TranslatePoint(new Point(0, 0), window));
            Assert.Equal(documentTopBefore, documentArea.TranslatePoint(new Point(0, 0), window));

            // El submenú se ancla al borde inferior de la barra de menú.
            var dropdownTop = dropdown.TranslatePoint(new Point(0, 0), window);
            Assert.NotNull(dropdownTop);
            Assert.Equal(menuBar.Bounds.Height, dropdownTop.Value.Y, precision: 0);
        }
        finally
        {
            window.Close();
        }
    }

    private static void Click(Window window, Control control)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        Assert.NotNull(point);
        window.MouseDown(point.Value, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point.Value, MouseButton.Left, RawInputModifiers.None);
    }

    [AvaloniaFact]
    public void Window_NavigatingWithADeployedMenuDoesNotShowASubmenu()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var toolsButton = Assert.IsType<ToggleButton>(window.FindControl<ToggleButton>("ToolsMenuButton"));
            var dropdown = Assert.IsType<Border>(window.FindControl<Border>("MenuDropdown"));
            var next = Assert.IsType<Button>(window.FindControl<Button>("NextMenuButton"));

            toolsButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(viewModel.IsMenuOpen);
            Assert.True(dropdown.IsVisible);

            next.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // OP-007/AC-009: la navegación no abre un menú ni deja visible el
            // submenú del rótulo anterior.
            Assert.False(viewModel.IsMenuOpen);
            Assert.False(dropdown.IsVisible);
            Assert.Equal(6, viewModel.SelectedMenuIndex);
            Assert.True(viewModel.IsHelpMenuSelected);
        }
        finally
        {
            window.Close();
        }
    }
}
