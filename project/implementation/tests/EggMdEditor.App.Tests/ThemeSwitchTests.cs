using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using EggMdEditor.App;
using EggMdEditor.App.I18n;
using Xunit;
using AvaloniaThemeVariant = Avalonia.Styling.ThemeVariant;
using PlatformThemeVariant = Avalonia.Platform.PlatformThemeVariant;

namespace EggMdEditor.App.Tests;

// Conmutación de tema claro/oscuro de SC-001 (T-009, OP-002, TR-008/TR-009,
// COM-003): el tema inicial se hereda del SO (ENT-003) y la conmutación se
// aplica de forma consistente a toda la aplicación, conservando el resto del
// estado de la sesión y abortando sin cambio parcial ante una entrada inválida.
public class ThemeSwitchTests
{
    [AvaloniaFact]
    public void SwitchingThemeAppliesToApplicationSessionAndWindow()
    {
        var application = Assert.IsType<App>(Application.Current);
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);
        using var applier = new ApplicationThemeApplier(application);
        viewModel.ThemeApplier = applier;

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            applier.Attach(session);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // ST-009: tema claro inicial; la ventana hereda el tema aplicado.
            Assert.Equal(AvaloniaThemeVariant.Light, application.RequestedThemeVariant);
            Assert.Equal(Theme.Light, session.Theme);
            Assert.Equal(Theme.Light, session.Preferences.Theme);
            Assert.Equal(Theme.Light, viewModel.CurrentTheme);
            Assert.True(viewModel.IsLightTheme);
            Assert.False(viewModel.IsDarkTheme);
            Assert.Equal(AvaloniaThemeVariant.Light, window.ActualThemeVariant);

            // TR-008: conmutar a oscuro aplica el tema a toda la aplicación.
            Assert.True(viewModel.TryChangeTheme(Theme.Dark));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(AvaloniaThemeVariant.Dark, application.RequestedThemeVariant);
            Assert.Equal(Theme.Dark, session.Theme);
            Assert.Equal(Theme.Dark, session.Preferences.Theme);
            Assert.Equal(Theme.Dark, applier.AppliedTheme);
            Assert.Equal(Theme.Dark, viewModel.CurrentTheme);
            Assert.False(viewModel.IsLightTheme);
            Assert.True(viewModel.IsDarkTheme);
            Assert.Equal(AvaloniaThemeVariant.Dark, window.ActualThemeVariant);

            // TR-009: conmutar a claro desde el comando del selector de tema.
            Assert.True(viewModel.ChangeThemeCommand.CanExecute("light"));
            viewModel.ChangeThemeCommand.Execute("light");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(AvaloniaThemeVariant.Light, application.RequestedThemeVariant);
            Assert.Equal(Theme.Light, session.Theme);
            Assert.Equal(Theme.Light, session.Preferences.Theme);
            Assert.Equal(AvaloniaThemeVariant.Light, window.ActualThemeVariant);

            // La conmutación de tema no altera el resto del estado de la sesión.
            Assert.True(session.WindowOpen);
            Assert.Equal("es", session.Language);
            Assert.Equal("es", session.Preferences.Language);
        }
        finally
        {
            applier.Detach();
            application.RequestedThemeVariant = AvaloniaThemeVariant.Default;
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ThemeChangeInSessionIsAppliedWithoutRecreatingTheWindow()
    {
        var application = Assert.IsType<App>(Application.Current);
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);
        using var applier = new ApplicationThemeApplier(application);
        viewModel.ThemeApplier = applier;

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            applier.Attach(session);

            // La preferencia (ENT-003) es la fuente de OP-002: la aplicación
            // sigue el tema de la sesión aunque el cambio no venga del VM.
            session.Preferences.Theme = Theme.Dark;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(Theme.Dark, session.Theme);
            Assert.Equal(AvaloniaThemeVariant.Dark, application.RequestedThemeVariant);
            Assert.Equal(AvaloniaThemeVariant.Dark, window.ActualThemeVariant);
            Assert.Equal(Theme.Dark, applier.AppliedTheme);

            Assert.True(session.TrySetTheme(Theme.Light));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal(AvaloniaThemeVariant.Light, application.RequestedThemeVariant);
            Assert.Equal(AvaloniaThemeVariant.Light, window.ActualThemeVariant);
            Assert.Same(viewModel, window.DataContext);
        }
        finally
        {
            applier.Detach();
            application.RequestedThemeVariant = AvaloniaThemeVariant.Default;
            window.Close();
        }
    }

    [Fact]
    public void Op002AppliesTheThemeThroughTheInjectedApplier()
    {
        var applied = new List<Theme>();
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session)
        {
            ThemeApplier = new RecordingThemeApplier(applied),
        };

        Assert.True(viewModel.TryChangeTheme("dark"));
        Assert.Equal(Theme.Dark, session.Theme);
        Assert.Equal(Theme.Dark, viewModel.CurrentTheme);
        Assert.Equal(new[] { Theme.Dark }, applied);

        Assert.True(viewModel.TryChangeTheme("claro"));
        Assert.Equal(Theme.Light, session.Theme);
        Assert.Equal(new[] { Theme.Dark, Theme.Light }, applied);

        // Entrada no soportada: ni estado ni aplicación cambian.
        Assert.False(viewModel.TryChangeTheme("purple"));
        Assert.Equal(Theme.Light, session.Theme);
        Assert.Equal(2, applied.Count);
    }

    [Fact]
    public void ThemeToggleAlternatesBetweenLightAndDark()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);

        Assert.True(viewModel.ToggleTheme());
        Assert.Equal(Theme.Dark, session.Theme);
        Assert.True(viewModel.IsDarkTheme);

        Assert.True(viewModel.ToggleTheme());
        Assert.Equal(Theme.Light, session.Theme);
        Assert.True(viewModel.IsLightTheme);
    }

    // T-016: Herramientas → Tema (ENT-003, OP-002, TR-008/TR-009,
    // BR-F001-009, AC-008): tres opciones mutuamente excluyentes, Sistema
    // por defecto heredado del SO, conmutación en caliente y estado oscuro
    // observable/capturable.
    [Fact]
    public void ThemeEntryDefaultsToSystemAndIsLocalized()
    {
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        var viewModel = new MainWindowViewModel(session);

        Assert.Equal(ThemePreference.System, session.ThemePreference);
        Assert.Equal(ThemePreference.System, session.Preferences.ThemePreference);
        Assert.Equal(ThemePreference.System, viewModel.CurrentThemePreference);
        Assert.True(viewModel.IsSystemThemeSelected);
        Assert.False(viewModel.IsLightThemeSelected);
        Assert.False(viewModel.IsDarkThemeSelected);

        Assert.Equal("Tema de la interfaz", viewModel.ThemeMenuHeader);
        Assert.Equal("Claro", viewModel.LightThemeOptionHeader);
        Assert.Equal("Oscuro", viewModel.DarkThemeOptionHeader);
        Assert.Equal("Sistema", viewModel.SystemThemeOptionHeader);

        Assert.True(viewModel.TryChangeLanguage("en"));

        Assert.Equal("Interface theme", viewModel.ThemeMenuHeader);
        Assert.Equal("Light", viewModel.LightThemeOptionHeader);
        Assert.Equal("Dark", viewModel.DarkThemeOptionHeader);
        Assert.Equal("System", viewModel.SystemThemeOptionHeader);
        Assert.True(viewModel.IsSystemThemeSelected);
    }

    [Fact]
    public void SystemPreferenceResolvesToTheOperatingSystemTheme()
    {
        var session = new Session(new I18nCatalog());
        var applied = new List<Theme>();
        var viewModel = new MainWindowViewModel(session)
        {
            ThemeApplier = new RecordingThemeApplier(applied),
            SystemThemeResolver = () => Theme.Dark,
        };

        Assert.True(viewModel.TryChangeTheme("system"));

        Assert.Equal(ThemePreference.System, session.ThemePreference);
        Assert.Equal(ThemePreference.System, session.Preferences.ThemePreference);
        Assert.Equal(Theme.Dark, session.Theme);
        Assert.Equal(Theme.Dark, session.Preferences.Theme);
        Assert.Equal(Theme.Dark, viewModel.CurrentTheme);
        Assert.True(viewModel.IsDarkTheme);
        Assert.False(viewModel.IsLightThemeSelected);
        Assert.False(viewModel.IsDarkThemeSelected);
        Assert.True(viewModel.IsSystemThemeSelected);
        Assert.Equal(new[] { Theme.Dark }, applied);

        // TR-009: con el SO en claro, Sistema vuelve a ST-009 sin cambiar la
        // preferencia, que sigue siendo Sistema.
        viewModel.SystemThemeResolver = () => Theme.Light;
        Assert.True(viewModel.TryChangeTheme(ThemePreference.System));
        Assert.Equal(Theme.Light, session.Theme);
        Assert.Equal(Theme.Light, viewModel.CurrentTheme);
        Assert.Equal(ThemePreference.System, session.ThemePreference);
        Assert.Equal(new[] { Theme.Dark, Theme.Light }, applied);
    }

    [AvaloniaFact]
    public void ThemeSelectorShowsThreeMutuallyExclusiveOptionsAndSwitchesHot()
    {
        var application = Assert.IsType<App>(Application.Current);
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session)
        {
            SystemThemeResolver = () => Theme.Dark,
        };
        var window = new MainWindow(viewModel);
        using var applier = new ApplicationThemeApplier(application);
        viewModel.ThemeApplier = applier;

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            applier.Attach(session);
            Assert.True(viewModel.OpenMenu("Menu.Tools"));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var light = Assert.IsType<RadioButton>(
                window.FindControl<RadioButton>("LightThemeItem"));
            var dark = Assert.IsType<RadioButton>(
                window.FindControl<RadioButton>("DarkThemeItem"));
            var system = Assert.IsType<RadioButton>(
                window.FindControl<RadioButton>("SystemThemeItem"));

            Assert.Equal("Claro", light.Content as string);
            Assert.Equal("Oscuro", dark.Content as string);
            Assert.Equal("Sistema", system.Content as string);
            Assert.True(light.IsVisible);
            Assert.True(dark.IsVisible);
            Assert.True(system.IsVisible);
            Assert.Equal("Theme.light", AutomationProperties.GetAutomationId(light));
            Assert.Equal("Theme.dark", AutomationProperties.GetAutomationId(dark));
            Assert.Equal("Theme.system", AutomationProperties.GetAutomationId(system));
            Assert.Equal("Claro", AutomationProperties.GetName(light));
            Assert.Equal("Oscuro", AutomationProperties.GetName(dark));
            Assert.Equal("Sistema", AutomationProperties.GetName(system));

            // El default es Sistema (heredado del SO) y la marca es única.
            Assert.False(light.IsChecked);
            Assert.False(dark.IsChecked);
            Assert.True(system.IsChecked);
            Assert.Equal(ThemePreference.System, session.ThemePreference);

            // TR-008: Claro → Oscuro en caliente; estado oscuro observable.
            viewModel.ChangeThemeCommand.Execute("dark");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(light.IsChecked);
            Assert.True(dark.IsChecked);
            Assert.False(system.IsChecked);
            Assert.Equal(Theme.Dark, session.Theme);
            Assert.Equal(ThemePreference.Dark, session.ThemePreference);
            Assert.Equal(Theme.Dark, session.Preferences.Theme);
            Assert.Equal(ThemePreference.Dark, session.Preferences.ThemePreference);
            Assert.Equal(Theme.Dark, applier.AppliedTheme);
            Assert.Equal(AvaloniaThemeVariant.Dark, application.RequestedThemeVariant);
            Assert.Equal(AvaloniaThemeVariant.Dark, window.ActualThemeVariant);
            Assert.True(viewModel.IsDarkTheme);
            Assert.True(viewModel.IsDarkThemeSelected);

            // TR-009: Oscuro → Claro en caliente.
            viewModel.ChangeThemeCommand.Execute("light");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(light.IsChecked);
            Assert.False(dark.IsChecked);
            Assert.False(system.IsChecked);
            Assert.Equal(Theme.Light, session.Theme);
            Assert.Equal(ThemePreference.Light, session.ThemePreference);
            Assert.Equal(AvaloniaThemeVariant.Light, application.RequestedThemeVariant);
            Assert.Equal(AvaloniaThemeVariant.Light, window.ActualThemeVariant);

            // TR-008 con SO en oscuro: Sistema resuelve al tema del SO.
            viewModel.ChangeThemeCommand.Execute("system");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(light.IsChecked);
            Assert.False(dark.IsChecked);
            Assert.True(system.IsChecked);
            Assert.Equal(ThemePreference.System, session.ThemePreference);
            Assert.Equal(Theme.Dark, session.Theme);
            Assert.Equal(AvaloniaThemeVariant.Dark, application.RequestedThemeVariant);
            Assert.Equal(AvaloniaThemeVariant.Dark, window.ActualThemeVariant);

            // La conmutación de tema no altera el resto del estado de SC-001.
            Assert.True(session.WindowOpen);
            Assert.Equal("es", session.Language);
            Assert.True(viewModel.IsMenuOpen);
        }
        finally
        {
            applier.Detach();
            application.RequestedThemeVariant = AvaloniaThemeVariant.Default;
            window.Close();
        }
    }

    private sealed class RecordingThemeApplier : IThemeApplier
    {
        private readonly List<Theme> _applied;

        public RecordingThemeApplier(List<Theme> applied)
        {
            _applied = applied;
        }

        public Theme AppliedTheme => _applied.Count == 0 ? Theme.Light : _applied[^1];

        public void Apply(Theme theme) => _applied.Add(theme);
    }

    [Fact]
    public void UnsupportedThemeIsRejectedWithoutPartialChange()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);

        Assert.False(viewModel.TryChangeTheme("purple"));
        Assert.False(viewModel.TryChangeTheme((string?)null));
        Assert.False(viewModel.TryChangeTheme((Theme)42));

        Assert.Equal(Theme.Light, session.Theme);
        Assert.Equal(Theme.Light, session.Preferences.Theme);
        Assert.Equal(Theme.Light, viewModel.CurrentTheme);
        Assert.True(viewModel.IsLightTheme);
    }

    [Fact]
    public void UndefinedThemeValueIsRejectedBySessionAndPreferences()
    {
        var session = new Session(new I18nCatalog());
        Assert.False(session.TrySetTheme((Theme)42));
        Assert.Equal(Theme.Light, session.Theme);
        Assert.Equal(Theme.Light, session.Preferences.Theme);

        var preferences = new UiPreferences();
        Assert.False(preferences.TrySetTheme((Theme)42));
        Assert.Equal(Theme.Light, preferences.Theme);

        // Un constructor con un valor fuera del catálogo normaliza a ST-009.
        Assert.Equal(Theme.Light, new UiPreferences("es", (Theme)42).Theme);
        Assert.Equal(
            Theme.Light,
            new Session(I18nCatalog.Instance, "es", (Theme)42, MenuState.Closed, false).Theme);
    }

    [Theory]
    [InlineData(PlatformThemeVariant.Light, Theme.Light)]
    [InlineData(PlatformThemeVariant.Dark, Theme.Dark)]
    public void InitialThemeInheritsTheOperatingSystemTheme(
        PlatformThemeVariant systemTheme,
        Theme expected)
    {
        // ENT-003: tema por defecto heredado del SO (design-system).
        Assert.Equal(expected, ApplicationThemeApplier.ResolveSystemTheme(systemTheme));
        Assert.Equal(
            expected == Theme.Dark ? AvaloniaThemeVariant.Dark : AvaloniaThemeVariant.Light,
            ApplicationThemeApplier.ToThemeVariant(expected));
        Assert.Equal(expected, ApplicationThemeApplier.FromThemeVariant(
            expected == Theme.Dark ? AvaloniaThemeVariant.Dark : AvaloniaThemeVariant.Light));
    }

    [AvaloniaFact]
    public void SystemThemeResolutionReturnsADefinedThemeForTheCurrentPlatform()
    {
        // Sin información fiable del SO se conserva el estado inicial ST-009.
        Assert.Equal(Theme.Light, ApplicationThemeApplier.ResolveSystemTheme((Avalonia.Platform.IPlatformSettings?)null));

        var resolved = ApplicationThemeApplier.ResolveSystemTheme(Application.Current);
        Assert.True(resolved is Theme.Light or Theme.Dark);
    }
}
