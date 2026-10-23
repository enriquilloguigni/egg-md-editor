namespace EggMdEditor.App;

public interface IAppSession : ISession { }
public interface IApplicationSession : ISession { }
public interface IUserSession : ISession { }
public interface ISessionState : ISession { }
public interface IAppState : ISession { }
public interface IApplicationContext : ISession { }

public interface IPreferences : IUiPreferences { }
public interface IAppPreferences : IUiPreferences { }
public interface IUserPreferences : IUiPreferences { }
public interface IAppSettings : IUiPreferences { }
public interface IUiSettings : IUiPreferences { }
public interface ISettings : IUiPreferences { }
