using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class I18nCatalogTests
{
    [Fact]
    public void DefaultLanguage_IsSpanish()
    {
        var catalog = new I18nCatalog();
        Assert.Equal("es", catalog.CurrentLanguage);
        Assert.Equal("es", catalog.FallbackLanguage);
    }

    [Fact]
    public void Get_Spanish_ReturnsSpanishText()
    {
        var catalog = new I18nCatalog();
        Assert.Equal("Archivo", catalog.Get("Menu.File"));
        Assert.Equal("Archivo", catalog["Menu.File"]);
    }

    [Fact]
    public void SwitchToEnglish_ReturnsEnglishText()
    {
        var catalog = new I18nCatalog();
        Assert.True(catalog.TrySetLanguage("en"));
        Assert.Equal("en", catalog.CurrentLanguage);
        Assert.Equal("File", catalog.Get("Menu.File"));
    }

    [Fact]
    public void MissingKeyInEnglish_FallsBackToSpanish()
    {
        var catalog = new I18nCatalog();
        catalog.TrySetLanguage("en");
        // Common.Ok only in Spanish
        Assert.Equal("Aceptar", catalog.Get("Common.Ok"));
    }

    [Fact]
    public void UnresolvedKeys_ContainsMissingEnglishKeys()
    {
        var catalog = new I18nCatalog();
        catalog.TrySetLanguage("en");
        var unresolved = catalog.GetUnresolvedKeys();
        Assert.Contains("Common.Ok", unresolved);
    }

    [Fact]
    public void TrySetLanguage_UnknownLanguage_ReturnsFalse_AndKeepsCurrent()
    {
        var catalog = new I18nCatalog();
        catalog.TrySetLanguage("en");
        var before = catalog.CurrentLanguage;
        Assert.False(catalog.TrySetLanguage("fr"));
        Assert.Equal(before, catalog.CurrentLanguage);
        // UI unchanged: still English
        Assert.Equal("File", catalog.Get("Menu.File"));
    }

    [Fact]
    public void SetLanguage_Throws_WhenLanguageNotLoaded()
    {
        var catalog = new I18nCatalog();
        Assert.Throws<InvalidOperationException>(() => catalog.SetLanguage("it"));
    }

    [Fact]
    public void MissingKeyEntirely_ReturnsKey()
    {
        var catalog = new I18nCatalog();
        Assert.Equal("NonExistent.Key", catalog.Get("NonExistent.Key"));
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenKeyMissing()
    {
        var catalog = new I18nCatalog();
        Assert.False(catalog.TryGet("Nope.Key", out var val));
        Assert.Equal("Nope.Key", val);
    }

    [Fact]
    public void InjectionConstructor_Works()
    {
        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["es"] = new Dictionary<string, string> { ["K"] = "Hola" },
            ["en"] = new Dictionary<string, string> { ["K"] = "Hello" },
        };
        var catalog = new I18nCatalog(resources, "es", "es");
        Assert.Equal("Hola", catalog.Get("K"));
        catalog.TrySetLanguage("en");
        Assert.Equal("Hello", catalog.Get("K"));
    }

    [Fact]
    public void InjectionConstructor_MissingTarget_FallbackToSpanish()
    {
        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["es"] = new Dictionary<string, string> { ["K"] = "Hola", ["OnlyEs"] = "Solo" },
            ["en"] = new Dictionary<string, string> { ["K"] = "Hello" },
        };
        var catalog = new I18nCatalog(resources, "en", "es");
        Assert.Equal("Hello", catalog.Get("K"));
        Assert.Equal("Solo", catalog.Get("OnlyEs"));
    }

    [Fact]
    public void AbortWithoutPartialChange_OnFailedSwitch()
    {
        var catalog = new I18nCatalog();
        catalog.TrySetLanguage("en");
        // attempt switch to unloaded language
        var currentBefore = catalog.CurrentLanguage;
        var textBefore = catalog.Get("Menu.File");
        Assert.False(catalog.TrySetLanguage("de"));
        Assert.Equal(currentBefore, catalog.CurrentLanguage);
        Assert.Equal(textBefore, catalog.Get("Menu.File"));
    }

    [Fact]
    public void SupportedLanguages_ContainsEsAndEn()
    {
        var catalog = new I18nCatalog();
        Assert.Contains("es", catalog.SupportedLanguages);
        Assert.Contains("en", catalog.SupportedLanguages);
    }

    [Fact]
    public void AliasMethods_Work()
    {
        var catalog = new I18nCatalog();
        Assert.Equal(catalog.Get("Menu.File"), catalog.GetString("Menu.File"));
        Assert.Equal(catalog.Get("Menu.File"), catalog.Resolve("Menu.File"));
        Assert.Equal(catalog.Get("Menu.File"), catalog.Translate("Menu.File"));
        Assert.Equal(catalog.Get("Menu.File"), catalog.Localize("Menu.File"));
        Assert.Equal(catalog.Get("Menu.File"), catalog.GetLocalizedString("Menu.File"));
    }

    [Fact]
    public void TryGet_WithLanguageParameter()
    {
        var catalog = new I18nCatalog();
        Assert.True(catalog.TryGet("Menu.File", "en", out var en));
        Assert.Equal("File", en);
        // missing in en falls back to es
        Assert.True(catalog.TryGet("Common.Ok", "en", out var ok));
        Assert.Equal("Aceptar", ok);
    }
}
