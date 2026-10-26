using System;
using System.ComponentModel;

namespace EggMdEditor.App;

/// <summary>
/// Preferencias de UI en memoria (ENT-003): idioma y tema. Sin persistencia en F-001.
/// </summary>
public class UiPreferences : INotifyPropertyChanged, IUiPreferences, IDisposable
{
    private string _language;
    private Theme _theme;
    private ThemePreference _themePreference;

    public UiPreferences()
        : this("es", Theme.Light, ThemePreference.System)
    {
    }

    public UiPreferences(string language, Theme theme)
        : this(language, theme, ThemePreferenceFor(theme))
    {
    }

    /// <summary>
    /// Preferencias con preferencia de tema explícita (ENT-003, OP-002). El
    /// tema efectivo inicial puede venir resuelto del SO cuando la preferencia
    /// es <see cref="ThemePreference.System"/>.
    /// </summary>
    public UiPreferences(string language, Theme theme, ThemePreference themePreference)
    {
        _language = NormalizeLanguage(language);
        _theme = NormalizeTheme(theme);
        _themePreference = NormalizeThemePreference(themePreference);

        if (_themePreference is ThemePreference.Light or ThemePreference.Dark)
        {
            _theme = EffectiveTheme(_themePreference, _theme);
        }
    }

    public UiPreferences(Theme theme)
        : this("es", theme)
    {
    }

    public UiPreferences(string language)
        : this(language, Theme.Light, ThemePreference.System)
    {
    }

    public string Language
    {
        get => _language;
        set
        {
            if (TrySetLanguage(value))
            {
                return;
            }
            // keep previous on failure (abort without partial change)
        }
    }

    public string Idioma
    {
        get => _language;
        set => Language = value;
    }

    public Theme Tema
    {
        get => _theme;
        set => Theme = value;
    }

    public string CurrentLanguage
    {
        get => _language;
        set => Language = value;
    }

    public string ActiveLanguage
    {
        get => _language;
        set => Language = value;
    }

    public string LanguageCode
    {
        get => _language;
        set => Language = value;
    }

    public Theme Theme
    {
        get => _theme;
        set => TrySetTheme(value);
    }

    public Theme CurrentTheme
    {
        get => _theme;
        set => Theme = value;
    }

    public Theme ActiveTheme
    {
        get => _theme;
        set => Theme = value;
    }

    /// <summary>
    /// Preferencia de ENT-003 {Claro,Oscuro,Sistema}; Sistema es el valor por
    /// defecto y no altera por sí solo el tema efectivo (se resuelve del SO).
    /// </summary>
    public ThemePreference ThemePreference
    {
        get => _themePreference;
        set => TrySetThemePreference(value);
    }

    public ThemePreference CurrentThemePreference
    {
        get => _themePreference;
        set => ThemePreference = value;
    }

    public ThemePreference TemaPreferencia
    {
        get => _themePreference;
        set => ThemePreference = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool TrySetLanguage(string? language)
    {
        var code = NormalizeLanguage(language);
        if (code is not ("es" or "en"))
        {
            return false;
        }

        if (string.Equals(_language, code, StringComparison.Ordinal))
        {
            return true;
        }

        _language = code;
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(ActiveLanguage));
        OnPropertyChanged(nameof(LanguageCode));
        return true;
    }

    public bool TrySetLanguage(Language language) => TrySetLanguage(ToCode(language));

    public bool TrySetLanguage(AppLanguage language) => TrySetLanguage(language == AppLanguage.English || language == AppLanguage.En ? "en" : "es");
    public bool TrySetLanguage(UiLanguage language) => TrySetLanguage(language == UiLanguage.English || language == UiLanguage.En ? "en" : "es");

    public void SetLanguage(AppLanguage language) => SetLanguage(language == AppLanguage.English || language == AppLanguage.En ? "en" : "es");
    public void SetLanguage(UiLanguage language) => SetLanguage(language == UiLanguage.English || language == UiLanguage.En ? "en" : "es");

    public void ChangeLanguage(AppLanguage language) => SetLanguage(language);
    public void ChangeLanguage(UiLanguage language) => SetLanguage(language);

    public void SwitchLanguage(AppLanguage language) => SetLanguage(language);
    public void SwitchLanguage(UiLanguage language) => SetLanguage(language);

    public void SetLanguage(string language)
    {
        if (!TrySetLanguage(language))
        {
            throw new InvalidOperationException($"Idioma no soportado: '{language}'.");
        }
    }

    public void SetLanguage(Language language) => SetLanguage(ToCode(language));

    public void ChangeLanguage(string language) => SetLanguage(language);
    public void ChangeLanguage(Language language) => SetLanguage(language);

    public void SwitchLanguage(string language) => SetLanguage(language);
    public void SwitchLanguage(Language language) => SetLanguage(language);

    public bool TrySetTheme(Theme theme)
    {
        // OP-002: tema efectivo {Claro,Oscuro}; un valor fuera de ese
        // catálogo se rechaza sin cambio parcial (ENT-003, integridad).
        if (theme is not (Theme.Light or Theme.Dark))
        {
            return false;
        }

        return TrySetThemePreference(ThemePreferenceFor(theme), theme);
    }

    /// <summary>
    /// OP-002: fija la preferencia de tema {Claro,Oscuro,Sistema}. Para
    /// Sistema se usa el tema efectivo actual como resolución del SO; el
    /// ViewModel es quien resuelve el SO y llama a la sobrecarga completa.
    /// </summary>
    public bool TrySetThemePreference(ThemePreference preference) =>
        TrySetThemePreference(preference, _theme);

    /// <summary>
    /// OP-002: fija preferencia y tema efectivo de forma atómica. Sistema
    /// resuelve al tema del SO recibido en <paramref name="systemTheme"/>.
    /// </summary>
    public bool TrySetThemePreference(ThemePreference preference, Theme systemTheme)
    {
        if (!IsSupportedThemePreference(preference))
        {
            return false;
        }

        var effective = EffectiveTheme(preference, systemTheme);
        var preferenceChanged = _themePreference != preference;
        var themeChanged = _theme != effective;

        if (!preferenceChanged && !themeChanged)
        {
            return true;
        }

        _themePreference = preference;
        _theme = effective;

        if (themeChanged)
        {
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(CurrentTheme));
            OnPropertyChanged(nameof(ActiveTheme));
            OnPropertyChanged(nameof(Tema));
        }

        if (preferenceChanged)
        {
            OnPropertyChanged(nameof(ThemePreference));
            OnPropertyChanged(nameof(CurrentThemePreference));
            OnPropertyChanged(nameof(TemaPreferencia));
        }

        return true;
    }

    public bool TrySetThemePreference(string? preference)
    {
        return TryParseThemePreference(preference, out var parsed)
            && TrySetThemePreference(parsed);
    }

    public bool TrySetTheme(AppTheme theme) => TrySetTheme((Theme)theme);
    public bool TrySetTheme(ThemeMode theme) => TrySetTheme((Theme)theme);
    public bool TrySetTheme(ThemeVariant theme) => TrySetTheme((Theme)theme);

    public void SetTheme(AppTheme theme) => SetTheme((Theme)theme);
    public void SetTheme(ThemeMode theme) => SetTheme((Theme)theme);
    public void SetTheme(ThemeVariant theme) => SetTheme((Theme)theme);

    public void ChangeTheme(AppTheme theme) => SetTheme(theme);
    public void ChangeTheme(ThemeMode theme) => SetTheme(theme);
    public void ChangeTheme(ThemeVariant theme) => SetTheme(theme);

    public void SwitchTheme(AppTheme theme) => SetTheme(theme);
    public void SwitchTheme(ThemeMode theme) => SetTheme(theme);
    public void SwitchTheme(ThemeVariant theme) => SetTheme(theme);

    public bool TrySetTheme(string? theme)
    {
        if (!TryParseThemePreference(theme, out var parsed))
        {
            return false;
        }

        return TrySetThemePreference(parsed);
    }

    public void SetTheme(Theme theme)
    {
        if (!TrySetTheme(theme))
        {
            throw new InvalidOperationException($"Tema no soportado: '{theme}'.");
        }
    }

    public void SetTheme(string theme)
    {
        if (!TrySetTheme(theme))
        {
            throw new InvalidOperationException($"Tema no soportado: '{theme}'.");
        }
    }

    public void ChangeTheme(Theme theme) => SetTheme(theme);
    public void ChangeTheme(string theme) => SetTheme(theme);

    public void SwitchTheme(Theme theme) => SetTheme(theme);
    public void SwitchTheme(string theme) => SetTheme(theme);

    public void ToggleTheme()
    {
        SetTheme(_theme == Theme.Light ? Theme.Dark : Theme.Light);
    }

    public UiPreferences Clone() => new UiPreferences(_language, _theme, _themePreference);

    public void Dispose() { }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Normaliza el tema efectivo al catálogo de ENT-001/ENT-003
    /// {Claro,Oscuro}. El valor por defecto heredado del SO se resuelve en la
    /// composición de la aplicación (App.axaml.cs), no aquí.
    /// </summary>
    public static Theme NormalizeTheme(Theme theme) =>
        theme == Theme.Dark ? Theme.Dark : Theme.Light;

    /// <summary>Catálogo de preferencia de tema de ENT-003 (OP-002).</summary>
    public static bool IsSupportedThemePreference(ThemePreference preference) =>
        preference is ThemePreference.Light
            or ThemePreference.Dark
            or ThemePreference.System;

    /// <summary>Preferencia concreta correspondiente a un tema efectivo.</summary>
    public static ThemePreference ThemePreferenceFor(Theme theme) =>
        theme == Theme.Dark ? ThemePreference.Dark : ThemePreference.Light;

    /// <summary>
    /// Tema efectivo de ENT-001/ST-009/ST-010 para una preferencia de
    /// ENT-003: Claro/Oscuro son directos y Sistema se resuelve del SO.
    /// </summary>
    public static Theme EffectiveTheme(ThemePreference preference, Theme systemTheme) =>
        preference switch
        {
            ThemePreference.Light => Theme.Light,
            ThemePreference.Dark => Theme.Dark,
            _ => NormalizeTheme(systemTheme),
        };

    /// <summary>
    /// Normaliza una preferencia no soportada al valor por defecto de
    /// ENT-003: Sistema.
    /// </summary>
    public static ThemePreference NormalizeThemePreference(ThemePreference preference) =>
        IsSupportedThemePreference(preference)
            ? preference
            : ThemePreference.System;

    public static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "es";
        }

        var trimmed = language.Trim().ToLowerInvariant();
        if (trimmed.StartsWith("es", StringComparison.Ordinal))
        {
            return "es";
        }

        if (trimmed.StartsWith("en", StringComparison.Ordinal))
        {
            return "en";
        }

        if (trimmed is "spanish" or "español")
        {
            return "es";
        }

        if (trimmed is "english" or "inglés" or "ingles")
        {
            return "en";
        }

        return trimmed.Length >= 2 ? trimmed.Substring(0, 2) : trimmed;
    }

    public static bool TryParseTheme(string? value, out Theme theme)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            theme = Theme.Light;
            return false;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        switch (trimmed)
        {
            case "light":
            case "claro":
            case "l":
                theme = Theme.Light;
                return true;
            case "dark":
            case "oscuro":
            case "d":
                theme = Theme.Dark;
                return true;
            default:
                theme = Theme.Light;
                return false;
        }
    }

    /// <summary>
    /// Interpreta la entrada de OP-002 {Claro,Oscuro,Sistema}; admite los
    /// nombres en español e inglés del catálogo de ENT-003.
    /// </summary>
    public static bool TryParseThemePreference(
        string? value,
        out ThemePreference preference)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            preference = ThemePreference.System;
            return false;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "light":
            case "claro":
            case "l":
                preference = ThemePreference.Light;
                return true;
            case "dark":
            case "oscuro":
            case "d":
                preference = ThemePreference.Dark;
                return true;
            case "system":
            case "sistema":
            case "auto":
            case "default":
            case "s":
                preference = ThemePreference.System;
                return true;
            default:
                preference = ThemePreference.System;
                return false;
        }
    }

    public static string ToCode(Language language) => language switch
    {
        EggMdEditor.App.Language.English or EggMdEditor.App.Language.En or EggMdEditor.App.Language.Ingles or EggMdEditor.App.Language.Inglés => "en",
        _ => "es",
    };

    public static Theme FromString(string? value)
    {
        return TryParseTheme(value, out var t) ? t : Theme.Light;
    }
}

public interface IUiPreferences
{
    string Language { get; set; }
    string Idioma { get; set; }
    string CurrentLanguage { get; set; }
    string ActiveLanguage { get; set; }
    string LanguageCode { get; set; }
    Theme Theme { get; set; }
    Theme Tema { get; set; }
    Theme CurrentTheme { get; set; }
    Theme ActiveTheme { get; set; }
    ThemePreference ThemePreference { get; set; }
    ThemePreference CurrentThemePreference { get; set; }
    ThemePreference TemaPreferencia { get; set; }
    bool TrySetLanguage(string? language);
    bool TrySetLanguage(Language language);
    void SetLanguage(string language);
    void SetLanguage(Language language);
    bool TrySetTheme(Theme theme);
    bool TrySetTheme(string? theme);
    bool TrySetThemePreference(ThemePreference preference);
    bool TrySetThemePreference(ThemePreference preference, Theme systemTheme);
    bool TrySetThemePreference(string? preference);
    void SetTheme(Theme theme);
    void SetTheme(string theme);
    void ToggleTheme();
}
