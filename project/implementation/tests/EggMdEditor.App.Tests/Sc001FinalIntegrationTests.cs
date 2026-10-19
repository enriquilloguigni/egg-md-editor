using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Final integration of F-001 (T-013): the complete SC-001 —real window with
// its catalogs built and the channels COM-001–COM-004 wired (T-011)— against
// the Spec's metrics: 100 % count of FR-011/FR-12 after startup with no
// missing or extra elements (DS-001), 100 % i18n key resolution rate after
// startup and after each switching (DS-002) and complete es/en switchings in
// the same session keeping window and state, with no restarts and no partial
// changes (DS-003). It complements —without duplicating— the unit and wiring
// tests of T-001–T-012, which cover each piece separately.
public class Sc001FinalIntegrationTests
{
    // --- DS-001: 100 % count in SC-001 after startup ---

    [Fact]
    public void Sc001_starts_with_100_percent_of_FR011_and_FR012_visible_with_no_missing_nor_extra()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        var bar = MenuBarOf(window);
        var formatBar = FormatBarOf(window);

        // Persistent, visible bars from startup (AC-002, AC-003).
        Assert.True(bar.IsVisible);
        Assert.True(formatBar.IsVisible);

        // FR-011 at 100 %: the 6 menus and the 48 entries, in the catalog's
        // declared order, with no missing or extra elements (0/0 threshold).
        // The two language values of the Tools → Interface language entry
        // (T-010) are also part of the declared SC-001.
        var menus = bar.Items.OfType<MenuItem>().ToList();
        Assert.Equal(MainMenuCatalog.TotalMenus, menus.Count);
        Assert.Equal(6, menus.Count);
        Assert.Equal(
            MainMenuCatalog.Menus.Select(menu => menu.Key),
            menus.Select(menu => menu.Tag as string));

        var entries = menus.SelectMany(menu => menu.Items.OfType<MenuItem>()).ToList();
        Assert.Equal(MainMenuCatalog.TotalEntries, entries.Count);
        Assert.Equal(48, entries.Count);
        Assert.Equal(
            MainMenuCatalog.Menus.SelectMany(menu => menu.Entries.Select(entry => entry.Key)),
            entries.Select(entry => entry.Tag as string));

        var languageValues = entries.Single(
                entry => (string?)entry.Tag == MainMenuBuilder.LanguageEntryKey)
            .Items.OfType<MenuItem>().ToList();
        Assert.Equal(
            LanguageCatalog.Values.Select(MainMenuBuilder.LanguageValueKey),
            languageValues.Select(value => value.Tag as string));

        // 0 undeclared elements: every MenuItem of the window carries a key
        // of the catalogs —menus, entries or language values—.
        var declaredKeys = new HashSet<string>(
            MainMenuCatalog.Menus.Select(menu => menu.Key)
                .Concat(MainMenuCatalog.Menus.SelectMany(menu => menu.Entries.Select(entry => entry.Key)))
                .Concat(LanguageCatalog.Values.Select(MainMenuBuilder.LanguageValueKey)));
        var all = MenuItemsOf(bar).ToList();
        Assert.Equal(MainMenuCatalog.TotalMenus + MainMenuCatalog.TotalEntries + languageValues.Count, all.Count);
        Assert.All(all, element => Assert.True(
            declaredKeys.Contains((string?)element.Tag ?? string.Empty),
            $"Undeclared menu item: {element.Tag}"));

        // FR-012 at 100 %: the 12 controls in writing order, with no missing
        // or extra elements (0/0 threshold), each one visible, enabled, with
        // icon (VR-001) and tooltip (VR-002).
        var controls = formatBar.Children.OfType<Button>().ToList();
        Assert.Equal(FormatBarCatalog.TotalControls, controls.Count);
        Assert.Equal(12, controls.Count);
        Assert.Equal(
            FormatBarCatalog.Controls.Select(control => control.Key),
            controls.Select(control => control.Tag as string));
        Assert.All(controls, control =>
        {
            Assert.True(control.IsVisible);
            Assert.True(control.IsEnabled);
            Assert.False(string.IsNullOrEmpty(Assert.IsType<TextBlock>(control.Content).Text));
            Assert.False(string.IsNullOrWhiteSpace(ToolTip.GetTip(control) as string));
        });

        // Spanish startup of the single window (AC-001): active language from
        // the session default and radio check at Español, only on it.
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.True(LanguageValueOf(window, InterfaceLanguage.Spanish).IsChecked);
        Assert.False(LanguageValueOf(window, InterfaceLanguage.English).IsChecked);
    }

    // --- DS-002: 100 % resolved key rate in SC-001 ---

    [Fact]
    public void Sc001_resolves_100_percent_of_its_texts_after_startup_and_after_each_switching()
    {
        // Threshold: 0 labels or tooltips untranslated (BR-F001-007): after
        // the Spanish startup and after each es/en switching, every menu
        // label —including the language values— and every tooltip of the real
        // SC-001 are resolved, with no nulls, no empties and no leftovers of
        // the previous language (no partial change).
        var session = new ApplicationSession();
        var window = new MainWindow(session);

        foreach (var language in new[]
                 {
                     InterfaceLanguage.Spanish, // startup (BR-007)
                     InterfaceLanguage.English, // AC-007: es → en
                     InterfaceLanguage.Spanish, // AC-008: en → es
                 })
        {
            if (language != session.ActiveLanguage)
            {
                window.LanguageService.Switch(language);
            }

            Assert.All(VisibleTextsOf(window), text =>
            {
                Assert.False(string.IsNullOrWhiteSpace(text.Value));
                Assert.Equal(I18nStringCatalog.Resolve(text.Key, language), text.Value);
            });
        }
    }

    // --- DS-003: hot switching with no restart nor partial change ---

    [Fact]
    public void The_es_en_switchings_are_complete_in_the_same_session_with_no_restart_nor_partial_change()
    {
        // 100 % of the switchings completed in the same session keeping window
        // and state: two es→en→es cycles over the same window and the same
        // session, each switching effective and complete (all texts
        // re-resolved in the new language), with the menu state and the
        // tooltip intact at each jump and no restart —same session, the
        // startup one— at the end of the cycle (BR-F001-005).
        var session = new ApplicationSession
        {
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var window = new MainWindow(session);
        var startupSessionId = session.SessionId;

        for (var cycle = 0; cycle < 2; cycle++)
        {
            Assert.Equal(LanguageSwitchState.Switched, window.LanguageService.Switch(InterfaceLanguage.English).State);
            AssertWindowAndStatePreserved(window, session, InterfaceLanguage.English);

            Assert.Equal(LanguageSwitchState.Switched, window.LanguageService.Switch(InterfaceLanguage.Spanish).State);
            AssertWindowAndStatePreserved(window, session, InterfaceLanguage.Spanish);
        }

        Assert.Equal(startupSessionId, session.SessionId);
    }

    // --- AC-004, AC-005: entry and control with no action, document intact ---

    [Fact]
    public void Pressing_an_entry_and_a_control_with_no_action_does_not_alter_the_document_nor_the_state()
    {
        // ADR-001 integrated in the real SC-001: pressing an entry whose
        // action belongs to another Feature (File → New, F-028) and pressing a
        // bar control open and close with no effect on the document —it stays
        // empty— nor on the session state (AC-004, AC-005).
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        var stateBefore = (session.ActiveLanguage, session.MainMenuState, session.IsToolTipVisible);

        var newEntry = MenuItemsOf(MenuBarOf(window))
            .Single(entry => (string?)entry.Tag == "menu.archivo.nuevo");
        newEntry.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        var control = FormatBarOf(window).Children.OfType<Button>().First();
        control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(stateBefore, (session.ActiveLanguage, session.MainMenuState, session.IsToolTipVisible));
        Assert.Null(window.FindControl<Border>("DocumentArea")!.Child);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
    }

    // --- Helpers ---

    private static Menu MenuBarOf(MainWindow window) =>
        window.FindControl<Menu>("MenuBar") ?? throw new InvalidOperationException("SC-001 without menu bar.");

    private static StackPanel FormatBarOf(MainWindow window) =>
        window.FindControl<StackPanel>("FormatBar") ?? throw new InvalidOperationException("SC-001 without format bar.");

    private static MenuItem LanguageValueOf(MainWindow window, InterfaceLanguage language) =>
        MenuItemsOf(MenuBarOf(window)).Single(
            item => (string?)item.Tag == MainMenuBuilder.LanguageValueKey(language));

    private static IEnumerable<MenuItem> MenuItemsOf(Menu bar) =>
        bar.Items.OfType<MenuItem>().SelectMany(MenuItemsOf);

    private static IEnumerable<MenuItem> MenuItemsOf(MenuItem item)
    {
        yield return item;

        foreach (var child in item.Items.OfType<MenuItem>())
        {
            foreach (var element in MenuItemsOf(child))
            {
                yield return element;
            }
        }
    }

    /// <summary>The 68 visible texts of SC-001: labels of menus, entries and language values, plus the format bar tooltips.</summary>
    private static IEnumerable<(string Key, string Value)> VisibleTextsOf(MainWindow window)
    {
        foreach (var item in MenuItemsOf(MenuBarOf(window)))
        {
            yield return ((string)item.Tag!, (string?)item.Header
                ?? throw new InvalidOperationException($"Item {item.Tag} has no label."));
        }

        foreach (var control in FormatBarOf(window).Children.OfType<Button>())
        {
            yield return ((string)control.Tag!, ToolTip.GetTip(control) as string
                ?? throw new InvalidOperationException($"Control {control.Tag} has no tooltip."));
        }
    }

    /// <summary>SC-001 texts not in the given language: 0 is the DS-002 threshold and the DS-003 partial-change detector.</summary>
    private static IReadOnlyList<(string Key, string Value)> DivergentTextsOf(MainWindow window, InterfaceLanguage language) =>
        VisibleTextsOf(window)
            .Where(text => I18nStringCatalog.Resolve(text.Key, language) != text.Value)
            .ToList();

    private static void AssertWindowAndStatePreserved(
        MainWindow window, ApplicationSession session, InterfaceLanguage expectedLanguage)
    {
        Assert.Equal(expectedLanguage, session.ActiveLanguage);
        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);

        // No partial change: the 68 texts of the window are in the new
        // language, none in the previous one.
        Assert.Empty(DivergentTextsOf(window, expectedLanguage));

        // The window is still served by the same session (no restart): the
        // window's service and the session agree on the active language.
        Assert.Equal(session.ActiveLanguage, window.LanguageService.ActiveLanguage);
    }
}
