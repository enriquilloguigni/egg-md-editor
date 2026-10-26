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
    public void Sessions_AreIndependent()
    {
        var first = new Session(new I18nCatalog());
        var second = new Session(new I18nCatalog());

        first.Language = "en";
        first.Theme = Theme.Dark;
        first.ShowNotice();

        // T-019: sin singleton ni Clone, cada sesión conserva su propio estado.
        Assert.Equal("es", second.Language);
        Assert.Equal(Theme.Light, second.Theme);
        Assert.False(second.NoticeVisible);
        Assert.NotSame(first.Preferences, second.Preferences);
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
    public void Session_Dispose()
    {
        var s = new Session();
        s.Dispose();
        // no throw
    }
}
