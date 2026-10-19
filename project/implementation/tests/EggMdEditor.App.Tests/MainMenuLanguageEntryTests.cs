using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Tools → Interface language entry of SC-001 (T-010): submenu with the
// Español | English values of the language catalog, with mutually exclusive
// selection (always one checked, and it is the session's active language
// one: AC-001, AC-007, AC-008) and with no business action connected
// (ADR-001); the switching with hot relabeling arrives wired in T-011
// (COM-001). The FR-011 count does not change: the values are the selection
// values of one entry, not new entries (DS-001).
public class MainMenuLanguageEntryTests
{
    [Fact]
    public void The_interface_language_entry_deploys_the_catalog_values_in_order()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, StartupSession());

        var entry = InterfaceLanguageEntry(bar);

        Assert.Equal(
            LanguageCatalog.Values.Select(MainMenuBuilder.LanguageValueKey),
            entry.Items.Cast<MenuItem>().Select(value => value.Tag as string));
    }

    [Fact]
    public void At_startup_in_spanish_only_spanish_stays_checked()
    {
        // AC-001: the interface starts in Spanish, with Tools → Interface
        // language checked at Español (BR-F001-004).
        var bar = new Menu();

        MainMenuBuilder.Build(bar, StartupSession());

        var (spanish, english) = LanguageValuesOf(bar);

        Assert.True(spanish.IsChecked);
        Assert.False(english.IsChecked);
    }

    [Fact]
    public void In_an_english_session_only_english_stays_checked()
    {
        var session = StartupSession();
        session.ActiveLanguage = InterfaceLanguage.English;

        var bar = new Menu();
        MainMenuBuilder.Build(bar, session);

        var (spanish, english) = LanguageValuesOf(bar);

        Assert.False(spanish.IsChecked);
        Assert.True(english.IsChecked);
    }

    [Fact]
    public void The_selection_is_mutually_exclusive_always_one_checked()
    {
        foreach (var language in LanguageCatalog.Values)
        {
            var session = StartupSession();
            session.ActiveLanguage = language;

            var bar = new Menu();
            MainMenuBuilder.Build(bar, session);

            var checkedCount = InterfaceLanguageEntry(bar).Items
                .Cast<MenuItem>()
                .Count(value => value.IsChecked);

            Assert.Equal(1, checkedCount);
        }
    }

    [Fact]
    public void The_language_values_are_radio_of_the_same_group()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, StartupSession());

        var (spanish, english) = LanguageValuesOf(bar);

        // Runtime mutual exclusion (pure visual state, no business): both
        // values are radio and share a group, with the check painted by the
        // theme for :checked:radio.
        Assert.Equal(MenuItemToggleType.Radio, spanish.ToggleType);
        Assert.Equal(MenuItemToggleType.Radio, english.ToggleType);
        Assert.Equal(spanish.GroupName, english.GroupName);
        Assert.False(string.IsNullOrEmpty(spanish.GroupName));
    }

    [Fact]
    public void The_value_labels_are_resolved_in_the_active_language()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, StartupSession());

        var (spanish, english) = LanguageValuesOf(bar);

        // Proper names of each language, the same in es and en (SC-001).
        Assert.Equal("Español", spanish.Header as string);
        Assert.Equal("English", english.Header as string);
        foreach (var value in new[] { spanish, english })
        {
            Assert.Equal(
                I18nStringCatalog.Resolve((string)value.Tag!, InterfaceLanguage.English),
                value.Header as string);
        }
    }

    [Fact]
    public void The_entry_and_its_values_remain_visible_with_no_connected_action()
    {
        // ADR-001: appearance of a connected entry, with no disabled marks
        // and no command; pressing them does not alter the document.
        var bar = new Menu();

        MainMenuBuilder.Build(bar, StartupSession());

        var entry = InterfaceLanguageEntry(bar);

        Assert.True(entry.IsEnabled);
        Assert.Null(entry.Command);
        Assert.All(entry.Items.Cast<MenuItem>(), value =>
        {
            Assert.True(value.IsEnabled);
            Assert.Null(value.Command);
        });
    }

    private static ApplicationSession StartupSession() => new();

    private static MenuItem InterfaceLanguageEntry(Menu bar) =>
        bar.Items.Cast<MenuItem>()
            .SelectMany(menu => menu.Items.Cast<MenuItem>())
            .Single(entry => (string?)entry.Tag == MainMenuBuilder.LanguageEntryKey);

    private static (MenuItem Spanish, MenuItem English) LanguageValuesOf(Menu bar)
    {
        var values = InterfaceLanguageEntry(bar).Items.Cast<MenuItem>().ToList();

        Assert.Equal(2, values.Count);

        return (values[0], values[1]);
    }
}
