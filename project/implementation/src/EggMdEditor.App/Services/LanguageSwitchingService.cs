using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Result of a language switching operation (COM-001).
/// </summary>
public enum LanguageSwitchState
{
    /// <summary>
    /// Switching completed: the session's active language was updated and the
    /// relabeling subscribers were notified exactly once.
    /// </summary>
    Switched = 1,

    /// <summary>
    /// The target language is already the active one: null operation, no
    /// change, no notification and no log (there is always exactly one active
    /// language, BR-F001-004).
    /// </summary>
    AlreadyActive = 2,

    /// <summary>
    /// Switching aborted with no partial change (ERR-003): the target
    /// language's i18n catalog does not load; the previous active language
    /// remains, with the window and the state intact. The failure is only
    /// logged (LOG-002).
    /// </summary>
    AbortedCatalogUnavailable = 3,
}

/// <summary>
/// Detailed result of a language switching (COM-001), with the source and
/// target languages and the i18n keys unresolved in the target language,
/// which are resolved in Spanish as it is the default language (ERR-004).
/// </summary>
/// <param name="State">Final state of the operation.</param>
/// <param name="SourceLanguage">Active language before the operation.</param>
/// <param name="TargetLanguage">Requested language.</param>
/// <param name="UnresolvedKeys">
/// Published keys absent from the target language's catalog, in ordinal
/// order; empty if the relabeling is complete.
/// </param>
public sealed record LanguageSwitchResult(
    LanguageSwitchState State,
    InterfaceLanguage SourceLanguage,
    InterfaceLanguage TargetLanguage,
    IReadOnlyList<string> UnresolvedKeys);

/// <summary>
/// Hot relabeling notification (DS-003): emitted exactly once per switching,
/// after applying the new active language to the session, so that the
/// subscribers resolve every string from the already-applied state. The
/// values are mutually exclusive (BR-007).
/// </summary>
public sealed class LanguageSwitchedEventArgs : EventArgs
{
    /// <summary>Active language before the switching.</summary>
    public InterfaceLanguage PreviousLanguage { get; }

    /// <summary>Active language after the switching.</summary>
    public InterfaceLanguage NewLanguage { get; }

    /// <summary>Creates the notification with the pair of languages of the switching.</summary>
    public LanguageSwitchedEventArgs(InterfaceLanguage previousLanguage, InterfaceLanguage newLanguage)
    {
        PreviousLanguage = previousLanguage;
        NewLanguage = newLanguage;
    }
}

/// <summary>
/// Interface language switching service (COM-001): synchronous, in-process
/// channel that receives the chosen language {es, en} from the «Interface
/// language» entry and makes the hot relabeling effective without restart
/// (BR-F001-005, NFR-006). Every visible string is resolved from an
/// observable source notified on switching (DS-003): the subscribers of
/// <see cref="LanguageSwitched"/> re-resolve their labels and tooltips
/// against the active language already applied to the session. If the target
/// language's i18n catalog does not load, the switching aborts with no
/// partial change: the previous active language remains, nothing is notified
/// and the failure is only logged (ERR-003); the user can retry. Each
/// operation ends by logging LOG-002: <c>Debug</c> if it is successful and
/// complete, <c>Warning</c> if i18n keys remain unresolved (ERR-004) and
/// <c>Error</c> if the catalog does not load. The only mutation of the
/// session is the active language: the menu state and the tooltip remain
/// intact (BR-F001-005) and the choice is not persisted (FR-013 → F-030).
/// Synchronous and in process (C-001): all operations run on the UI thread,
/// without blocking.
/// </summary>
public sealed class LanguageSwitchingService
{
    private readonly ApplicationSession _session;
    private readonly II18nTextProvider _textProvider;
    private readonly ILogOutput _logOutput;
    private readonly Func<DateTimeOffset> _clock;

    /// <summary>
    /// Single hot relabeling notification (DS-003), emitted after applying the
    /// new language to the session. A subscriber that throws is a programming
    /// error: it is not caught.
    /// </summary>
    public event EventHandler<LanguageSwitchedEventArgs>? LanguageSwitched;

    /// <summary>
    /// Creates the service over the application session. The text provider
    /// defaults to the in-memory i18n string catalog (T-003) and the log
    /// output to the null one; the clock allows fixing the ISO 8601 timestamp
    /// of the LOG-002 logs.
    /// </summary>
    public LanguageSwitchingService(
        ApplicationSession session,
        II18nTextProvider? textProvider = null,
        ILogOutput? logOutput = null,
        Func<DateTimeOffset>? clock = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _textProvider = textProvider ?? I18nStringCatalogProvider.Instance;
        _logOutput = logOutput ?? NullLogOutput.Instance;
        _clock = clock ?? DefaultClock;
    }

    /// <summary>Active language of the session: the observable source of the relabeling (BR-F001-004).</summary>
    public InterfaceLanguage ActiveLanguage => _session.ActiveLanguage;

    private static DateTimeOffset DefaultClock() => DateTimeOffset.UtcNow;

    /// <summary>
    /// Switches the session's active language to the given target (COM-001).
    /// It loads the target language's catalog first: if it does not load, the
    /// operation aborts with no partial change and no notification (ERR-003);
    /// if it loads, it updates the active language —the only mutation of the
    /// session, with the state intact (BR-F001-005)— and notifies the
    /// relabeling subscribers exactly once (DS-003), logging LOG-002 at the
    /// end. Switching to the already-active language is a null operation. The
    /// target must belong to the language catalog (BR-007); any other value
    /// is a programming error.
    /// </summary>
    public LanguageSwitchResult Switch(InterfaceLanguage targetLanguage)
    {
        if (!LanguageCatalog.Values.Contains(targetLanguage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetLanguage), targetLanguage,
                "The target language does not belong to the interface language catalog (BR-007).");
        }

        var sourceLanguage = _session.ActiveLanguage;
        var sourceCode = I18nStringCatalog.CodeOf(sourceLanguage);
        var targetCode = I18nStringCatalog.CodeOf(targetLanguage);

        if (sourceLanguage == targetLanguage)
        {
            return new LanguageSwitchResult(
                LanguageSwitchState.AlreadyActive, sourceLanguage, targetLanguage, []);
        }

        // ERR-003: the target language's catalog is loaded before touching the
        // state, so the abort leaves no partial change.
        if (!_textProvider.TryLoad(targetCode, out var targetTexts))
        {
            Log(
                LogLevel.Error,
                $"LOG-002: conmutación de idioma {sourceCode} → {targetCode} abortada: " +
                "el catálogo i18n del idioma destino no carga; se conserva el idioma activo " +
                $"{sourceCode} con la ventana y el estado intactos (ERR-003).");

            return new LanguageSwitchResult(
                LanguageSwitchState.AbortedCatalogUnavailable,
                sourceLanguage, targetLanguage, []);
        }

        // ERR-004: published keys absent from the target catalog; they will be
        // resolved in Spanish (default language) during the relabeling.
        var unresolvedKeys = UnresolvedKeysIn(targetTexts);

        // Only mutation of the session: the active language (BR-F001-005).
        _session.ActiveLanguage = targetLanguage;

        // DS-003: single notification per switching, after applying the state.
        LanguageSwitched?.Invoke(this, new LanguageSwitchedEventArgs(sourceLanguage, targetLanguage));

        if (unresolvedKeys.Count > 0)
        {
            Log(
                LogLevel.Warning,
                $"LOG-002: conmutación de idioma {sourceCode} → {targetCode} completada " +
                "tras el relabelizado; claves i18n sin resolver, resueltas en español " +
                $"(ERR-004): {string.Join(", ", unresolvedKeys)}.");
        }
        else
        {
            Log(
                LogLevel.Debug,
                $"LOG-002: conmutación de idioma {sourceCode} → {targetCode} completada " +
                "tras el relabelizado, sin claves i18n sin resolver.");
        }

        return new LanguageSwitchResult(
            LanguageSwitchState.Switched, sourceLanguage, targetLanguage, unresolvedKeys);
    }

    /// <summary>
    /// Keys published by the catalogs (the single source is the default
    /// language's set of keys, BR-F001-007) absent from the target catalog,
    /// in ordinal order for a deterministic log.
    /// </summary>
    private List<string> UnresolvedKeysIn(IReadOnlyDictionary<string, string> targetTexts) =>
        I18nStringCatalog.TextsByLanguage[I18nStringCatalog.DefaultCode].Keys
            .Where(key => !targetTexts.ContainsKey(key))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Logs through the log output: ISO 8601 timestamp and session identifier
    /// shared by every log (logs.md, NFR-003), with no writing to disk and no
    /// leaving the machine (C-001).
    /// </summary>
    private void Log(LogLevel level, string message) =>
        _logOutput.Write(
            level,
            $"{_clock().ToString("O")} [sesión {_session.SessionId}] {message}");
}
