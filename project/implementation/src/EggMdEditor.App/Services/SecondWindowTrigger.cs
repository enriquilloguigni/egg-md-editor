namespace EggMdEditor.App.Services;

/// <summary>
/// Focus counterpart of the single window SC-001 (BR-003): synchronous,
/// in-process order to bring the existing window to the front and give it the
/// focus. The real implementation over the Avalonia window is wired with the
/// rest of SC-001's channels (T-011); the trigger never creates windows, so
/// this abstraction only exposes focusing the one that already exists.
/// </summary>
public interface IWindowFocuser
{
    /// <summary>Gives the focus to the existing single window SC-001 (BR-003).</summary>
    void Focus();
}

/// <summary>
/// State of the attention of a second window request (AC-014).
/// </summary>
public enum SecondWindowRequestState
{
    /// <summary>
    /// The existing SC-001 received the focus: no window was created and
    /// there is still exactly one per session, with no state transition
    /// (AC-014, BR-003).
    /// </summary>
    Focused = 1,

    /// <summary>
    /// No window connected yet —SC-001 does not exist or its focus
    /// counterpart is not wired (T-011)—: no response; the trigger does not
    /// create any window to attend the request.
    /// </summary>
    NoWindowConnected = 2,
}

/// <summary>
/// Detailed result of the attention of a second window request (AC-014).
/// </summary>
/// <param name="State">Final state of the request.</param>
public sealed record SecondWindowRequestResult(
    SecondWindowRequestState State);

/// <summary>
/// Trigger of the second window request (AC-014): synchronous, in-process
/// coordinator that receives the request for a second window of the
/// application and attends it by focusing the existing single window SC-001,
/// without creating another (BR-003). Scoped to a single process by the Spec
/// (the Plan's risk): it does not attend second launched processes nor uses
/// any interprocess channel — the request reaches it within the process
/// itself, from SC-001's wiring (T-011). It does not touch the session: a
/// second window request while the session is in ST-002/ST-003 produces no
/// state transition (states-transitions.md) and generates no technical log
/// (logs.md defines no LOG-xxx for this internal event of SC-001). While the
/// focus counterpart is not connected, the request has no response and a
/// window is never created to attend it: creating the only window belongs to
/// the startup (T-008), never to this coordinator.
/// </summary>
public sealed class SecondWindowTrigger
{
    private readonly IWindowFocuser? _windowFocuser;

    /// <summary>
    /// Creates the trigger. SC-001's focus counterpart is injectable and by
    /// default is not connected: its implementation over the real window is
    /// wired with the rest of SC-001's channels (T-011).
    /// </summary>
    public SecondWindowTrigger(IWindowFocuser? windowFocuser = null)
    {
        _windowFocuser = windowFocuser;
    }

    /// <summary>
    /// Attends a second window request of the application (AC-014): with
    /// SC-001 existing, gives it the focus —exactly one invocation per
    /// request— and does not create another window, so the session keeps
    /// exactly one (BR-003) with no state transition. Without a window
    /// connected, it produces no response.
    /// </summary>
    public SecondWindowRequestResult RequestSecondWindow()
    {
        if (_windowFocuser is null)
        {
            return new SecondWindowRequestResult(
                SecondWindowRequestState.NoWindowConnected);
        }

        _windowFocuser.Focus();

        return new SecondWindowRequestResult(
            SecondWindowRequestState.Focused);
    }
}
