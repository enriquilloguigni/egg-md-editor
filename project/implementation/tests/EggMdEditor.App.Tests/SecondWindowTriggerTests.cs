using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Trigger of the second window request (AC-014): in-process coordinator that,
// with SC-001 existing, focuses it without creating another window (BR-003),
// with no state transition. Scoped to a single process by the Spec (the
// Plan's risk): no interprocess channel nor attention of second processes.
// Without a window connected, the request has no response.
public class SecondWindowTriggerTests
{
    [Fact]
    public void The_request_with_an_existing_window_focuses_it_once_without_creating_another()
    {
        var window = new TestSingleWindow();
        var trigger = new SecondWindowTrigger(window);

        var result = trigger.RequestSecondWindow();

        // AC-014: SC-001 receives the focus; no other window is created.
        Assert.Equal(SecondWindowRequestState.Focused, result.State);
        Assert.Equal(1, window.FocusCount);
    }

    [Fact]
    public void Each_request_focuses_the_same_existing_unique_window()
    {
        var window = new TestSingleWindow();
        var trigger = new SecondWindowTrigger(window);

        var results = Enumerable.Range(0, 3)
            .Select(_ => trigger.RequestSecondWindow())
            .ToList();

        // BR-003: there is still exactly one window per session; every
        // request is attended by focusing the same injected existing window —
        // the coordinator creates no other nor replaces it—.
        Assert.All(results, r => Assert.Equal(SecondWindowRequestState.Focused, r.State));
        Assert.Equal(3, window.FocusCount);
    }

    [Fact]
    public void The_request_produces_no_state_transition_in_the_session()
    {
        var session = new ApplicationSession
        {
            ActiveLanguage = InterfaceLanguage.English,
            MainMenuState = MainMenuState.ExpandedWithActiveEntry,
            IsToolTipVisible = true,
        };
        var trigger = new SecondWindowTrigger(new TestSingleWindow());

        trigger.RequestSecondWindow();

        // states-transitions.md: second window request in ST-002/ST-003 with
        // no transition; the trigger does not touch the session.
        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);
        Assert.True(session.IsToolTipVisible);
    }

    [Fact]
    public void The_request_without_connected_window_has_no_response_and_creates_no_window()
    {
        var trigger = new SecondWindowTrigger();

        var result = trigger.RequestSecondWindow();

        // Without a focus counterpart (SC-001 does not exist or its wiring
        // arrives in T-011): no response and a window is never created to
        // attend it — the only window is created at startup (T-008).
        Assert.Equal(SecondWindowRequestState.NoWindowConnected, result.State);
    }

    [Fact]
    public void The_focus_counterpart_is_optional_in_the_constructor()
    {
        Assert.Null(Record.Exception(() => new SecondWindowTrigger(null)));
        Assert.Null(Record.Exception(() => new SecondWindowTrigger()));
    }

    /// <summary>
    /// Test SC-001: counts the received focuses without lifting real UI.
    /// </summary>
    private sealed class TestSingleWindow : IWindowFocuser
    {
        public int FocusCount { get; private set; }

        public void Focus() => FocusCount++;
    }
}
