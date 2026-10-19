using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Wiring of the single window's focus counterpart (T-006 → T-011): the second
// window request reaching the window's trigger is attended by focusing the
// existing SC-001 —exactly one window per session, BR-003, AC-014—, without
// creating another and with no state transition.
public class SecondWindowWiringTests
{
    [Fact]
    public void The_second_window_request_is_attended_by_focusing_the_existing_window()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();

        var result = window.SecondWindowTrigger.RequestSecondWindow();

        // The counterpart wired over this window received the request:
        // exactly one focus invocation, without creating another window
        // (BR-003, AC-014) and with no state transition in the session.
        Assert.Equal(SecondWindowRequestState.Focused, result.State);
        Assert.True(window.IsVisible);
        Assert.Equal(LanguageCatalog.Default, session.ActiveLanguage);
    }

    [Fact]
    public void The_default_startup_exposes_the_window_with_its_session_and_services_wired()
    {
        // The parameterless constructor (startup, T-012) creates the session
        // at its default —Spanish, BR-007— and the channels over it.
        var window = new MainWindow();

        Assert.NotNull(window.LanguageService);
        Assert.NotNull(window.ShortcutDispatcher);
        Assert.NotNull(window.SecondWindowTrigger);
        Assert.Equal(SecondWindowRequestState.Focused,
            window.SecondWindowTrigger.RequestSecondWindow().State);
    }
}
