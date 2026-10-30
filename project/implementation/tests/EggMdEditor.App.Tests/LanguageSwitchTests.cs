using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EggMdEditor.App.I18n;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EggMdEditor.App.Tests;

// Conmutación de idioma en caliente de SC-001 (T-008, OP-001,
// TR-002/TR-003): relabeliza rótulos y avisos sin reiniciar ni perder
// ventana/estado, y aborta sin cambio parcial si el catálogo destino no
// carga (ERR-003, LOG-002).
public class LanguageSwitchTests
{
    private static readonly string[] ExpectedEnglishHeaders =
    [
        "File",
        "Edit",
        "View",
        "Format",
        "Tools",
        "Help",
    ];

    private static readonly string[] ExpectedEnglishNames =
    [
        "Emphasis",
        "Headings",
        "Lists",
        "Quote",
        "Code",
        "Link",
        "Image",
        "Table",
        "Footnote",
        "Formula",
        "Diagram",
        "Table of contents",
    ];

    [AvaloniaFact]
    public void SwitchingLanguageRelabelsTheOpenWindowAndPreservesItsState()
    {
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        session.OpenMenu();
        session.ShowNotice();

        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var labels = Assert.IsType<StackPanel>(window.FindControl<StackPanel>("MenuLabels"));
            var menuButtons = labels.Children.OfType<ToggleButton>().ToList();
            Assert.Equal("Archivo", menuButtons[0].Content as string);
            Assert.True(viewModel.IsSpanish);
            Assert.False(viewModel.IsEnglish);

            Assert.True(viewModel.TryChangeLanguage("en"));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // TR-002: idioma activo, preferencia y catálogo conmutados.
            Assert.Equal("en", catalog.CurrentLanguage);
            Assert.Equal("en", session.Language);
            Assert.Equal("en", session.Preferences.Language);
            Assert.True(viewModel.IsEnglish);

            // El estado de la sesión no se pierde con el relabelizado.
            Assert.True(session.WindowOpen);
            Assert.Equal(MenuState.Open, session.MenuState);
            Assert.True(session.NoticeVisible);

            // Rótulos del menú y de la barra relabelizados en la misma ventana.
            Assert.Equal(ExpectedEnglishHeaders, menuButtons.Select(button => button.Content as string));
            Assert.Equal("Interface language", viewModel.LanguageMenuHeader);
            Assert.Equal("Previous", viewModel.PreviousMenuLabel);
            Assert.Equal("Next", viewModel.NextMenuLabel);
            Assert.True(menuButtons[0].IsChecked);

            var bar = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("FormatBar"));
            var buttons = bar.GetVisualDescendants().OfType<Button>().ToList();
            Assert.Equal(ExpectedEnglishNames, buttons.Select(AutomationProperties.GetName));
            Assert.Same(viewModel, window.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LanguageSelectorShowsExactlyOneCheckedOptionAndSwitchesHot()
    {
        var catalog = new I18nCatalog();
        var viewModel = new MainWindowViewModel(catalog);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var spanish = Assert.IsType<RadioButton>(window.FindControl<RadioButton>("SpanishLanguageItem"));
            var english = Assert.IsType<RadioButton>(window.FindControl<RadioButton>("EnglishLanguageItem"));

            Assert.Equal("Español", spanish.Content as string);
            Assert.Equal("English", english.Content as string);
            Assert.True(spanish.IsChecked);
            Assert.False(english.IsChecked);

            Assert.True(viewModel.ChangeLanguageCommand.CanExecute("en"));
            viewModel.ChangeLanguageCommand.Execute("en");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("en", catalog.CurrentLanguage);
            Assert.False(spanish.IsChecked);
            Assert.True(english.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FailedCatalogSwitchAbortsWithoutPartialChangeAndLogsError()
    {
        var spanishOnly = BuildResources(includeEnglish: false);
        var logger = new RecordingLogger();
        var catalog = new I18nCatalog(spanishOnly, "es", "es", logger);
        var session = new Session(catalog);
        session.OpenMenu();
        session.ShowNotice();

        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var labels = Assert.IsType<StackPanel>(window.FindControl<StackPanel>("MenuLabels"));
            var menuButtons = labels.Children.OfType<ToggleButton>().ToList();
            var headersBefore = menuButtons.Select(button => button.Content as string).ToArray();

            var bar = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("FormatBar"));
            var namesBefore = bar.GetVisualDescendants().OfType<Button>()
                .Select(button => AutomationProperties.GetName(button))
                .ToArray();

            var spanish = Assert.IsType<RadioButton>(window.FindControl<RadioButton>("SpanishLanguageItem"));
            var english = Assert.IsType<RadioButton>(window.FindControl<RadioButton>("EnglishLanguageItem"));
            Assert.True(spanish.IsChecked);
            Assert.False(english.IsChecked);

            // ERR-003: el idioma destino no está cargado -> aborto sin cambio.
            // El selector de radio marca la opción antes de ejecutar el comando.
            english.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
            Assert.True(english.IsChecked);
            Assert.False(spanish.IsChecked);

            viewModel.ChangeLanguageCommand.Execute("en");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.Equal("es", catalog.CurrentLanguage);
            Assert.Equal("es", session.Language);
            Assert.Equal("es", session.Preferences.Language);
            Assert.Equal(MenuState.Open, session.MenuState);
            Assert.True(session.NoticeVisible);
            Assert.Equal(headersBefore, menuButtons.Select(button => button.Content as string));
            Assert.Equal(
                namesBefore,
                bar.GetVisualDescendants().OfType<Button>()
                    .Select(button => AutomationProperties.GetName(button)));
            Assert.True(spanish.IsChecked);
            Assert.False(english.IsChecked);

            Assert.Contains(logger.Entries, entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Error);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void SuccessfulSwitchLogsDebugWhenCompleteAndWarningWhenKeysFallBack()
    {
        var completeLogger = new RecordingLogger();
        var complete = new I18nCatalog(BuildResources(includeEnglish: true), "es", "es", completeLogger);
        var completeViewModel = new MainWindowViewModel(complete) { Logger = completeLogger };

        Assert.True(completeViewModel.TryChangeLanguage("en"));
        Assert.Contains(completeLogger.Entries, entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Debug);

        var fallbackLogger = new RecordingLogger();
        var fallback = new I18nCatalog(BuildResources(includeEnglish: true, omitEnglishKey: "Menu.Help"), "es", "es", fallbackLogger);
        var fallbackViewModel = new MainWindowViewModel(fallback) { Logger = fallbackLogger };

        Assert.True(fallbackViewModel.TryChangeLanguage("en"));
        Assert.Contains(
            fallbackLogger.Entries,
            entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Warning && entry.Message.Contains("Menu.Help", StringComparison.Ordinal));
    }

    [Fact]
    public void UnsupportedLanguageIsRejectedWithoutPartialChange()
    {
        var logger = new RecordingLogger();
        var catalog = new I18nCatalog();
        var session = new Session(catalog);
        var viewModel = new MainWindowViewModel(session) { Logger = logger };

        var headersBefore = viewModel.Menus.Select(menu => menu.Header).ToArray();

        Assert.False(viewModel.TryChangeLanguage("fr"));
        Assert.Equal("es", catalog.CurrentLanguage);
        Assert.Equal("es", session.Language);
        Assert.Equal("es", session.Preferences.Language);
        Assert.Equal(headersBefore, viewModel.Menus.Select(menu => menu.Header));
        Assert.Contains(logger.Entries, entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Error);
    }

    private static Dictionary<string, IReadOnlyDictionary<string, string>> BuildResources(
        bool includeEnglish,
        string? omitEnglishKey = null)
    {
        var es = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MainWindowViewModel.LanguageMenuKey] = "Idioma de la interfaz",
            [MainWindowViewModel.SpanishLanguageKey] = "Español",
            [MainWindowViewModel.EnglishLanguageKey] = "English",
        };
        var en = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MainWindowViewModel.LanguageMenuKey] = "Interface language",
            [MainWindowViewModel.SpanishLanguageKey] = "Español",
            [MainWindowViewModel.EnglishLanguageKey] = "English",
        };

        foreach (var key in MainWindowViewModel.MenuKeys
                     .Concat(MainWindowViewModel.FormatBarKeys)
                     .Append(MainWindowViewModel.PreviousMenuKey)
                     .Append(MainWindowViewModel.NextMenuKey))
        {
            es[key] = "ES " + key;
            if (!string.Equals(key, omitEnglishKey, StringComparison.Ordinal))
            {
                en[key] = "EN " + key;
            }
        }

        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["es"] = es,
        };

        if (includeEnglish)
        {
            resources["en"] = en;
        }

        return resources;
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(Microsoft.Extensions.Logging.LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
