using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using EggMdEditor.App.I18n;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(EggMdEditor.App.Tests.TestAppBuilder))]

namespace EggMdEditor.App.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

// Ventana principal SC-001 (T-006): menú principal de los 6 menús de FR-011
// (ENT-004), rótulos i18n en el idioma activo y ninguna acción conectada
// (ADR-001).
public class MainWindowTests
{
    private static readonly string[] ExpectedKeys =
    [
        "Menu.File",
        "Menu.Edit",
        "Menu.View",
        "Menu.Format",
        "Menu.Tools",
        "Menu.Help",
    ];

    private static readonly string[] ExpectedSpanishHeaders =
    [
        "Archivo",
        "Edición",
        "Ver",
        "Formato",
        "Herramientas",
        "Ayuda",
    ];

    private static readonly string[] ExpectedEnglishHeaders =
    [
        "File",
        "Edit",
        "View",
        "Format",
        "Tools",
        "Help",
    ];

    [Fact]
    public void ViewModel_ExposesTheSixFr011MenusInSpanishByDefault()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());

        Assert.Equal(ExpectedKeys, viewModel.Menus.Select(menu => menu.Key));
        Assert.Equal(ExpectedSpanishHeaders, viewModel.Menus.Select(menu => menu.Header));
        Assert.Equal("Archivo", viewModel.FileMenuHeader);
        Assert.Equal("Ayuda", viewModel.HelpMenuHeader);
        Assert.Equal(1, viewModel.SelectedMenuIndex);
        Assert.Equal("Menu.File", viewModel.SelectedMenuKey);
        Assert.Equal("Archivo", viewModel.SelectedMenuHeader);
        Assert.True(viewModel.IsFileMenuSelected);
        Assert.Equal("Anterior", viewModel.PreviousMenuLabel);
        Assert.Equal("Siguiente", viewModel.NextMenuLabel);
    }

    [Fact]
    public void ViewModel_ResolvesHeadersInTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        Assert.True(catalog.TrySetLanguage("en"));

        var viewModel = new MainWindowViewModel(catalog);

        Assert.Equal(ExpectedEnglishHeaders, viewModel.Menus.Select(menu => menu.Header));
        Assert.Equal("Previous", viewModel.PreviousMenuLabel);
        Assert.Equal("Next", viewModel.NextMenuLabel);
    }

    [AvaloniaFact]
    public void Window_ShowsTheSixMenuLabelsAsButtonsAndTheTwoNavigationButtons()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var labels = window.FindControl<StackPanel>("MenuLabels");
            Assert.NotNull(labels);

            var buttons = labels.Children.OfType<ToggleButton>().ToList();
            Assert.Equal(6, buttons.Count);
            Assert.Equal(ExpectedKeys, buttons.Select(button => button.Tag as string));
            Assert.Equal(
                ExpectedSpanishHeaders,
                buttons.Select(button => button.Content as string));
            Assert.Equal(
                ExpectedSpanishHeaders,
                buttons.Select(button => AutomationProperties.GetName(button)));
            Assert.All(buttons, button =>
            {
                Assert.Null(button.Command);
                Assert.True(button.IsEnabled);
                Assert.True(button.IsVisible);
            });

            // ST-011: Archivo nace seleccionado y el estado es visible y
            // accesible mediante el ToggleButton marcado.
            Assert.True(buttons[0].IsChecked);
            Assert.All(buttons.Skip(1), button => Assert.False(button.IsChecked));

            var previous = window.FindControl<Button>("PreviousMenuButton");
            var next = window.FindControl<Button>("NextMenuButton");
            Assert.NotNull(previous);
            Assert.NotNull(next);
            Assert.Equal("Anterior", previous.Content as string);
            Assert.Equal("Siguiente", next.Content as string);
            Assert.Equal("Anterior", AutomationProperties.GetName(previous));
            Assert.Equal("Siguiente", AutomationProperties.GetName(next));
            Assert.Same(viewModel.SelectPreviousMenuCommand, previous.Command);
            Assert.Same(viewModel.SelectNextMenuCommand, next.Command);

            Assert.Equal(1280, window.Width);
            Assert.Equal(720, window.Height);
        }
        finally
        {
            window.Close();
        }
    }
}
