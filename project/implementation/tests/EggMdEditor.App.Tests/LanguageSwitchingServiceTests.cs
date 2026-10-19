using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Language switching service (COM-001): hot relabeling without restart
// keeping window and state (BR-F001-005, DS-003), aborted with no partial
// change if the target language's catalog does not load (ERR-003, AC-010)
// and technical log LOG-002 with Debug level on full success, Warning if
// keys remain unresolved (ERR-004, AC-009) and Error if the catalog does
// not load.
public class LanguageSwitchingServiceTests
{
    private static readonly DateTimeOffset FixedInstant =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void The_service_active_language_is_the_session_one_and_starts_in_spanish()
    {
        var session = new ApplicationSession();
        var service = new LanguageSwitchingService(session);

        Assert.Equal(InterfaceLanguage.Spanish, service.ActiveLanguage);
        Assert.Equal(session.ActiveLanguage, service.ActiveLanguage);
    }

    [Fact]
    public void Switching_to_en_updates_the_session_notifies_once_and_keeps_the_state()
    {
        var session = new ApplicationSession
        {
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(
            session, logOutput: captured, clock: () => FixedInstant);
        var notifications = new List<LanguageSwitchedEventArgs>();
        InterfaceLanguage languageSeenInNotification = InterfaceLanguage.Spanish;
        service.LanguageSwitched += (_, e) =>
        {
            notifications.Add(e);
            languageSeenInNotification = service.ActiveLanguage;
        };

        var result = service.Switch(InterfaceLanguage.English);

        Assert.Equal(LanguageSwitchState.Switched, result.State);
        Assert.Equal(InterfaceLanguage.Spanish, result.SourceLanguage);
        Assert.Equal(InterfaceLanguage.English, result.TargetLanguage);
        Assert.Empty(result.UnresolvedKeys);
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal(InterfaceLanguage.English, service.ActiveLanguage);

        // DS-003: single notification, emitted with the state already applied.
        Assert.Single(notifications);
        Assert.Equal(InterfaceLanguage.Spanish, notifications[0].PreviousLanguage);
        Assert.Equal(InterfaceLanguage.English, notifications[0].NewLanguage);
        Assert.Equal(InterfaceLanguage.English, languageSeenInNotification);

        // BR-F001-005: only the language changes; the session state remains intact.
        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);

        // LOG-002 on full success: a single Debug log with time, session and
        // source → target languages.
        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Debug, log.Level);
        Assert.Contains(FixedInstant.ToString("O"), log.Message);
        Assert.Contains(session.SessionId, log.Message);
        Assert.Contains("es → en", log.Message);
    }

    [Fact]
    public void Switching_from_en_back_to_es_updates_and_notifies_in_the_reverse_direction()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, logOutput: captured);
        var notifications = new List<LanguageSwitchedEventArgs>();
        service.LanguageSwitched += (_, e) => notifications.Add(e);

        Assert.Equal(
            LanguageSwitchState.Switched,
            service.Switch(InterfaceLanguage.English).State);
        Assert.Equal(
            LanguageSwitchState.Switched,
            service.Switch(InterfaceLanguage.Spanish).State);

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal(2, notifications.Count);
        Assert.Equal((InterfaceLanguage.Spanish, InterfaceLanguage.English),
            (notifications[0].PreviousLanguage, notifications[0].NewLanguage));
        Assert.Equal((InterfaceLanguage.English, InterfaceLanguage.Spanish),
            (notifications[1].PreviousLanguage, notifications[1].NewLanguage));
        Assert.Contains("en → es", captured.Logs[1].Message);
    }

    [Fact]
    public void Switching_to_the_already_active_language_is_a_null_operation()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, logOutput: captured);
        var notifications = 0;
        service.LanguageSwitched += (_, _) => notifications++;

        var result = service.Switch(InterfaceLanguage.Spanish);

        Assert.Equal(LanguageSwitchState.AlreadyActive, result.State);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal(0, notifications);
        Assert.Empty(captured.Logs);
    }

    [Fact]
    public void Switching_aborts_with_no_partial_change_and_allows_retrying_ERR003()
    {
        var session = new ApplicationSession
        {
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var provider = new TestProvider();
        provider.SimulateFailure("en");
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, provider, captured);
        var notifications = 0;
        service.LanguageSwitched += (_, _) => notifications++;

        var result = service.Switch(InterfaceLanguage.English);

        // ERR-003: aborted with no partial change, with the window and the state intact.
        Assert.Equal(LanguageSwitchState.AbortedCatalogUnavailable, result.State);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal(InterfaceLanguage.Spanish, service.ActiveLanguage);
        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);
        Assert.Equal(0, notifications);

        // LOG-002: the failure is only logged, with Error level, with no visible message.
        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Contains(session.SessionId, log.Message);
        Assert.Contains("es → en", log.Message);

        // Retry with the catalog already available: the switching proceeds.
        provider.Allow("en");
        var retry = service.Switch(InterfaceLanguage.English);

        Assert.Equal(LanguageSwitchState.Switched, retry.State);
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal(1, notifications);
        Assert.Equal(2, captured.Logs.Count);
        Assert.Equal(LogLevel.Debug, captured.Logs[1].Level);
    }

    [Fact]
    public void Unresolved_keys_in_the_target_are_listed_and_logged_as_Warning_ERR004()
    {
        var session = new ApplicationSession();
        var provider = new TestProvider();
        provider.Register(
            "en",
            I18nStringCatalog.TextsByLanguage["en"]
                .Where(pair => pair.Key is not ("menu.archivo" or "barra.cita"))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, provider, captured);
        var notifications = 0;
        service.LanguageSwitched += (_, _) => notifications++;

        var result = service.Switch(InterfaceLanguage.English);

        // The switching is not aborted: the catalog loads, with unresolved
        // keys that fall back to the default language (ERR-004).
        Assert.Equal(LanguageSwitchState.Switched, result.State);
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal(1, notifications);
        Assert.Equal((string[])["barra.cita", "menu.archivo"], result.UnresolvedKeys);

        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Warning, log.Level);
        Assert.Contains("es → en", log.Message);
        Assert.Contains("menu.archivo", log.Message);
        Assert.Contains("barra.cita", log.Message);
    }

    [Fact]
    public void With_the_static_catalog_every_switching_is_es_en_complete_and_Debug()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, logOutput: captured);

        var forth = service.Switch(InterfaceLanguage.English);
        var back = service.Switch(InterfaceLanguage.Spanish);

        Assert.Empty(forth.UnresolvedKeys);
        Assert.Empty(back.UnresolvedKeys);
        Assert.Equal(2, captured.Logs.Count);
        Assert.All(captured.Logs, log => Assert.Equal(LogLevel.Debug, log.Level));
    }

    [Fact]
    public void Switching_to_a_value_outside_the_catalog_is_a_programming_error()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var service = new LanguageSwitchingService(session, logOutput: captured);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Switch((InterfaceLanguage)99));

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Empty(captured.Logs);
    }

    [Fact]
    public void Each_session_has_its_own_identifier_for_log_correlation()
    {
        Assert.NotEqual(
            new ApplicationSession().SessionId,
            new ApplicationSession().SessionId);
    }

    [Fact]
    public void The_default_provider_loads_es_and_en_and_rejects_unregistered_codes()
    {
        var provider = I18nStringCatalogProvider.Instance;

        Assert.True(provider.TryLoad("es", out var spanish));
        Assert.Equal(I18nStringCatalog.TotalStrings, spanish.Count);
        Assert.True(provider.TryLoad(" EN ", out var english));
        Assert.Equal(I18nStringCatalog.TotalStrings, english.Count);

        // Case and whitespace do not count; empty or null falls back to the default language.
        Assert.True(provider.TryLoad("", out var empty));
        Assert.Equal(I18nStringCatalog.TotalStrings, empty.Count);
        Assert.True(provider.TryLoad(null!, out var nullTexts));
        Assert.Equal(I18nStringCatalog.TotalStrings, nullTexts.Count);

        // An unregistered code does not load: the switching is aborted (ERR-003).
        Assert.False(provider.TryLoad("fr", out var absent));
        Assert.Empty(absent);
    }

    /// <summary>Log output that captures what is emitted for its verification.</summary>
    private sealed class CapturedOutput : ILogOutput
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public void Write(LogLevel level, string message) =>
            Logs.Add((level, message));
    }

    /// <summary>
    /// Test text provider: full es/en catalog by default, with load failures
    /// and partial catalogs simulatable.
    /// </summary>
    private sealed class TestProvider : II18nTextProvider
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyTexts =
            new Dictionary<string, string>();

        private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _textsByCode;
        private readonly HashSet<string> _failingCodes = [];

        public TestProvider()
        {
            _textsByCode = new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["es"] = I18nStringCatalog.TextsByLanguage[I18nStringCatalog.DefaultCode],
                ["en"] = I18nStringCatalog.TextsByLanguage["en"],
            };
        }

        /// <summary>Forces the given language code not to load (ERR-003).</summary>
        public void SimulateFailure(string code) => _failingCodes.Add(code);

        /// <summary>Stops forcing the failure of the given language code.</summary>
        public void Allow(string code) => _failingCodes.Remove(code);

        /// <summary>Replaces the texts of the given language code.</summary>
        public void Register(string code, IReadOnlyDictionary<string, string> texts) =>
            _textsByCode[code] = texts;

        public bool TryLoad(string languageCode, out IReadOnlyDictionary<string, string> texts)
        {
            var code = languageCode?.Trim().ToLowerInvariant() ?? I18nStringCatalog.DefaultCode;

            if (_failingCodes.Contains(code)
                || !_textsByCode.TryGetValue(code, out var found))
            {
                texts = EmptyTexts;
                return false;
            }

            texts = found;
            return true;
        }
    }
}
