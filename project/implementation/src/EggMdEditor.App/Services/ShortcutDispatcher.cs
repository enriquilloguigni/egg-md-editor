using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Actions connectable to a shortcut in F-001, per the Plan: toggle the
/// interface language (via COM-001) and close the application (via COM-004).
/// The combination → action assignment lives only in the dispatcher's single
/// register; reviewing it when connecting each new action corresponds to
/// F-028/F-029.
/// </summary>
public enum ShortcutAction
{
    /// <summary>Toggles the interface language between Español and English (COM-001, BR-F001-005).</summary>
    ToggleLanguage = 1,

    /// <summary>Closes the application: end of the session (COM-004).</summary>
    CloseApplication = 2,
}

/// <summary>
/// Counterpart of the close shortcut (COM-004): close order that ends the
/// application session releasing its data, synchronous and in process. The
/// finalization logic and its wiring are implemented with the closing of
/// SC-001 (T-011): the dispatcher only invokes it if it is connected and,
/// while it is not, the <c>Ctrl/Cmd+Q</c> shortcut produces no response
/// (COM-002, ADR-001).
/// </summary>
public interface ICloseAction
{
    /// <summary>Executes the application close (COM-004).</summary>
    void Execute();
}

/// <summary>
/// State of the dispatch of a keyboard gesture (COM-002): the action was
/// executed if the combination is assigned and its action is connected; there
/// is no response if the combination is assigned but its action does not
/// exist yet; and the gesture is inert if its combination is not in the
/// single register (BR-F001-003, ADR-001).
/// </summary>
public enum ShortcutDispatchState
{
    /// <summary>Combination assigned in the single register and action connected: the action was executed.</summary>
    Dispatched = 1,

    /// <summary>Assigned combination whose action is not connected yet: no response (COM-002, ADR-001).</summary>
    NoActionConnected = 2,

    /// <summary>Unassigned combination: inert shortcut, no observable effect (BR-F001-003, ADR-001).</summary>
    Inert = 3,
}

/// <summary>
/// Detailed result of the dispatch of a keyboard gesture (COM-002).
/// </summary>
/// <param name="State">State of the dispatch.</param>
/// <param name="Action">Action assigned to the combination; null if the shortcut is inert.</param>
/// <param name="LanguageSwitch">
/// Result of the COM-001 channel when the action was
/// <see cref="ShortcutAction.ToggleLanguage"/> — including a switching
/// aborted by an unavailable catalog, whose handling belongs to
/// COM-001 (ERR-003, LOG-002)—; null in the other cases.
/// </param>
public sealed record ShortcutDispatchResult(
    ShortcutDispatchState State,
    ShortcutAction? Action,
    LanguageSwitchResult? LanguageSwitch);

/// <summary>
/// Shortcut dispatcher (COM-002): synchronous, in-process channel that
/// receives the keyboard gesture of SC-001 and executes the action assigned
/// to its key combination. The combination register is unique and centralized
/// in this class (the Plan's risk): it assigns <c>Ctrl+Shift+L</c> —its macOS
/// form is <c>Cmd+Shift+L</c>, the same logical combination over the primary
/// modifier— to toggling es/en via COM-001, and <c>Ctrl+Q</c> —macOS form
/// <c>Cmd+Q</c>— to closing the application via COM-004; any other
/// combination remains inert (BR-F001-003, ADR-001). The dispatcher keeps no
/// state of its own and emits no logs: the only observable effect of a
/// dispatch is that of the connected action receiving it (the LOG-002 of
/// COM-001, or the finalization one when the close is implemented). The
/// gesture arrives already translated from the physical keyboard; translating
/// keys to modifiers and main key is the responsibility of the window wiring
/// (T-011).
/// </summary>
public sealed class ShortcutDispatcher
{
    /// <summary>
    /// Single register of the combinations assigned in F-001 (the Plan's
    /// risk): the only source of the combination → action assignment,
    /// reviewed when connecting each future action (F-028/F-029).
    /// </summary>
    private static readonly IReadOnlyDictionary<KeyCombination, ShortcutAction> RegisteredCombinations =
        new Dictionary<KeyCombination, ShortcutAction>
        {
            [new(ShortcutModifiers.Primary | ShortcutModifiers.Shift, 'L')] =
                ShortcutAction.ToggleLanguage,
            [new(ShortcutModifiers.Primary, 'Q')] =
                ShortcutAction.CloseApplication,
        };

    private readonly LanguageSwitchingService _languageService;
    private readonly ICloseAction? _closeAction;

    /// <summary>
    /// Creates the dispatcher over the language switching service (COM-001,
    /// T-004). The close action (COM-004) is injectable and by default is not
    /// connected: its session finalization logic is implemented later (T-011).
    /// </summary>
    public ShortcutDispatcher(
        LanguageSwitchingService languageService,
        ICloseAction? closeAction = null)
    {
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _closeAction = closeAction;
    }

    /// <summary>
    /// Single register of assigned combinations, exposed for review when
    /// connecting each new action (F-028/F-029): normalized keys and no
    /// duplicates by construction.
    /// </summary>
    public static IReadOnlyDictionary<KeyCombination, ShortcutAction> AssignedCombinations =>
        RegisteredCombinations;

    /// <summary>
    /// Receives a keyboard gesture and dispatches the action assigned to its
    /// combination (COM-002): toggles the language via COM-001, invokes the
    /// close counterpart if connected, or produces no effect at all, whether
    /// the combination is not assigned (inert shortcut, BR-F001-003) or its
    /// action does not exist yet (no response, ADR-001).
    /// </summary>
    public ShortcutDispatchResult Dispatch(KeyCombination combination)
    {
        if (!RegisteredCombinations.TryGetValue(combination, out var action))
        {
            return new ShortcutDispatchResult(ShortcutDispatchState.Inert, null, null);
        }

        return action switch
        {
            ShortcutAction.ToggleLanguage => ToggleLanguage(),
            ShortcutAction.CloseApplication => CloseApplication(),

            // Unreachable while the register only contains catalog actions; a
            // foreign value would be a programming error.
            _ => throw new InvalidOperationException(
                "The single combination register contains an unrecognized shortcut action."),
        };
    }

    /// <summary>
    /// Convenience overload for the keyboard wiring (T-011): normalizes the
    /// received gesture —modifiers reduced to the model and invariantly
    /// uppercase key— and dispatches.
    /// </summary>
    public ShortcutDispatchResult Dispatch(ShortcutModifiers modifiers, char key) =>
        Dispatch(KeyCombination.From(modifiers, key));

    /// <summary>
    /// Toggles the interface language (COM-001): with exactly two mutually
    /// exclusive languages (BR-007), the toggle target is the one different
    /// from the active one. The switching and its single relabeling
    /// notification (DS-003) belong entirely to COM-001; the result travels
    /// in the dispatch for the wiring (T-011).
    /// </summary>
    private ShortcutDispatchResult ToggleLanguage()
    {
        var activeLanguage = _languageService.ActiveLanguage;
        var targetLanguage = LanguageCatalog.Values.Single(language => language != activeLanguage);
        var languageSwitch = _languageService.Switch(targetLanguage);

        return new ShortcutDispatchResult(
            ShortcutDispatchState.Dispatched, ShortcutAction.ToggleLanguage, languageSwitch);
    }

    /// <summary>
    /// Close of the application (COM-004): invokes the injected counterpart
    /// exactly once, or produces no response while it is not connected.
    /// </summary>
    private ShortcutDispatchResult CloseApplication()
    {
        if (_closeAction is null)
        {
            return new ShortcutDispatchResult(
                ShortcutDispatchState.NoActionConnected, ShortcutAction.CloseApplication, null);
        }

        _closeAction.Execute();

        return new ShortcutDispatchResult(
            ShortcutDispatchState.Dispatched, ShortcutAction.CloseApplication, null);
    }
}
