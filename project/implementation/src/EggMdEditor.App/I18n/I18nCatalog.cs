using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EggMdEditor.App.I18n;

/// <summary>
/// Implementación del catálogo i18n es/en con fallback a español (ENT-006, COM-002, ERR-004).
/// Los recursos se cargan desde los .resx embebidos (español neutro + satélite inglés).
/// </summary>
public sealed class I18nCatalog : II18nCatalog
{
    public const string Spanish = "es";
    public const string English = "en";

    private readonly Dictionary<string, Dictionary<string, string>> _resources;
    private readonly string _fallbackLanguage;
    private readonly ILogger _logger;
    private string _currentLanguage;

    public I18nCatalog()
        : this(LoadEmbeddedResources(), Spanish, Spanish, null)
    {
    }

    /// <summary>
    /// Crea el catálogo embebido con el registro técnico de LOG-002 (ERR-003).
    /// </summary>
    public I18nCatalog(ILogger logger)
        : this(LoadEmbeddedResources(), Spanish, Spanish, logger)
    {
    }

    public I18nCatalog(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> resources,
        string defaultLanguage = Spanish,
        string fallbackLanguage = Spanish,
        ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;

        _resources = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        if (resources is not null)
        {
            foreach (var (lang, dict) in resources)
            {
                var copy = new Dictionary<string, string>(StringComparer.Ordinal);
                if (dict is not null)
                {
                    foreach (var (k, v) in dict)
                    {
                        copy[k] = v;
                    }
                }

                // ERR-003: un catálogo sin textos no se considera cargado; la
                // conmutación a ese idioma se abortará sin cambio parcial.
                if (copy.Count > 0)
                {
                    _resources[lang] = copy;
                }
            }
        }

        _fallbackLanguage = NormalizeLanguage(fallbackLanguage);
        _currentLanguage = NormalizeLanguage(defaultLanguage);

        if (!_resources.ContainsKey(_fallbackLanguage))
        {
            _resources[_fallbackLanguage] = new Dictionary<string, string>(StringComparer.Ordinal);
        }
        if (!_resources.ContainsKey(_currentLanguage))
        {
            _currentLanguage = _fallbackLanguage;
        }
    }

    public static I18nCatalog Instance { get; } = new I18nCatalog();

    /// <summary>
    /// Identificador de la sesión técnica (LOG-002). La composición de la
    /// aplicación lo fija para que el registro de ERR-003 sea correlacionable;
    /// sin sesión compuesta puede ser <c>null</c>.
    /// </summary>
    public string? SessionId { get; set; }

    public string CurrentLanguage => _currentLanguage;
    public string Language => _currentLanguage;
    public string FallbackLanguage => _fallbackLanguage;

    public IReadOnlyList<string> SupportedLanguages => new List<string>(_resources.Keys);
    public IReadOnlyList<string> Languages => SupportedLanguages;

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key ?? string.Empty;
        }

        if (_resources.TryGetValue(_currentLanguage, out var current)
            && current.TryGetValue(key, out var value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (_resources.TryGetValue(_fallbackLanguage, out var fallback)
            && fallback.TryGetValue(key, out var fallbackValue)
            && !string.IsNullOrWhiteSpace(fallbackValue))
        {
            return fallbackValue;
        }

        return key;
    }

    public string this[string key] => Get(key);

    public string GetString(string key) => Get(key);
    public string Resolve(string key) => Get(key);
    public string Translate(string key) => Get(key);
    public string Localize(string key) => Get(key);
    public string GetLocalizedString(string key) => Get(key);

    public bool TryGet(string key, out string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = key ?? string.Empty;
            return false;
        }

        if (_resources.TryGetValue(_currentLanguage, out var current)
            && current.TryGetValue(key, out var v)
            && !string.IsNullOrWhiteSpace(v))
        {
            value = v;
            return true;
        }

        // fallback
        if (_resources.TryGetValue(_fallbackLanguage, out var fb)
            && fb.TryGetValue(key, out var fv)
            && !string.IsNullOrWhiteSpace(fv))
        {
            value = fv;
            return true;
        }

        value = key;
        return false;
    }

    public bool TryGet(string key, string language, out string value)
    {
        var code = NormalizeLanguage(language);
        if (string.IsNullOrEmpty(key))
        {
            value = key ?? string.Empty;
            return false;
        }

        if (_resources.TryGetValue(code, out var dict)
            && dict.TryGetValue(key, out var v)
            && !string.IsNullOrWhiteSpace(v))
        {
            value = v;
            return true;
        }

        // fallback to spanish
        if (_resources.TryGetValue(_fallbackLanguage, out var fb)
            && fb.TryGetValue(key, out var fv)
            && !string.IsNullOrWhiteSpace(fv))
        {
            value = fv;
            return true;
        }

        value = key;
        return false;
    }

    public bool TrySetLanguage(string language)
    {
        var code = NormalizeLanguage(language);
        if (!_resources.ContainsKey(code))
        {
            // ERR-003/LOG-002: el catálogo destino no carga; se aborta sin
            // cambio parcial y se registra el fallo (sin aviso visible).
            _logger.LogError(
                "No se pudo cargar el catálogo i18n. Hora: {Timestamp:O}. " +
                "Sesión: {SessionId}. Idioma origen: {CurrentLanguage}; " +
                "idioma destino: {TargetLanguage}. Se aborta la conmutación y " +
                "se conserva el idioma activo.",
                DateTimeOffset.UtcNow,
                SessionId ?? "sin-sesion",
                _currentLanguage,
                language);
            return false;
        }

        _currentLanguage = code;
        return true;
    }

    public void SetLanguage(string language)
    {
        if (!TrySetLanguage(language))
        {
            throw new InvalidOperationException($"No se pudo cargar el catálogo i18n para el idioma '{language}'.");
        }
    }

    public IReadOnlyList<string> GetUnresolvedKeys()
    {
        if (!_resources.TryGetValue(_currentLanguage, out var current))
        {
            current = new Dictionary<string, string>(StringComparer.Ordinal);
        }
        if (!_resources.TryGetValue(_fallbackLanguage, out var fallback))
        {
            return Array.Empty<string>();
        }

        var result = new List<string>();
        foreach (var k in fallback.Keys)
        {
            // ERR-004: una clave ausente o con texto vacío no está resuelta
            // en el idioma activo; se resuelve con el fallback español.
            if (!current.TryGetValue(k, out var value) || string.IsNullOrWhiteSpace(value))
            {
                result.Add(k);
            }
        }
        return result.AsReadOnly();
    }

    public IEnumerable<string> UnresolvedKeys => GetUnresolvedKeys();

    private static string NormalizeLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return Spanish;
        }

        var trimmed = language.Trim().ToLowerInvariant();
        if (trimmed.StartsWith("es", StringComparison.Ordinal))
        {
            return Spanish;
        }
        if (trimmed.StartsWith("en", StringComparison.Ordinal))
        {
            return English;
        }
        if (trimmed is "spanish" or "español")
        {
            return Spanish;
        }
        if (trimmed is "english" or "inglés" or "ingles")
        {
            return English;
        }

        return trimmed.Length >= 2 ? trimmed.Substring(0, 2) : trimmed;
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadEmbeddedResources()
    {
        var assembly = typeof(I18nCatalog).Assembly;
        var baseName = "EggMdEditor.App.Resources.Strings";
        var rm = new ResourceManager(baseName, assembly);

        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        // Español = recursos neutros (invariant culture).
        var spanish = new Dictionary<string, string>(StringComparer.Ordinal);
        var neutralSet = rm.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false);
        if (neutralSet is not null)
        {
            foreach (System.Collections.DictionaryEntry entry in neutralSet)
            {
                if (entry.Key is string k && entry.Value is string v)
                {
                    spanish[k] = v;
                }
            }
        }

        if (spanish.Count == 0)
        {
            throw new InvalidOperationException("El catálogo i18n en español (recursos neutros) no se pudo cargar.");
        }

        result[Spanish] = spanish;

        // Inglés = satélite 'en' (sin fallback a padres para detectar carga real).
        var englishSet = rm.GetResourceSet(new CultureInfo(English), createIfNotExists: true, tryParents: false);
        if (englishSet is not null)
        {
            var english = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (System.Collections.DictionaryEntry entry in englishSet)
            {
                if (entry.Key is string k && entry.Value is string v)
                {
                    english[k] = v;
                }
            }

            if (english.Count > 0)
            {
                result[English] = english;
            }
        }

        return new Dictionary<string, IReadOnlyDictionary<string, string>>(
            result,
            StringComparer.OrdinalIgnoreCase);
    }
}
