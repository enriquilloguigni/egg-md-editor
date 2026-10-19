using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Source of the i18n texts (key→text) per language code behind which the
/// switching service works (COM-001). «Loading the target language's
/// catalog» is the operation that can fail and cause the abort with no
/// partial change (ERR-003); being behind an interface of the core, the
/// service does not depend on where the texts live and the tests can simulate
/// the load failure.
/// </summary>
public interface II18nTextProvider
{
    /// <summary>
    /// Tries to load the texts of the given language code. Returns
    /// <c>false</c> if the language's catalog does not load (ERR-003); an
    /// empty or null code keeps the semantics of the string catalog: it falls
    /// back to the default language (BR-F001-004).
    /// </summary>
    bool TryLoad(string languageCode, out IReadOnlyDictionary<string, string> texts);
}

/// <summary>
/// Default provider: loads the texts from the in-memory i18n string catalog
/// (T-003). A registered language with text loads; an unregistered code does
/// not load and causes the abort of the switching (ERR-003).
/// </summary>
public sealed class I18nStringCatalogProvider : II18nTextProvider
{
    private static readonly IReadOnlyDictionary<string, string> EmptyTexts =
        new Dictionary<string, string>();

    /// <summary>Single stateless instance (the catalog is static).</summary>
    public static I18nStringCatalogProvider Instance { get; } = new();

    private I18nStringCatalogProvider()
    {
    }

    /// <summary>
    /// Loads the texts of the i18n string catalog for the given code,
    /// normalized as in the key resolution (case and whitespace do not count;
    /// empty or null falls back to the default language).
    /// </summary>
    public bool TryLoad(string languageCode, out IReadOnlyDictionary<string, string> texts)
    {
        var code = string.IsNullOrWhiteSpace(languageCode)
            ? I18nStringCatalog.DefaultCode
            : languageCode.Trim().ToLowerInvariant();

        if (I18nStringCatalog.TextsByLanguage.TryGetValue(code, out var foundTexts)
            && foundTexts.Count > 0)
        {
            texts = foundTexts;
            return true;
        }

        texts = EmptyTexts;
        return false;
    }
}
