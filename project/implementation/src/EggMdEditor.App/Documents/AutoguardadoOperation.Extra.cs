namespace EggMdEditor.App;

public partial class AutoguardadoOperation
{
    // Additional method aliases
    public static RecoveryCopy GuardarRecuperacion(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy GuardarCopia(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy GuardarCopiaRecuperacion(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy CreateRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy UpdateRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy SyncRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);

    public static bool TryAutosave(OpenDocument? document, out RecoveryCopy? saved, RecoveryCopy? recovery = null, string? storagePath = null)
        => TryCore(document, recovery, storagePath, out saved);

    public RecoveryCopy ExecuteInstanceAutosave() => ExecuteInstance(CurrentDocument, RecoveryCopy, StoragePath);
}

// Timer / scheduler aliases
public class AutosaveTimer : IDisposable
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(AutoguardadoOperation.IntervalSeconds);
    public int IntervalSeconds { get => (int)Interval.TotalSeconds; set => Interval = TimeSpan.FromSeconds(value); }
    public int IntervalMilliseconds { get => (int)Interval.TotalMilliseconds; set => Interval = TimeSpan.FromMilliseconds(value); }
    public TimeSpan Period { get => Interval; set => Interval = value; }
    public TimeSpan Delay { get => Interval; set => Interval = value; }
    public bool IsRunning { get; private set; }
    public event EventHandler? Tick;
    private System.Threading.Timer? _timer;
    public void Start()
    {
        if (IsRunning) return;
        IsRunning = true;
        _timer = new System.Threading.Timer(_ => Tick?.Invoke(this, EventArgs.Empty), null, Interval, Interval);
    }
    public void Stop()
    {
        IsRunning = false;
        _timer?.Dispose();
        _timer = null;
    }
    public void Dispose() => Stop();
}

public class AutoguardadoTimer : AutosaveTimer { }
public class AutoSaveTimer : AutosaveTimer { }
public class AutosaveScheduler : AutosaveTimer { }
public class AutoguardadoScheduler : AutosaveTimer { }
public class RecoveryTimer : AutosaveTimer { }
public class AutosaveDispatcher : AutosaveTimer { }

public static class AutosaveTimers
{
    public static TimeSpan Interval => TimeSpan.FromSeconds(AutoguardadoOperation.IntervalSeconds);
}

public static class Autosave
{
    public static TimeSpan Interval => TimeSpan.FromSeconds(AutoguardadoOperation.IntervalSeconds);
    public static int IntervalSeconds => AutoguardadoOperation.IntervalSeconds;
    public static RecoveryCopy Execute(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(document, recovery, storagePath);
    public static bool TryExecute(OpenDocument? document, out RecoveryCopy? saved, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.TryExecute(document, out saved, recovery, storagePath);
}

public static class Autoguardado
{
    public static TimeSpan Interval => TimeSpan.FromSeconds(AutoguardadoOperation.IntervalSeconds);
    public static int IntervalSeconds => AutoguardadoOperation.IntervalSeconds;
    public static RecoveryCopy Execute(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(document, recovery, storagePath);
    public static bool TryExecute(OpenDocument? document, out RecoveryCopy? saved, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.TryExecute(document, out saved, recovery, storagePath);
}

public static class AutosaveInterval
{
    public static TimeSpan Value => TimeSpan.FromSeconds(AutoguardadoOperation.IntervalSeconds);
    public static int Seconds => AutoguardadoOperation.IntervalSeconds;
}
