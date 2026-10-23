using System;
using System.ComponentModel;

namespace EggMdEditor.App;

/// <summary>
/// Preferencias de UI en memoria (ENT-003): idioma y tema. Sin persistencia en F-001.
/// </summary>
public class UiPreferences : INotifyPropertyChanged, IUiPreferences
{
    private string _language;
    private Theme _theme;

    public UiPreferences()
        : this("es", Theme.Light)
    {
    }

    public UiPreferences(string language, Theme theme)
    {
        _language = NormalizeLanguage(language);
        _theme = theme;
    }

    public UiPreferences(Theme theme)
        : this("es", theme)
    {
    }

    public UiPreferences(string language)
        : this(language, Theme.Light)
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
        set => _theme = value;
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
        if (_theme == theme)
        {
            return true;
        }

        _theme = theme;
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(ActiveTheme));
        return true;
    }

    public bool TrySetTheme(string? theme)
    {
        if (!TryParseTheme(theme, out var parsed))
        {
            return false;
        }

        return TrySetTheme(parsed);
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

    public UiPreferences Clone() => new UiPreferences(_language, _theme);

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

    public static string ToCode(Language language) => language switch
    {
        EggMdEditor.App.Language.English or EggMdEditor.App.Language.En or EggMdEditor.App.Language.Ingles => "en",
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
    string CurrentLanguage { get; set; }
    Theme Theme { get; set; }
    Theme CurrentTheme { get; set; }
    bool TrySetLanguage(string? language);
    bool TrySetLanguage(Language language);
    void SetLanguage(string language);
    void SetLanguage(Language language);
    bool TrySetTheme(Theme theme);
    bool TrySetTheme(string? theme);
    void SetTheme(Theme theme);
    void SetTheme(string theme);
    void ToggleTheme();
}
