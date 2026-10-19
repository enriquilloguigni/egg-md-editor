using EggMdEditor.App.Catalogs;
using Xunit;

namespace EggMdEditor.App.Tests;

// Structure of the session catalogs (DS-001): language, menu state and
// tooltip, with their controlled values and startup defaults.
public class SessionCatalogsTests
{
    [Fact]
    public void Language_catalog_has_exactly_the_DS001_values()
    {
        Assert.Equal(
            [InterfaceLanguage.Spanish, InterfaceLanguage.English],
            LanguageCatalog.Values);
    }

    [Fact]
    public void Language_catalog_default_is_spanish()
    {
        Assert.Equal(InterfaceLanguage.Spanish, LanguageCatalog.Default);
    }

    [Fact]
    public void Main_menu_state_catalog_has_exactly_the_DS001_values()
    {
        Assert.Equal(
            [MainMenuState.Closed, MainMenuState.ExpandedWithActiveEntry],
            MainMenuStateCatalog.Values);
    }

    [Fact]
    public void Main_menu_state_catalog_default_is_closed()
    {
        Assert.Equal(MainMenuState.Closed, MainMenuStateCatalog.Default);
    }

    [Fact]
    public void Session_starts_with_the_DS001_startup_defaults()
    {
        var session = new ApplicationSession();

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal(MainMenuState.Closed, session.MainMenuState);
        Assert.False(session.IsToolTipVisible);
    }

    [Fact]
    public void Session_languages_are_mutually_exclusive_distinct_values()
    {
        Assert.NotEqual(InterfaceLanguage.Spanish, InterfaceLanguage.English);
    }
}
