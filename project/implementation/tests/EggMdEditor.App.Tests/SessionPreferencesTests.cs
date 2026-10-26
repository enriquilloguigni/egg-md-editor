using EggMdEditor.App;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class SessionPreferencesTests
{
    [Fact]
    public void Session_Defaults()
    {
        var s = new Session();
        Assert.Equal("es", s.Language);
        Assert.Equal(Theme.Light, s.Theme);
        Assert.Equal(MenuState.Closed, s.MenuState);
        Assert.False(s.NoticeVisible);
        Assert.True(s.WindowOpen);
        Assert.NotNull(s.Preferences);
        Assert.Equal("es", s.Preferences.Language);
        Assert.Equal(Theme.Light, s.Preferences.Theme);
    }

    [Fact]
    public void UiPreferences_Defaults()
    {
        var p = new UiPreferences();
        Assert.Equal("es", p.Language);
        Assert.Equal(Theme.Light, p.Theme);
    }

    [Fact]
    public void Session_SetLanguage_UpdatesPreferencesAndCatalog()
    {
        var catalog = new I18nCatalog();
        var s = new Session(catalog);
        Assert.True(s.TrySetLanguage("en"));
        Assert.Equal("en", s.Language);
        Assert.Equal("en", s.Preferences.Language);
        Assert.Equal("en", catalog.CurrentLanguage);
        Assert.False(s.TrySetLanguage("fr"));
        Assert.Equal("en", s.Language);
    }

    [Fact]
    public void Session_SetTheme_UpdatesPreferences()
    {
        var s = new Session();
        s.Theme = Theme.Dark;
        Assert.Equal(Theme.Dark, s.Theme);
        Assert.Equal(Theme.Dark, s.Preferences.Theme);
        Assert.True(s.TrySetTheme("light"));
        Assert.Equal(Theme.Light, s.Theme);
    }

    [Fact]
    public void Session_MenuAndNotice()
    {
        var s = new Session();
        s.OpenMenu();
        Assert.True(s.IsMenuOpen);
        Assert.Equal(MenuState.Open, s.MenuState);
        s.CloseMenu();
        Assert.False(s.IsMenuOpen);
        s.ShowNotice();
        Assert.True(s.NoticeVisible);
        Assert.Equal(NoticeState.Visible, s.NoticeState);
        s.HideNotice();
        Assert.False(s.NoticeVisible);
    }

    [Fact]
    public void Session_PreferencesTwoWay()
    {
        var s = new Session();
        s.Preferences.Language = "en";
        Assert.Equal("en", s.Language);
        s.Preferences.Theme = Theme.Dark;
        Assert.Equal(Theme.Dark, s.Theme);
    }

    [Fact]
    public void Session_PreferencesSetter()
    {
        var s = new Session();
        s.Preferences = new UiPreferences("en", Theme.Dark);
        Assert.Equal("en", s.Language);
        Assert.Equal(Theme.Dark, s.Theme);
    }

    [Fact]
    public void Singleton_Instance()
    {
        Assert.Same(Session.Instance, Session.Instance);
    }

    [Fact]
    public void Clone_Preserves()
    {
        var s = new Session();
        s.Language = "en";
        s.Theme = Theme.Dark;
        s.OpenMenu();
        s.ShowNotice();
        var c = s.Clone();
        Assert.Equal("en", c.Language);
        Assert.Equal(Theme.Dark, c.Theme);
        Assert.Equal(MenuState.Open, c.MenuState);
        Assert.True(c.NoticeVisible);
    }

    [Fact]
    public void UiPreferences_ThemeSetterRaises()
    {
        var p = new UiPreferences();
        var raised = false;
        p.PropertyChanged += (o, e) => { if (e.PropertyName == nameof(UiPreferences.Theme)) raised = true; };
        p.Theme = Theme.Dark;
        Assert.True(raised);
        Assert.Equal(Theme.Dark, p.Theme);
    }

    [Fact]
    public void Session_CloneKeepsItsOwnPreferences()
    {
        var s = new Session(new I18nCatalog());
        s.Language = "en";
        s.Theme = Theme.Dark;

        var clone = s.Clone();
        clone.Preferences.Language = "es";
        clone.Preferences.Theme = Theme.Light;

        Assert.Equal("en", s.Language);
        Assert.Equal(Theme.Dark, s.Theme);
        Assert.Equal("es", clone.Language);
        Assert.Equal(Theme.Light, clone.Theme);
    }

    [Fact]
    public void UiPreferences_CloneKeepsLanguageThemeAndPreference()
    {
        var preferences = new UiPreferences("en", Theme.Dark, ThemePreference.Dark);
        var clone = preferences.Clone();

        Assert.Equal("en", clone.Language);
        Assert.Equal(Theme.Dark, clone.Theme);
        Assert.Equal(ThemePreference.Dark, clone.ThemePreference);

        clone.Language = "es";
        clone.Theme = Theme.Light;

        Assert.Equal("en", preferences.Language);
        Assert.Equal(Theme.Dark, preferences.Theme);
        Assert.Equal(ThemePreference.Dark, preferences.ThemePreference);
    }

    [Fact]
    public void Session_Dispose()
    {
        var s = new Session();
        s.Dispose();
        // no throw
    }
}
