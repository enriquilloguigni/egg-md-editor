using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;

namespace EggMdEditor.App;

public partial class MainWindow : Window
{
    private readonly ApplicationSession _session;
    private readonly SessionShutdownManager _shutdownManager;

    /// <summary>
    /// Parameterless constructor for the XAML designer and for the startup:
    /// creates the session at its startup default (Spanish language, BR-007)
    /// with the default services.
    /// </summary>
    public MainWindow() : this(new ApplicationSession())
    {
    }

    /// <summary>
    /// Creates the application's single window (BR-003) with its persistent
    /// menu bar at 100 % of FR-011 (T-008, T-010), its format bar at 100 % of
    /// FR-012 (T-009) and its empty document area (AC-015), and wires the
    /// internal channels COM-001–COM-004 into it (T-011):
    ///
    /// <list type="bullet">
    /// <item>COM-001 — hot relabeling (DS-003): single subscription to the
    /// language switching service notification (T-004) that re-resolves every
    /// visible string from the active language already applied to the session;
    /// the menu's language entry (T-010) switches on pressing each value and
    /// restores its radio check if the switching does not become effective
    /// (already active or aborted, ERR-003).</item>
    /// <item>COM-002 — shortcuts (T-005): the window's keyboard translates
    /// each gesture to the combination model and dispatches it; the
    /// dispatcher's single register assigns <c>Ctrl/Cmd+Shift+L</c> to
    /// toggling es/en and <c>Ctrl/Cmd+Q</c> to closing, and any other shortcut
    /// remains inert (BR-F001-003, ADR-001).</item>
    /// <item>COM-003 — visual state (T-009, T-010): the mirror reflects onto
    /// the session the menu state {Closed | Expanded with active entry} and
    /// the format bar tooltip visibility.</item>
    /// <item>COM-004 — close: the close shortcut and the window close through
    /// the same path; on closing, the session is released with its LOG-003
    /// log (T-007) and the internal channels cease; on close failure, ERR-005
    /// with error exit code.</item>
    /// </list>
    ///
    /// The services are injectable for deterministic wiring in tests; by
    /// default they are created over the given session, with the log output
    /// formatted to the process's standard output (T-007, C-001).
    /// </summary>
    public MainWindow(
        ApplicationSession session,
        LanguageSwitchingService? languageService = null,
        ShortcutDispatcher? shortcutDispatcher = null,
        SecondWindowTrigger? secondWindowTrigger = null,
        LifecycleLogger? lifecycleLogger = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        _session = session;

        InitializeComponent();

        MainMenuBuilder.Build(MenuBar, session);
        FormatBarBuilder.Build(FormatBar, session);

        // COM-001 (T-004): switching service with the process's log output
        // (LOG-002) and single relabeling notification (DS-003).
        LanguageService = languageService ?? new LanguageSwitchingService(
            session,
            I18nStringCatalogProvider.Instance,
            new ConsoleLogOutput());
        LanguageService.LanguageSwitched += OnLanguageSwitched;
        WireLanguageEntry();

        // COM-003: visual state mirror of menu and tooltips (VR-002).
        new VisualStateMirror(session).Connect(MenuBar, FormatBar);

        // COM-004 (T-007): window close with session release and LOG-003
        // logs; the dispatcher's close shortcut (COM-002) closes through this
        // same path.
        _shutdownManager = new SessionShutdownManager(session, lifecycleLogger);
        ShortcutDispatcher = shortcutDispatcher
            ?? new ShortcutDispatcher(LanguageService, new WindowCloseAction(this));
        AddHandler(InputElement.KeyDownEvent, OnKeyPressed, RoutingStrategies.Tunnel);
        Closed += OnWindowClosed;

        // T-006: focus counterpart of the single window, wired over this
        // window; the trigger attends the request without creating another.
        SecondWindowTrigger =
            secondWindowTrigger ?? new SecondWindowTrigger(new WindowFocuser(this));
    }

    /// <summary>Language switching service (COM-001) wired to this window.</summary>
    public LanguageSwitchingService LanguageService { get; }

    /// <summary>Shortcut dispatcher (COM-002) wired to this window.</summary>
    public ShortcutDispatcher ShortcutDispatcher { get; }

    /// <summary>
    /// Trigger of the second window request (AC-014, T-006) with this window's
    /// focus counterpart: the process's entry point passes it the request and
    /// the existing single window receives the focus, without another one
    /// being created (BR-003).
    /// </summary>
    public SecondWindowTrigger SecondWindowTrigger { get; }

    /// <summary>
    /// COM-001 (DS-003): hot relabeling. The notification arrives after
    /// applying the new language to the session —the observable source—, so
    /// every visible string is re-resolved from the already-applied state,
    /// with no restart and keeping the rest of the state (BR-F001-005).
    /// </summary>
    private void OnLanguageSwitched(object? sender, LanguageSwitchedEventArgs e) =>
        UiRelabeler.Relabel(MenuBar, FormatBar, _session.ActiveLanguage);

    /// <summary>
    /// COM-001 from the menu (T-010): pressing a language selection value
    /// switches to the chosen language. If the switching does not become
    /// effective —already active language or operation aborted by an
    /// unavailable catalog (ERR-003)—, the session's active language did not
    /// change and the radio check is restored from the state; if effective,
    /// the relabeling arrives with the single notification (DS-003) and the
    /// check with it (the relabeling re-checks the selection from the state).
    /// </summary>
    private void SwitchFromMenu(InterfaceLanguage targetLanguage)
    {
        var result = LanguageService.Switch(targetLanguage);

        if (result.State != LanguageSwitchState.Switched)
        {
            UiRelabeler.Relabel(MenuBar, FormatBar, _session.ActiveLanguage);
        }
    }

    /// <summary>
    /// Connects the language selection values of the Tools → Interface
    /// language entry (T-010) to the COM-001 channel: each value, identified
    /// by its catalog key, switches to its language.
    /// </summary>
    private void WireLanguageEntry()
    {
        foreach (var item in EnumerateMenuItems(MenuBar))
        {
            if (item.Tag is not string key)
            {
                continue;
            }

            // Only the language values (T-010) switch when pressed. The
            // match is exact with the catalog values: any other entry stays
            // with no connected action (ADR-001) — never switches to a value
            // outside the catalog (BR-007).
            foreach (var language in LanguageCatalog.Values)
            {
                if (MainMenuBuilder.LanguageValueKey(language) == key)
                {
                    var valueLanguage = language;
                    item.Click += (_, _) => SwitchFromMenu(valueLanguage);
                    break;
                }
            }
        }
    }

    /// <summary>COM-002: translates the gesture to the model and dispatches it (T-005). Non-expressible gestures —no letter nor digit— and unassigned combinations remain inert (BR-F001-003, ADR-001).</summary>
    private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        var combination = GestureTranslator.Translate(e.Key, e.KeyModifiers);

        if (combination is null)
        {
            return;
        }

        var result = ShortcutDispatcher.Dispatch(combination.Value);

        if (result.State == ShortcutDispatchState.Dispatched)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// COM-004: the window was closed —by the close shortcut (COM-002), the
    /// system order or the user's close—. The window's internal channels
    /// cease and the session is released with its LOG-003 log (T-007); on a
    /// close or release failure, ERR-005: the process is left with the error
    /// exit code to terminate anyway, with no residual processes.
    /// </summary>
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        LanguageService.LanguageSwitched -= OnLanguageSwitched;
        RemoveHandler(InputElement.KeyDownEvent, OnKeyPressed);

        try
        {
            _shutdownManager.Shutdown();
        }
        catch (Exception)
        {
            // ERR-005 already logged by the manager (Spanish message through
            // the error output and LOG-003 with Error level): the shutdown
            // continues to the end and the process terminates with an error
            // code.
            Environment.ExitCode = _shutdownManager.PendingExitCode;
        }
    }

    private static IEnumerable<MenuItem> EnumerateMenuItems(Menu bar) =>
        bar.Items.OfType<MenuItem>().SelectMany(EnumerateMenuItems);

    private static IEnumerable<MenuItem> EnumerateMenuItems(MenuItem item)
    {
        yield return item;

        foreach (var child in item.Items.OfType<MenuItem>())
        {
            foreach (var element in EnumerateMenuItems(child))
            {
                yield return element;
            }
        }
    }

    /// <summary>Close counterpart (COM-004) over the window: closing it triggers the session shutdown.</summary>
    private sealed class WindowCloseAction(MainWindow window) : ICloseAction
    {
        public void Execute() => window.Close();
    }

    /// <summary>Focus counterpart (T-006) over the existing single window: bring it to the front with the focus, without creating another (BR-003).</summary>
    private sealed class WindowFocuser(MainWindow window) : IWindowFocuser
    {
        public void Focus() => window.Activate();
    }
}
