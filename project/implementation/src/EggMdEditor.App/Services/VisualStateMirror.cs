using Avalonia.Controls;
using Avalonia.Interactivity;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Visual state of SC-001's menu and format bar (COM-003, T-011): mirrors
/// onto the session —the observable source the window paints— the transient
/// state of the surface {<see cref="MainMenuState"/>: Closed | Expanded with
/// active entry} and the tooltip {visible | hidden}, from the pointer events
/// over the menu bar and the format bar.
///
/// <para>
/// Mirror with no functional semantics: none of these events emit towards an
/// action or alter the document (ADR-001) and it is not functional session
/// state (states-transitions.md) — only the reflection COM-003 declares as
/// destination. The appearance (tooltips VR-002 over the format bar, drop-down
/// menu) is painted by Avalonia; this mirror keeps the session state up to
/// date with it. The menu state goes to «Expanded with active entry» when the
/// pointer enters a menu entry and back to «Closed» when it leaves; the
/// tooltip stays visible while the pointer remains over a format bar control
/// and hidden when it leaves.
/// </para>
/// </summary>
public sealed class VisualStateMirror
{
    private readonly ApplicationSession _session;

    /// <summary>
    /// Creates the mirror over the application session, whose visual state
    /// fields it updates.
    /// </summary>
    public VisualStateMirror(ApplicationSession session) =>
        _session = session ?? throw new ArgumentNullException(nameof(session));

    /// <summary>
    /// Connects the pointer events of SC-001's menu bar and format bar to the
    /// session state: each menu entry reports the pointer entering and
    /// leaving it, and each bar control, its own. The events arrive on the UI
    /// thread, synchronous and in process (COM-003).
    /// </summary>
    public void Connect(Menu menuBar, StackPanel formatBar)
    {
        ArgumentNullException.ThrowIfNull(menuBar);
        ArgumentNullException.ThrowIfNull(formatBar);

        foreach (var item in menuBar.Items.OfType<MenuItem>())
        {
            ConnectMenuEntry(item);
        }

        foreach (var button in formatBar.Children.OfType<Button>())
        {
            button.PointerEntered += OnPointerEnteredFormatBarControl;
            button.PointerExited += OnPointerExitedFormatBarControl;
        }
    }

    /// <summary>
    /// The pointer enters a menu entry: the entry becomes active and the
    /// menu, expanded with active entry.
    /// </summary>
    public void OnMenuEntryEntered(MenuItem entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _session.MainMenuState = MainMenuState.ExpandedWithActiveEntry;
    }

    /// <summary>
    /// The pointer leaves a menu entry: with no active entry, the menu is
    /// closed again.
    /// </summary>
    public void OnMenuEntryExited(MenuItem entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _session.MainMenuState = MainMenuState.Closed;
    }

    /// <summary>
    /// The pointer enters a format bar control: its tooltip becomes visible
    /// (VR-002).
    /// </summary>
    public void OnFormatBarControlEntered(Button control)
    {
        ArgumentNullException.ThrowIfNull(control);

        _session.IsToolTipVisible = true;
    }

    /// <summary>
    /// The pointer leaves a format bar control: the tooltip becomes hidden
    /// (VR-002).
    /// </summary>
    public void OnFormatBarControlExited(Button control)
    {
        ArgumentNullException.ThrowIfNull(control);

        _session.IsToolTipVisible = false;
    }

    private void ConnectMenuEntry(MenuItem item)
    {
        item.PointerEnteredItem += OnPointerEnteredMenuEntry;
        item.PointerExitedItem += OnPointerExitedMenuEntry;

        foreach (var child in item.Items.OfType<MenuItem>())
        {
            ConnectMenuEntry(child);
        }
    }

    private void OnPointerEnteredMenuEntry(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem entry)
        {
            OnMenuEntryEntered(entry);
        }
    }

    private void OnPointerExitedMenuEntry(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem entry)
        {
            OnMenuEntryExited(entry);
        }
    }

    private void OnPointerEnteredFormatBarControl(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (sender is Button control)
        {
            OnFormatBarControlEntered(control);
        }
    }

    private void OnPointerExitedFormatBarControl(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (sender is Button control)
        {
            OnFormatBarControlExited(control);
        }
    }
}
