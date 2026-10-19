using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Building of SC-001's persistent menu bar (T-008) from the main menu
// catalog: exact count and order of FR-011 (DS-001), i18n labels resolved in
// the session's active language (Spanish at startup, BR-007) and every entry
// visible with no connected action (ADR-001).
public class MainMenuBuilderTests
{
    [Fact]
    public void Builds_the_6_menus_of_FR011_in_catalog_order()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, SpanishSession());

        Assert.Equal(
            MainMenuCatalog.Menus.Select(menu => menu.Key),
            MenuKeysOf(bar));
    }

    [Fact]
    public void Builds_the_48_entries_of_FR011_in_catalog_order()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, SpanishSession());

        var entries = EntriesOf(bar).ToList();

        Assert.Equal(MainMenuCatalog.TotalEntries, entries.Count);
        Assert.Equal(48, entries.Count);
        Assert.Equal(
            MainMenuCatalog.Menus
                .SelectMany(menu => menu.Entries.Select(entry => entry.Key)),
            entries.Select(entry => entry.Tag as string));
    }

    [Fact]
    public void Resolves_every_label_in_the_session_active_language_at_startup()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, SpanishSession());

        foreach (var menu in bar.Items.Cast<MenuItem>())
        {
            Assert.Equal(
                I18nStringCatalog.Resolve((string)menu.Tag!, InterfaceLanguage.Spanish),
                menu.Header as string);
            foreach (var entry in menu.Items.Cast<MenuItem>())
            {
                Assert.Equal(
                    I18nStringCatalog.Resolve((string)entry.Tag!, InterfaceLanguage.Spanish),
                    entry.Header as string);
            }
        }

        // Spot sample of the Spanish startup (BR-007): first menu and its
        // first entry.
        var menus = bar.Items.Cast<MenuItem>().ToList();
        Assert.Equal("Archivo", menus[0].Header as string);
        Assert.Equal("Nuevo", ((MenuItem)menus[0].Items[0]!).Header as string);
    }

    [Fact]
    public void Resolves_the_labels_in_english_when_the_session_starts_in_english()
    {
        var session = SpanishSession();
        session.ActiveLanguage = InterfaceLanguage.English;

        var bar = new Menu();
        MainMenuBuilder.Build(bar, session);

        var menus = bar.Items.Cast<MenuItem>().ToList();
        Assert.Equal("File", menus[0].Header as string);
        Assert.Equal("Edit", menus[1].Header as string);
        foreach (var menu in menus)
        {
            Assert.Equal(
                I18nStringCatalog.Resolve((string)menu.Tag!, InterfaceLanguage.English),
                menu.Header as string);
            foreach (var entry in menu.Items.Cast<MenuItem>())
            {
                Assert.Equal(
                    I18nStringCatalog.Resolve((string)entry.Tag!, InterfaceLanguage.English),
                    entry.Header as string);
            }
        }
    }

    [Fact]
    public void Leaves_every_entry_visible_with_no_connected_action()
    {
        // ADR-001: appearance of a connected entry, with no disabled marks and
        // no command: pressing them does not alter the document.
        var bar = new Menu();

        MainMenuBuilder.Build(bar, SpanishSession());

        var elements = bar.Items.Cast<MenuItem>()
            .Concat(EntriesOf(bar))
            .ToList();

        Assert.Equal(
            MainMenuCatalog.TotalMenus + MainMenuCatalog.TotalEntries,
            elements.Count);
        Assert.All(elements, element =>
        {
            Assert.True(element.IsEnabled);
            Assert.Null(element.Command);
        });
    }

    [Fact]
    public void Building_over_an_existing_bar_does_not_duplicate_menus()
    {
        var bar = new Menu();

        MainMenuBuilder.Build(bar, SpanishSession());
        MainMenuBuilder.Build(bar, SpanishSession());

        Assert.Equal(MainMenuCatalog.TotalMenus, bar.Items.Count);
        Assert.Equal(
            MainMenuCatalog.TotalEntries,
            EntriesOf(bar).Count());
    }

    private static ApplicationSession SpanishSession() => new();

    private static IEnumerable<string> MenuKeysOf(Menu bar) =>
        bar.Items.Cast<MenuItem>().Select(menu => menu.Tag as string ?? string.Empty);

    private static IEnumerable<MenuItem> EntriesOf(Menu bar) =>
        bar.Items.Cast<MenuItem>().SelectMany(menu => menu.Items.Cast<MenuItem>());
}
