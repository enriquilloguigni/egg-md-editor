namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Catalog of interface languages (DS-001): controlled values <c>Español</c>
/// and <c>English</c>, mutually exclusive (BR-007). The session's startup
/// value is always Español.
/// </summary>
public enum InterfaceLanguage
{
    Spanish = 1,
    English = 2,
}

/// <summary>
/// In-memory catalog of the <c>Language</c> catalog (DS-001): ordered list of
/// values and session default. No persistence (FR-013 → F-030).
/// </summary>
public static class LanguageCatalog
{
    /// <summary>Controlled values of the catalog, in canonical order.</summary>
    public static IReadOnlyList<InterfaceLanguage> Values { get; } =
        [InterfaceLanguage.Spanish, InterfaceLanguage.English];

    /// <summary>Session default: exactly one active language, always Español (BR-007).</summary>
    public static InterfaceLanguage Default { get; } = InterfaceLanguage.Spanish;
}
