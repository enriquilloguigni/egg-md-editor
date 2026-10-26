namespace EggMdEditor.App;

public enum Theme
{
    Light = 0,
    Dark = 1,
}

/// <summary>
/// Preferencia de tema de ENT-003 (OP-002): el usuario elige Claro, Oscuro o
/// Sistema. El tema efectivo de ENT-001/ST-009/ST-010 solo puede ser
/// <see cref="Theme.Light"/> o <see cref="Theme.Dark"/>: Sistema se resuelve
/// del SO al arrancar y cada vez que se selecciona.
/// </summary>
public enum ThemePreference
{
    Light = 0,
    Dark = 1,
    System = 2,
}

public enum MenuState
{
    Closed = 0,
    Open = 1,
}

public enum NoticeState
{
    Hidden = 0,
    Visible = 1,
}
