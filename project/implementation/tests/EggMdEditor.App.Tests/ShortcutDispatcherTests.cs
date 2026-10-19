using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Shortcut dispatcher (COM-002): single register of the combinations assigned
// in the Plan —Ctrl+Shift+L (macOS Cmd+Shift+L) toggles es/en via COM-001 and
// Ctrl+Q (macOS Cmd+Q) closes via COM-004—, with any other shortcut inert
// (BR-F001-003, ADR-001) and no response for the close action while its
// counterpart is not connected (COM-002).
public class ShortcutDispatcherTests
{
    private static readonly KeyCombination ToggleLanguage =
        new(ShortcutModifiers.Primary | ShortcutModifiers.Shift, 'L');

    private static readonly KeyCombination CloseApplication =
        new(ShortcutModifiers.Primary, 'Q');

    [Fact]
    public void The_single_register_contains_exactly_the_combinations_assigned_in_the_plan()
    {
        var register = ShortcutDispatcher.AssignedCombinations;

        // Single, centralized register: two assignments, with no duplicates.
        Assert.Equal(2, register.Count);

        // Ctrl/Cmd+Shift+L → toggle es/en: the primary modifier is the same on
        // Windows/Linux (Ctrl) and macOS (Cmd), so the Plan's form «Ctrl+Shift+L
        // (macOS Cmd+Shift+L)» is a single entry.
        Assert.True(register.TryGetValue(ToggleLanguage, out var languageAction));
        Assert.Equal(ShortcutAction.ToggleLanguage, languageAction);

        // Ctrl/Cmd+Q → close the application.
        Assert.True(register.TryGetValue(CloseApplication, out var closeAction));
        Assert.Equal(ShortcutAction.CloseApplication, closeAction);
    }

    [Fact]
    public void Dispatching_ctrl_shift_l_toggles_from_spanish_to_english_via_com001()
    {
        var session = new ApplicationSession
        {
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var languageService = new LanguageSwitchingService(session);
        var dispatcher = new ShortcutDispatcher(languageService);
        var notifications = new List<LanguageSwitchedEventArgs>();
        languageService.LanguageSwitched += (_, e) => notifications.Add(e);

        var result = dispatcher.Dispatch(ToggleLanguage);

        Assert.Equal(ShortcutDispatchState.Dispatched, result.State);
        Assert.Equal(ShortcutAction.ToggleLanguage, result.Action);
        Assert.NotNull(result.LanguageSwitch);
        Assert.Equal(LanguageSwitchState.Switched, result.LanguageSwitch!.State);
        Assert.Equal(InterfaceLanguage.Spanish, result.LanguageSwitch.SourceLanguage);
        Assert.Equal(InterfaceLanguage.English, result.LanguageSwitch.TargetLanguage);

        // COM-001 applied the change to the session with its single DS-003 notification.
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Single(notifications);
        Assert.Equal(InterfaceLanguage.Spanish, notifications[0].PreviousLanguage);
        Assert.Equal(InterfaceLanguage.English, notifications[0].NewLanguage);

        // BR-F001-005: the dispatch only touches the language via COM-001; the
        // rest of the session state remains intact.
        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);
    }

    [Fact]
    public void Dispatching_ctrl_shift_l_twice_toggles_and_returns_to_the_previous_language()
    {
        var session = new ApplicationSession();
        var dispatcher = new ShortcutDispatcher(new LanguageSwitchingService(session));

        dispatcher.Dispatch(ToggleLanguage);
        dispatcher.Dispatch(ToggleLanguage);

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
    }

    [Fact]
    public void Dispatching_ctrl_q_without_connected_counterpart_produces_no_response()
    {
        var session = new ApplicationSession();
        var dispatcher = new ShortcutDispatcher(new LanguageSwitchingService(session));

        var result = dispatcher.Dispatch(CloseApplication);

        // COM-002: assigned combination whose action does not exist yet — no
        // response (T-011 will connect the session finalization, COM-004).
        Assert.Equal(ShortcutDispatchState.NoActionConnected, result.State);
        Assert.Equal(ShortcutAction.CloseApplication, result.Action);
        Assert.Null(result.LanguageSwitch);

        // No observable effect on the session.
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal(MainMenuState.Closed, session.MainMenuState);
        Assert.False(session.IsToolTipVisible);
    }

    [Fact]
    public void Dispatching_ctrl_q_with_connected_counterpart_invokes_it_once()
    {
        var session = new ApplicationSession();
        var close = new CapturedCloseAction();
        var dispatcher = new ShortcutDispatcher(
            new LanguageSwitchingService(session), closeAction: close);

        var result = dispatcher.Dispatch(CloseApplication);

        Assert.Equal(ShortcutDispatchState.Dispatched, result.State);
        Assert.Equal(ShortcutAction.CloseApplication, result.Action);
        Assert.Null(result.LanguageSwitch);
        Assert.Equal(1, close.Calls);
    }

    [Theory]
    [InlineData(ShortcutModifiers.None, 'L')]                              // without primary
    [InlineData(ShortcutModifiers.Shift, 'L')]                             // Shift+L without primary
    [InlineData(ShortcutModifiers.Primary, 'L')]                           // missing Shift
    [InlineData(ShortcutModifiers.Primary | ShortcutModifiers.Alt, 'L')]
    [InlineData(ShortcutModifiers.Primary | ShortcutModifiers.Alt, 'Q')]
    [InlineData(ShortcutModifiers.None, 'Q')]                              // Q without primary
    [InlineData(ShortcutModifiers.Primary, 'B')]                           // future F-028
    [InlineData(ShortcutModifiers.Primary, '8')]                           // unassigned digit
    public void Unassigned_shortcuts_remain_inert(
        ShortcutModifiers modifiers, char key)
    {
        var session = new ApplicationSession();
        var close = new CapturedCloseAction();
        var dispatcher = new ShortcutDispatcher(
            new LanguageSwitchingService(session), closeAction: close);

        var result = dispatcher.Dispatch(modifiers, key);

        // BR-F001-003 and ADR-001: no observable effect and no response.
        Assert.Equal(ShortcutDispatchState.Inert, result.State);
        Assert.Null(result.Action);
        Assert.Null(result.LanguageSwitch);
        Assert.Equal(0, close.Calls);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
    }

    [Fact]
    public void The_combination_is_normalized_in_case_and_modifiers()
    {
        var session = new ApplicationSession();
        var dispatcher = new ShortcutDispatcher(new LanguageSwitchingService(session));

        // Lowercase key and a modifier bit outside the model: the normalized
        // combination matches the assigned one.
        var modifiersWithNoise =
            (ShortcutModifiers)8 | ShortcutModifiers.Primary | ShortcutModifiers.Shift;

        var result = dispatcher.Dispatch(modifiersWithNoise, 'l');

        Assert.Equal(ShortcutDispatchState.Dispatched, result.State);
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
    }

    [Fact]
    public void Toggling_with_unavailable_catalog_aborts_without_language_change()
    {
        var session = new ApplicationSession();
        var languageService = new LanguageSwitchingService(
            session, textProvider: new FailingProvider());
        var dispatcher = new ShortcutDispatcher(languageService);
        var notifications = new List<LanguageSwitchedEventArgs>();
        languageService.LanguageSwitched += (_, e) => notifications.Add(e);

        var result = dispatcher.Dispatch(ToggleLanguage);

        // The dispatch happened; the failure handling belongs to COM-001
        // (ERR-003): aborted with no partial change nor notification.
        Assert.Equal(ShortcutDispatchState.Dispatched, result.State);
        Assert.Equal(ShortcutAction.ToggleLanguage, result.Action);
        Assert.Equal(
            LanguageSwitchState.AbortedCatalogUnavailable,
            result.LanguageSwitch!.State);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Empty(notifications);
    }

    [Fact]
    public void Dispatch_does_not_accept_a_missing_language_service()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ShortcutDispatcher(null!));
    }

    [Fact]
    public void The_combination_is_represented_in_the_plan_format()
    {
        Assert.Equal("Ctrl/Cmd+Shift+L", ToggleLanguage.ToString());
        Assert.Equal("Ctrl/Cmd+Q", CloseApplication.ToString());
    }

    /// <summary>Test close counterpart: counts its invocations.</summary>
    private sealed class CapturedCloseAction : ICloseAction
    {
        public int Calls { get; private set; }

        public void Execute() => Calls++;
    }

    /// <summary>Test provider: no language catalog loads (ERR-003).</summary>
    private sealed class FailingProvider : II18nTextProvider
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyTexts =
            new Dictionary<string, string>();

        public bool TryLoad(string languageCode, out IReadOnlyDictionary<string, string> texts)
        {
            texts = EmptyTexts;
            return false;
        }
    }
}
