using EggMdEditor.App.Catalogs;
using Xunit;

namespace EggMdEditor.App.Tests;

// Catalog of i18n strings (DS-001): every visible string of SC-001 is
// resolved from here in the active language (BR-F001-007), with Spanish as
// default (BR-F001-004) and Spanish fallback for keys unresolved in the
// target language (ERR-004). The coherence with the keys published by the
// menu (T-002) and bar (T-002) catalogs is checked against their lists:
// there can be no extra or missing keys.
public class I18nStringCatalogTests
{
    [Fact]
    public void The_default_language_is_spanish()
    {
        Assert.Equal("es", I18nStringCatalog.DefaultCode);
        Assert.Equal("es", I18nStringCatalog.CodeOf(InterfaceLanguage.Spanish));
        Assert.Equal("en", I18nStringCatalog.CodeOf(InterfaceLanguage.English));
    }

    [Fact]
    public void Every_language_value_has_a_code_and_the_unknown_one_falls_back_to_default()
    {
        Assert.All(
            LanguageCatalog.Values,
            language => Assert.False(string.IsNullOrWhiteSpace(
                I18nStringCatalog.CodeOf(language))));

        Assert.Equal(
            I18nStringCatalog.DefaultCode,
            I18nStringCatalog.CodeOf((InterfaceLanguage)99));
    }

    [Fact]
    public void Covers_100_percent_of_the_keys_published_by_the_catalogs()
    {
        var publishedKeys = KeysPublishedByCatalogs()
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

        // The published labels are those of the menu and bar catalogs (T-002)
        // plus the 2 values of the Interface language entry's selection
        // (SC-001, T-010), derived as MainMenuBuilder builds them from the
        // language catalog.
        Assert.Equal(
            6 + 48 + 12 + LanguageCatalog.Values.Count,
            publishedKeys.Count);
        Assert.Equal(
            publishedKeys,
            I18nStringCatalog.TextsByLanguage[I18nStringCatalog.DefaultCode]
                .Keys.OrderBy(key => key, StringComparer.Ordinal));
        Assert.Equal(
            publishedKeys,
            I18nStringCatalog.TextsByLanguage["en"]
                .Keys.OrderBy(key => key, StringComparer.Ordinal));
        Assert.Equal(
            6 + 48 + 12 + LanguageCatalog.Values.Count,
            I18nStringCatalog.TotalStrings);
    }

    [Fact]
    public void Every_key_has_text_in_es_and_en_and_both_languages_have_parity()
    {
        var esKeys = I18nStringCatalog.TextsByLanguage["es"].Keys
            .OrderBy(key => key, StringComparer.Ordinal);
        var enKeys = I18nStringCatalog.TextsByLanguage["en"].Keys
            .OrderBy(key => key, StringComparer.Ordinal);

        Assert.Equal(esKeys, enKeys);

        Assert.All(
            I18nStringCatalog.TextsByLanguage,
            pair => Assert.All(
                pair.Value.Values,
                text => Assert.False(string.IsNullOrWhiteSpace(text))));
    }

    [Fact]
    public void Resolves_in_the_active_language_with_the_language_code()
    {
        Assert.Equal("Archivo", I18nStringCatalog.Resolve("menu.archivo", "es"));
        Assert.Equal("File", I18nStringCatalog.Resolve("menu.archivo", "en"));
        Assert.Equal("Guardar como…", I18nStringCatalog.Resolve("menu.archivo.guardar-como", "es"));
        Assert.Equal("Save as…", I18nStringCatalog.Resolve("menu.archivo.guardar-como", "en"));
        Assert.Equal("Nota al pie", I18nStringCatalog.Resolve("barra.nota-al-pie", "es"));
        Assert.Equal("Footnote", I18nStringCatalog.Resolve("barra.nota-al-pie", "en"));
    }

    [Fact]
    public void Resolves_in_the_active_language_with_the_language_catalog()
    {
        Assert.Equal("Edición", I18nStringCatalog.Resolve("menu.edicion", InterfaceLanguage.Spanish));
        Assert.Equal("Edit", I18nStringCatalog.Resolve("menu.edicion", InterfaceLanguage.English));
        Assert.Equal("Índice", I18nStringCatalog.Resolve("barra.indice", InterfaceLanguage.Spanish));
        Assert.Equal("Table of contents", I18nStringCatalog.Resolve("barra.indice", InterfaceLanguage.English));
    }

    [Fact]
    public void Key_absent_in_target_language_resolves_in_spanish_ERR004()
    {
        var spanishTexts = I18nStringCatalog.TextsByLanguage[I18nStringCatalog.DefaultCode];

        // An unregistered language leaves every key unresolved: each one
        // falls back to the default language (ERR-004), never to null or
        // empty.
        Assert.All(
            KeysPublishedByCatalogs(),
            key => Assert.Equal(spanishTexts[key], I18nStringCatalog.Resolve(key, "fr")));
    }

    [Fact]
    public void The_language_code_is_normalized_regardless_of_case_or_whitespace()
    {
        Assert.Equal("Archivo", I18nStringCatalog.Resolve("menu.archivo", "ES"));
        Assert.Equal("Archivo", I18nStringCatalog.Resolve("menu.archivo", " es "));
        Assert.Equal("File", I18nStringCatalog.Resolve("menu.archivo", " En "));
    }

    [Fact]
    public void Absent_or_empty_code_falls_back_to_default_language()
    {
        Assert.Equal("Ayuda", I18nStringCatalog.Resolve("menu.ayuda", ""));
        Assert.Equal("Ayuda", I18nStringCatalog.Resolve("menu.ayuda", "   "));
        Assert.Equal("Ayuda", I18nStringCatalog.Resolve("menu.ayuda", null!));
    }

    [Fact]
    public void Unknown_key_never_leaves_text_unresolved()
    {
        var resolved = I18nStringCatalog.Resolve("clave.inexistente", "en");

        Assert.Equal("clave.inexistente", resolved);
        Assert.False(string.IsNullOrWhiteSpace(resolved));
    }

    [Fact]
    public void Null_key_is_a_programming_error()
    {
        Assert.Throws<ArgumentNullException>(
            () => I18nStringCatalog.Resolve(null!, "es"));
    }

    /// <summary>
    /// Union of the keys published by the T-002 catalogs: the 6 menus and
    /// their 48 entries of the main menu (FR-011) and the 12 controls of the
    /// format bar (FR-012), with no duplicates.
    /// </summary>
    private static IEnumerable<string> KeysPublishedByCatalogs() =>
        MainMenuCatalog.Menus
            .Select(menu => menu.Key)
            .Concat(MainMenuCatalog.Menus
                .SelectMany(menu => menu.Entries.Select(entry => entry.Key)))
            .Concat(FormatBarCatalog.Controls.Select(control => control.Key))
            .Concat(LanguageValueKeys());

    // Español | English values of the Interface language entry (T-010):
    // keys derived from the language catalog just as MainMenuBuilder
    // publishes them.
    private static IEnumerable<string> LanguageValueKeys() =>
        LanguageCatalog.Values.Select(Services.MainMenuBuilder.LanguageValueKey);
}
