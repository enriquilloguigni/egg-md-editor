namespace EggMdEditor.App;

public enum Language
{
    Spanish = 0,
    English = 1,
    Es = 0,
    En = 1,
    Español = 0,
    Ingles = 1,
    Inglés = 1,
}

public enum Theme
{
    Light = 0,
    Dark = 1,
    Claro = 0,
    Oscuro = 1,
    System = 0,
    Auto = 0,
    Default = 0,
}

/// <summary>
/// Preferencia de tema de ENT-003 (OP-002): el usuario elige Claro, Oscuro o
/// Sistema. El tema efectivo de ENT-001/ST-009/ST-010 solo puede ser
/// <see cref="Light"/> o <see cref="Dark"/>: Sistema se resuelve del SO al
/// arrancar y cada vez que se selecciona.
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
    Cerrado = 0,
    Desplegado = 1,
}

public enum NoticeState
{
    Hidden = 0,
    Visible = 1,
    Oculto = 0,
}

// Aliases
public enum MenuStatus
{
    Closed = 0,
    Open = 1,
    Cerrado = 0,
    Desplegado = 1,
}

public enum NoticeVisibility
{
    Hidden = 0,
    Visible = 1,
    Oculto = 0,
}

public enum TooltipState
{
    Hidden = 0,
    Visible = 1,
    Oculto = 0,
}

public enum AppLanguage
{
    Spanish = 0,
    English = 1,
    Es = 0,
    En = 1,
}

public enum AppTheme
{
    Light = 0,
    Dark = 1,
    Claro = 0,
    Oscuro = 1,
}

public enum UiLanguage
{
    Spanish = 0,
    English = 1,
    Es = 0,
    En = 1,
}

public enum ThemeMode
{
    Light = 0,
    Dark = 1,
    Claro = 0,
    Oscuro = 1,
}

public enum ThemeVariant
{
    Light = 0,
    Dark = 1,
    Claro = 0,
    Oscuro = 1,
}
