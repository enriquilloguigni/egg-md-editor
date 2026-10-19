using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Wiring of COM-001 in SC-001 (T-011): hot relabeling from the language
// switching service's single notification (T-004), with every string
// re-resolved from the active language already applied to the session —the
// observable source— and with no restart (DS-003, AC-007, AC-008, NFR-006),
// language selection check repainted from the state (T-010) and session
// state —menu and tooltip— intact (BR-F001-005). The aborted switching
// (ERR-003) does not notify: labels and check remain in the previous
// language (AC-010).
public class RelabelingWiringTests
{
    [Fact]
    public void The_switching_hot_relabels_the_menu_labels()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        window.LanguageService.Switch(InterfaceLanguage.English);

        var bar = MenuBarOf(window);
        Assert.Equal("File", HeaderOf(bar, "menu.archivo"));
        Assert.Equal("Edit", HeaderOf(bar, "menu.edicion"));
        Assert.Equal("New", HeaderOf(bar, "menu.archivo.nuevo"));
        Assert.Equal("Interface language", HeaderOf(bar, MainMenuBuilder.LanguageEntryKey));
    }

    [Fact]
    public void The_switching_hot_relabels_the_format_bar_tooltips()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        window.LanguageService.Switch(InterfaceLanguage.English);

        Assert.Equal("Emphasis", ToolTipOf(window, 0));
        Assert.Equal("Table of contents", ToolTipOf(window, 11));
    }

    [Fact]
    public void The_switching_rechecks_the_language_selection_from_the_state()
    {
        // Path by shortcut (without pressing the menu): the check follows the
        // active language because the relabeling repaints it from the session.
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        window.LanguageService.Switch(InterfaceLanguage.English);

        var (spanish, english) = LanguageValuesOf(window);
        Assert.False(spanish.IsChecked);
        Assert.True(english.IsChecked);

        window.LanguageService.Switch(InterfaceLanguage.Spanish);

        Assert.True(spanish.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void The_switching_keeps_the_session_state()
    {
        // AC-007: a menu can be expanded and a tooltip visible when
        // switching; the relabeling only touches texts and checks
        // (BR-F001-005).
        var session = new ApplicationSession
        {
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var window = new MainWindow(session);

        window.LanguageService.Switch(InterfaceLanguage.English);

        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
    }

    [Fact]
    public void The_aborted_switching_keeps_the_previous_labels_check_and_language()
    {
        // AC-010 (ERR-003): the target language's catalog does not load;
        // without a notification there is no relabeling and the whole UI
        // remains as it was.
        var session = new ApplicationSession();
        var brokenProvider = new BrokenProvider();
        var window = new MainWindow(
            session,
            new LanguageSwitchingService(session, brokenProvider));

        var result = window.LanguageService.Switch(InterfaceLanguage.English);

        Assert.Equal(LanguageSwitchState.AbortedCatalogUnavailable, result.State);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal("Archivo", HeaderOf(MenuBarOf(window), "menu.archivo"));
        Assert.Equal("Énfasis", ToolTipOf(window, 0));

        var (spanish, english) = LanguageValuesOf(window);
        Assert.True(spanish.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void Pressing_the_english_menu_value_switches_and_relabels()
    {
        // COM-001 from the language entry (T-010): pressing the value
        // switches and the single notification (DS-003) relabels.
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        LanguageValueOf(window, InterfaceLanguage.English)
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal("File", HeaderOf(MenuBarOf(window), "menu.archivo"));

        var (spanish, english) = LanguageValuesOf(window);
        Assert.False(spanish.IsChecked);
        Assert.True(english.IsChecked);
    }

    [Fact]
    public void Pressing_the_already_active_value_leaves_the_check_and_labels_intact()
    {
        // Null operation (BR-F001-004): without switching, without
        // notification and with the check restored from the state (only one
        // checked).
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        var notifications = new List<LanguageSwitchedEventArgs>();
        window.LanguageService.LanguageSwitched += (_, e) => notifications.Add(e);

        LanguageValueOf(window, InterfaceLanguage.Spanish)
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Empty(notifications);
        Assert.Equal("Archivo", HeaderOf(MenuBarOf(window), "menu.archivo"));

        var (spanish, english) = LanguageValuesOf(window);
        Assert.True(spanish.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void Pressing_a_value_with_aborted_switching_restores_the_active_language_check()
    {
        // ERR-003 from the language entry: pressing leaves no partial
        // change; the check returns to the active language even if the theme
        // or the pressing had moved it (AC-010).
        var session = new ApplicationSession();
        var window = new MainWindow(
            session,
            new LanguageSwitchingService(session, new BrokenProvider()));

        var english = LanguageValueOf(window, InterfaceLanguage.English);
        english.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal("Archivo", HeaderOf(MenuBarOf(window), "menu.archivo"));

        var (spanish, englishAfterAbort) = LanguageValuesOf(window);
        Assert.True(spanish.IsChecked);
        Assert.False(englishAfterAbort.IsChecked);
    }

    // --- Helpers ---

    private static Menu MenuBarOf(MainWindow window) =>
        window.FindControl<Menu>("MenuBar") ?? throw new InvalidOperationException("SC-001 without menu bar.");

    private static string HeaderOf(Menu bar, string key) =>
        EnumerateItems(bar).Single(item => (string?)item.Tag == key).Header as string
        ?? throw new InvalidOperationException($"Item {key} has no label.");

    private static MenuItem LanguageValueOf(MainWindow window, InterfaceLanguage language) =>
        EnumerateItems(MenuBarOf(window))
            .Single(item => (string?)item.Tag == MainMenuBuilder.LanguageValueKey(language));

    private static (MenuItem Spanish, MenuItem English) LanguageValuesOf(MainWindow window) =>
        (LanguageValueOf(window, InterfaceLanguage.Spanish), LanguageValueOf(window, InterfaceLanguage.English));

    private static IEnumerable<MenuItem> EnumerateItems(Menu bar) =>
        bar.Items.OfType<MenuItem>().SelectMany(EnumerateItems);

    private static IEnumerable<MenuItem> EnumerateItems(MenuItem item)
    {
        yield return item;

        foreach (var child in item.Items.OfType<MenuItem>())
        {
            foreach (var grandChild in EnumerateItems(child))
            {
                yield return grandChild;
            }
        }
    }

    private static string ToolTipOf(MainWindow window, int index) =>
        ToolTip.GetTip(FormatBarButtonAt(window, index)) as string
        ?? throw new InvalidOperationException("The bar control has no tooltip.");

    private static Button FormatBarButtonAt(MainWindow window, int index) =>
        window.FindControl<StackPanel>("FormatBar")!.Children.OfType<Button>().ElementAt(index);

    /// <summary>Provider whose catalog never loads: causes the ERR-003 abort.</summary>
    private sealed class BrokenProvider : II18nTextProvider
    {
        public bool TryLoad(string languageCode, out IReadOnlyDictionary<string, string> texts)
        {
            texts = new Dictionary<string, string>();
            return false;
        }
    }
}
