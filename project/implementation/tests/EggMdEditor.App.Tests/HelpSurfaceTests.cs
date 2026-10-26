using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using EggMdEditor.App.I18n;
using Xunit;
using AvaloniaThemeVariant = Avalonia.Styling.ThemeVariant;

namespace EggMdEditor.App.Tests;

// SC-003 (T-014): superficie de ayuda con Acerca de (versión, atribuciones y
// licencias MIT), guía de Markdown y listado de atajos, con navegación por
// secciones y texto seleccionable; contenido completo conectado por F-010
// (screens.md, SC-003, FR-010, NFR-011).
public class HelpSurfaceTests
{
    private static readonly string[] ExpectedKeys =
    [
        HelpWindowViewModel.AboutKey,
        HelpWindowViewModel.GuideKey,
        HelpWindowViewModel.ShortcutsKey,
    ];

    [Fact]
    public void HelpWindowViewModel_ExposesTheThreeSectionsInTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        var viewModel = new HelpWindowViewModel(session, catalog);

        Assert.Equal(ExpectedKeys, viewModel.Sections.Select(section => section.Key));
        Assert.Equal("Ayuda — Egg MD Editor", viewModel.WindowTitle);
        Assert.Equal("Secciones", viewModel.SectionsHeader);
        Assert.Equal("Copiar texto", viewModel.CopyTextLabel);
        Assert.Equal("Cerrar", viewModel.CloseLabel);
        Assert.Equal(HelpSection.About, viewModel.ActiveSectionKind);

        // Acerca de: versión, atribuciones y licencia MIT (FR-010, NFR-011).
        Assert.Contains("Egg MD Editor", viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("Versión: " + viewModel.Version, viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("Avalonia UI", viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("MIT", viewModel.Content, StringComparison.Ordinal);

        // BR-013/COM-002: la superficie se relabeliza en caliente.
        Assert.True(session.TrySetLanguage("en"));
        Assert.Equal("Help — Egg MD Editor", viewModel.WindowTitle);
        Assert.Equal("Sections", viewModel.SectionsHeader);
        Assert.Equal("Copy text", viewModel.CopyTextLabel);
        Assert.Equal("Close", viewModel.CloseLabel);
        Assert.Equal(
            new[] { "About", "Markdown guide", "Keyboard shortcuts" },
            viewModel.Sections.Select(section => section.Title));
        Assert.Contains("Version: " + viewModel.Version, viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("MIT license", viewModel.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void HelpWindowViewModel_ExposesTheGuideAndTheAvailableShortcuts()
    {
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        var shortcuts = new List<HelpShortcutDefinition>
        {
            new("Alt+A", "Abrir el menú Archivo"),
            new("Esc", "Cerrar menús y diálogos"),
            new("Ctrl+W", "Cerrar la ventana"),
        };
        var viewModel = new HelpWindowViewModel(session, catalog, HelpSection.Guide, () => shortcuts);

        Assert.Equal(HelpSection.Guide, viewModel.ActiveSectionKind);
        Assert.Contains("# Encabezado 1", viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("| Columna | Columna |", viewModel.Content, StringComparison.Ordinal);

        viewModel.SelectSection(HelpSection.Shortcuts);
        Assert.Equal(HelpSection.Shortcuts, viewModel.ActiveSectionKind);
        Assert.Contains("Alt+A", viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("Ctrl+W", viewModel.Content, StringComparison.Ordinal);
        Assert.Contains("Abrir el menú Archivo", viewModel.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowViewModel_ListsOnlyTheShortcutsAvailableInF001()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());

        var shortcuts = viewModel.AvailableShortcuts;

        // BR-F001-003: solo los atajos cuya acción existe en F-001; el resto
        // permanece inerte y no se lista.
        Assert.Contains(shortcuts, shortcut => shortcut.Combination == "Esc");
        Assert.Contains(shortcuts, shortcut => shortcut.Combination == "Ctrl+W");
        Assert.Contains(shortcuts, shortcut => shortcut.Combination.StartsWith("Alt+", StringComparison.Ordinal));
        Assert.Equal(MainWindowViewModel.MenuKeys.Count + 2, shortcuts.Count);
        Assert.All(shortcuts, shortcut => Assert.False(string.IsNullOrWhiteSpace(shortcut.Description)));
    }

    [Fact]
    public void HelpWindowViewModel_SelectsTheRequestedSectionAndIgnoresUnknownOnes()
    {
        var viewModel = new HelpWindowViewModel(null, new I18nCatalog(), HelpSection.Shortcuts);

        Assert.Equal(HelpSection.Shortcuts, viewModel.ActiveSectionKind);
        Assert.Equal("Atajos de teclado", viewModel.ActiveSectionTitle);

        viewModel.SelectSection(HelpSection.About);
        Assert.Equal(HelpSection.About, viewModel.ActiveSectionKind);

        viewModel.ActiveSection = null;
        Assert.Equal(HelpSection.About, viewModel.ActiveSectionKind);
    }

    [Fact]
    public void ApplicationVersionIsResolvedFromTheAssembly()
    {
        var version = HelpWindowViewModel.ResolveVersion();

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.DoesNotContain('+', version);
        Assert.Matches(@"^\d+\.\d+", version);
    }

    [AvaloniaFact]
    public void HelpWindowShowsTheSectionsWithSelectableContentAndClosesWithEscape()
    {
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        var viewModel = new HelpWindowViewModel(session, catalog);
        var window = new HelpWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var list = window.FindControl<ListBox>("HelpSectionList");
            var content = window.FindControl<SelectableTextBlock>("HelpContent");
            var title = window.FindControl<TextBlock>("HelpSectionTitle");
            var navigation = window.FindControl<Border>("HelpNavigation");
            var copy = window.FindControl<Button>("CopyHelpTextButton");
            var close = window.FindControl<Button>("CloseHelpButton");
            Assert.NotNull(list);
            Assert.NotNull(content);
            Assert.NotNull(title);
            Assert.NotNull(navigation);
            Assert.NotNull(copy);
            Assert.NotNull(close);

            Assert.NotNull(navigation.Background);
            Assert.Equal(3, list.ItemCount);
            Assert.Equal("Acerca de", title.Text);
            Assert.Contains("MIT", content.Text, StringComparison.Ordinal);
            Assert.Equal("Copiar texto", copy.Content);
            Assert.Equal("Cerrar", close.Content);

            // SC-003: navegación por secciones con texto seleccionable.
            viewModel.SelectSection(HelpSection.Guide);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("Guía de Markdown", title.Text);
            Assert.Contains("# Encabezado 1", content.Text, StringComparison.Ordinal);
            Assert.Same(viewModel.ActiveSection, list.SelectedItem);

            // La copia es una acción de superficie: no rompe con el
            // portapapeles no disponible de la plataforma headless.
            copy.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // SC-003: cierre con Esc.
            window.KeyPressQwerty(Avalonia.Input.PhysicalKey.Escape, Avalonia.Input.RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MainWindowOpensHelpFromTheAyudaMenuAndReusesTheSurface()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var helpButton = Assert.IsType<ToggleButton>(window.FindControl<ToggleButton>("HelpMenuButton"));
            Assert.NotNull(helpButton);
            Assert.Equal("Guía de Markdown", Assert.IsType<Button>(window.FindControl<Button>("HelpGuideItem")).Content as string);
            Assert.Equal("Atajos de teclado", Assert.IsType<Button>(window.FindControl<Button>("HelpShortcutsItem")).Content as string);
            Assert.IsType<Separator>(window.FindControl<Separator>("HelpMenuSeparator"));
            Assert.Equal("Acerca de Egg MD Editor", Assert.IsType<Button>(window.FindControl<Button>("HelpAboutItem")).Content as string);

            helpButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(viewModel.IsMenuOpen);

            var about = Assert.IsType<Button>(window.FindControl<Button>("HelpAboutItem"));
            about.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var help = window.HelpWindow;
            Assert.NotNull(help);
            Assert.True(help.IsVisible);
            Assert.Equal(HelpSection.About, help.ViewModel.ActiveSectionKind);
            Assert.Equal("Acerca de", help.ViewModel.ActiveSectionTitle);

            // La misma superficie se reutiliza al navegar desde el menú.
            var guide = Assert.IsType<Button>(window.FindControl<Button>("HelpGuideItem"));
            guide.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Same(help, window.HelpWindow);
            Assert.Equal(HelpSection.Guide, help.ViewModel.ActiveSectionKind);

            var shortcuts = Assert.IsType<Button>(window.FindControl<Button>("HelpShortcutsItem"));
            shortcuts.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Same(help, window.HelpWindow);
            Assert.Equal(HelpSection.Shortcuts, help.ViewModel.ActiveSectionKind);
            Assert.Contains("Alt+", help.ViewModel.Content, StringComparison.Ordinal);

            help.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Null(window.HelpWindow);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void OpenHelpWindowRelabelsOnLanguageSwitch()
    {
        var catalog = new I18nCatalog();
        var viewModel = new MainWindowViewModel(catalog);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var help = window.OpenHelp(HelpSection.Guide);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var sectionList = Assert.IsType<ListBox>(help.FindControl<ListBox>("HelpSectionList"));
            Assert.Equal("Ayuda — Egg MD Editor", help.Title);
            Assert.Equal("Guía de Markdown", help.ViewModel.ActiveSectionTitle);
            Assert.Contains("# Encabezado 1", help.ViewModel.Content, StringComparison.Ordinal);

            Assert.True(viewModel.TryChangeLanguage("en"));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // BR-013/COM-002: las superficies SC-001 y SC-003 se relabelizan
            // en caliente sin cerrar la ventana de ayuda.
            Assert.Equal("Help — Egg MD Editor", help.Title);
            Assert.Equal("Markdown guide", help.ViewModel.ActiveSectionTitle);
            Assert.Contains("# Heading 1", help.ViewModel.Content, StringComparison.Ordinal);
            Assert.Equal(
                "Markdown guide",
                Assert.IsType<HelpSectionDefinition>(sectionList.SelectedItem).Title);
            Assert.Same(help, window.HelpWindow);

            help.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HelpMenuEntriesRelabelOnLanguageSwitch()
    {
        var catalog = new I18nCatalog();
        var viewModel = new MainWindowViewModel(catalog);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var about = Assert.IsType<Button>(window.FindControl<Button>("HelpAboutItem"));
            Assert.Equal("Acerca de Egg MD Editor", about.Content as string);

            Assert.True(viewModel.TryChangeLanguage("en"));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("About Egg MD Editor", about.Content as string);
            Assert.Equal("Markdown guide", Assert.IsType<Button>(window.FindControl<Button>("HelpGuideItem")).Content as string);
            Assert.Equal("Keyboard shortcuts", Assert.IsType<Button>(window.FindControl<Button>("HelpShortcutsItem")).Content as string);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ClosingMainWindowClosesTheHelpSurface()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var help = window.OpenHelp(HelpSection.About);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.True(help.IsVisible);

        window.Close();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.False(help.IsVisible);
        Assert.Null(window.HelpWindow);
    }

    [AvaloniaFact]
    public void HelpSurfaceFollowsTheActiveTheme()
    {
        var application = Assert.IsType<App>(Application.Current);
        var viewModel = new HelpWindowViewModel(null, new I18nCatalog());
        var window = new HelpWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var navigation = Assert.IsType<Border>(window.FindControl<Border>("HelpNavigation"));
            var light = Assert.IsType<SolidColorBrush>(navigation.Background).Color;

            // OP-002/COM-003: la superficie de ayuda repinta con el tema
            // activo (TK-001).
            application.RequestedThemeVariant = AvaloniaThemeVariant.Dark;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var dark = Assert.IsType<SolidColorBrush>(navigation.Background).Color;
            Assert.NotEqual(light, dark);
        }
        finally
        {
            application.RequestedThemeVariant = AvaloniaThemeVariant.Default;
            window.Close();
        }
    }
}
