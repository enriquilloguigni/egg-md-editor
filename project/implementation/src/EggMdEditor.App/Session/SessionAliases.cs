using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

public class AppSession : Session
{
    public AppSession() : base() { }
    public AppSession(II18nCatalog catalog) : base(catalog) { }
    public AppSession(UiPreferences preferences) : base(preferences) { }
    public AppSession(II18nCatalog catalog, UiPreferences preferences) : base(catalog, preferences) { }
    public AppSession(II18nCatalog catalog, string language, Theme theme, MenuState menuState, bool noticeVisible, bool windowOpen = true)
        : base(catalog, language, theme, menuState, noticeVisible, windowOpen) { }
}

public class ApplicationSession : Session
{
    public ApplicationSession() : base() { }
    public ApplicationSession(II18nCatalog catalog) : base(catalog) { }
    public ApplicationSession(UiPreferences preferences) : base(preferences) { }
    public ApplicationSession(II18nCatalog catalog, UiPreferences preferences) : base(catalog, preferences) { }
}

public class UserSession : Session
{
    public UserSession() : base() { }
    public UserSession(II18nCatalog catalog) : base(catalog) { }
    public UserSession(UiPreferences preferences) : base(preferences) { }
}

public class SessionState : Session
{
    public SessionState() : base() { }
    public SessionState(II18nCatalog catalog) : base(catalog) { }
    public SessionState(UiPreferences preferences) : base(preferences) { }
}

public class AppState : Session
{
    public AppState() : base() { }
    public AppState(II18nCatalog catalog) : base(catalog) { }
    public AppState(UiPreferences preferences) : base(preferences) { }
}

public class SessionManager : Session
{
    public SessionManager() : base() { }
    public SessionManager(II18nCatalog catalog) : base(catalog) { }
    public SessionManager(UiPreferences preferences) : base(preferences) { }
    public SessionManager(II18nCatalog catalog, UiPreferences preferences) : base(catalog, preferences) { }
}

public class SessionService : Session
{
    public SessionService() : base() { }
    public SessionService(II18nCatalog catalog) : base(catalog) { }
    public SessionService(UiPreferences preferences) : base(preferences) { }
    public SessionService(II18nCatalog catalog, UiPreferences preferences) : base(catalog, preferences) { }
}

public class ApplicationContext : Session
{
    public ApplicationContext() : base() { }
    public ApplicationContext(II18nCatalog catalog) : base(catalog) { }
    public ApplicationContext(UiPreferences preferences) : base(preferences) { }
}

public class Preferences : UiPreferences
{
    public Preferences() : base() { }
    public Preferences(string language, Theme theme) : base(language, theme) { }
    public Preferences(Theme theme) : base(theme) { }
    public Preferences(string language) : base(language) { }
}

public class AppPreferences : UiPreferences
{
    public AppPreferences() : base() { }
    public AppPreferences(string language, Theme theme) : base(language, theme) { }
    public AppPreferences(Theme theme) : base(theme) { }
    public AppPreferences(string language) : base(language) { }
}

public class UserPreferences : UiPreferences
{
    public UserPreferences() : base() { }
    public UserPreferences(string language, Theme theme) : base(language, theme) { }
    public UserPreferences(Theme theme) : base(theme) { }
    public UserPreferences(string language) : base(language) { }
}

public class AppSettings : UiPreferences
{
    public AppSettings() : base() { }
    public AppSettings(string language, Theme theme) : base(language, theme) { }
    public AppSettings(Theme theme) : base(theme) { }
    public AppSettings(string language) : base(language) { }
}

public class UiSettings : UiPreferences
{
    public UiSettings() : base() { }
    public UiSettings(string language, Theme theme) : base(language, theme) { }
    public UiSettings(Theme theme) : base(theme) { }
    public UiSettings(string language) : base(language) { }
}

public class Settings : UiPreferences
{
    public Settings() : base() { }
    public Settings(string language, Theme theme) : base(language, theme) { }
    public Settings(Theme theme) : base(theme) { }
    public Settings(string language) : base(language) { }
}

public class PreferenceSet : UiPreferences
{
    public PreferenceSet() : base() { }
    public PreferenceSet(string language, Theme theme) : base(language, theme) { }
    public PreferenceSet(Theme theme) : base(theme) { }
    public PreferenceSet(string language) : base(language) { }
}
