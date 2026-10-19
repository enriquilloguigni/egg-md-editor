using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Headless;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Wiring of COM-003 in SC-001 (T-011): the surface's transient state is
// mirrored onto the session the window paints — menu {Closed | Expanded with
// active entry} and tooltip {visible | hidden} — with no emission at all
// towards actions and no effect on the document (ADR-001): surface states,
// not functional ones (states-transitions.md).
public class VisualStateWiringTests
{
    // --- Contract of the mirror (reflection of the session state) ---

    [Fact]
    public void Entering_a_menu_entry_marks_it_expanded_and_leaving_closes()
    {
        var session = new ApplicationSession();
        var mirror = new VisualStateMirror(session);

        mirror.OnMenuEntryEntered(new MenuItem());

        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);

        mirror.OnMenuEntryExited(new MenuItem());

        Assert.Equal(MainMenuState.Closed, session.MainMenuState);
    }

    [Fact]
    public void Entering_a_bar_control_marks_the_tooltip_visible_and_leaving_hides_it()
    {
        var session = new ApplicationSession();
        var mirror = new VisualStateMirror(session);

        mirror.OnFormatBarControlEntered(new Button());

        Assert.True(session.IsToolTipVisible);

        mirror.OnFormatBarControlExited(new Button());

        Assert.False(session.IsToolTipVisible);
    }

    // --- Wiring of SC-001's pointer events (COM-003) ---

    [Fact]
    public void The_pointer_over_a_menu_entry_marks_the_session_state()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();

        var file = window.FindControl<Menu>("MenuBar")!.Items.OfType<MenuItem>().First();
        window.MouseMove(PointAt(window, file), RawInputModifiers.None);

        Assert.Equal(MainMenuState.ExpandedWithActiveEntry, session.MainMenuState);

        // When the pointer leaves the bar, the menu is closed again.
        window.MouseMove(new Point(600, 400), RawInputModifiers.None);

        Assert.Equal(MainMenuState.Closed, session.MainMenuState);
    }

    [Fact]
    public void The_pointer_over_a_bar_control_marks_the_tooltip_in_the_session()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();

        var button = window.FindControl<StackPanel>("FormatBar")!.Children.OfType<Button>().First();
        window.MouseMove(PointAt(window, button), RawInputModifiers.None);

        Assert.True(session.IsToolTipVisible);

        window.MouseMove(new Point(600, 400), RawInputModifiers.None);

        Assert.False(session.IsToolTipVisible);
    }

    [Fact]
    public void The_visual_interaction_does_not_alter_the_language_nor_publishes_switching_events()
    {
        // COM-003: no menu or bar event emits towards an action (ADR-001);
        // the only effect is the surface state.
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();
        var switchings = 0;
        window.LanguageService.LanguageSwitched += (_, _) => switchings++;

        window.MouseMove(
            PointAt(window, window.FindControl<StackPanel>("FormatBar")!.Children.OfType<Button>().First()),
            RawInputModifiers.None);

        Assert.Equal(0, switchings);
        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
    }

    // --- Helpers ---

    private static Point PointAt(MainWindow window, Control control)
    {
        var controlCenter = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
        return control.TranslatePoint(controlCenter, window)
            ?? throw new InvalidOperationException("The control cannot be located over the window.");
    }
}
