using System.Collections.Generic;

namespace EggMdEditor.App.I18n;

/// <summary>
/// Catálogo de cadenas i18n (ENT-006) con idiomas español (defecto) e inglés
/// y fallback a español (ERR-004). Extensible a más idiomas (A-005).
/// </summary>
public interface II18nCatalog
{
    string CurrentLanguage { get; }
    string FallbackLanguage { get; }
    IReadOnlyList<string> SupportedLanguages { get; }

    string Get(string key);
    string this[string key] { get; }

    bool TryGet(string key, out string value);
    bool TryGet(string key, string language, out string value);

    bool TrySetLanguage(string language);
    void SetLanguage(string language);

    IReadOnlyList<string> GetUnresolvedKeys();
}
