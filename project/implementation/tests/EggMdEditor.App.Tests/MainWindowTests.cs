using System.Linq;
using Avalonia;
using Avalonia.Controls;
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
    }

    [Fact]
    public void ViewModel_ResolvesHeadersInTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        Assert.True(catalog.TrySetLanguage("en"));

        var viewModel = new MainWindowViewModel(catalog);

        Assert.Equal(ExpectedEnglishHeaders, viewModel.Menus.Select(menu => menu.Header));
    }

    [AvaloniaFact]
    public void Window_ShowsTheSixMenusWithoutConnectedActions()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            var menu = window.FindControl<Menu>("MainMenu");

            Assert.NotNull(menu);
            Assert.Same(viewModel, window.DataContext);
            Assert.Equal(6, menu.Items.Count);

            var menuItems = menu.Items.Cast<MenuItem>().ToList();
            Assert.Equal(ExpectedKeys, menuItems.Select(item => item.Tag as string));
            Assert.Equal(ExpectedSpanishHeaders, menuItems.Select(item => item.Header as string));
            Assert.All(menuItems, item =>
            {
                Assert.Null(item.Command);
                Assert.True(item.IsEnabled);
            });

            Assert.Equal(1280, window.Width);
            Assert.Equal(720, window.Height);
        }
        finally
        {
            window.Close();
        }
    }
}
