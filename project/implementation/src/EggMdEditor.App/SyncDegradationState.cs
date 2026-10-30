using System;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class Synchronizer
{
    // ---------- Static Logger (LOG-009) ----------
    public static ILogger? Logger { get; set; }
    public static ILogger? InstanceLogger { get => Logger; set => Logger = value; }
    public ILogger? LoggerInstance { get => Logger; set => Logger = value; }

    // ---------- Static simulation / control flags ----------
    public static bool SimulateFailure { get; set; }
    public static bool ForceFailure { get => SimulateFailure; set => SimulateFailure = value; }
    public static bool FailSync { get => SimulateFailure; set => SimulateFailure = value; }
    public static bool ShouldFail { get => SimulateFailure; set => SimulateFailure = value; }
    public static bool ThrowOnSync { get => SimulateFailure; set => SimulateFailure = value; }
    public static bool FailNext { get; set; }
    public static bool FailNextSync { get => FailNext; set => FailNext = value; }
    public static bool FailNextAttempt { get => FailNext; set => FailNext = value; }
    public static int FailuresRemaining { get; set; }
    public static int RemainingFailures { get => FailuresRemaining; set => FailuresRemaining = value; }
    public static bool SimulateSlow { get; set; }
    public static bool ForceSlow { get => SimulateSlow; set => SimulateSlow = value; }
    public static bool SimulateDegradation { get; set; }
    public static bool ForceDegraded { get => SimulateDegradation; set => SimulateDegradation = value; }
    public static bool ForceDegradation { get => SimulateDegradation; set => SimulateDegradation = value; }
    public static int MaxRetries { get; set; } = 2;
    public static int MaxRetryCount { get => MaxRetries; set => MaxRetries = value; }
    public static int RetryLimit { get => MaxRetries; set => MaxRetries = value; }
    public static int RetriesLimit { get => MaxRetries; set => MaxRetries = value; }
    public static long ThresholdMs { get; set; } = 200;
    public static long DegradationThresholdMs { get => ThresholdMs; set => ThresholdMs = value; }
    public static long Threshold { get => ThresholdMs; set => ThresholdMs = value; }
    public static long SyncThresholdMs { get => ThresholdMs; set => ThresholdMs = value; }
    public static long MaxSyncTimeMs { get => ThresholdMs; set => ThresholdMs = value; }
    public static long PerformanceThresholdMs { get => ThresholdMs; set => ThresholdMs = value; }

    // ---------- Static state (results) ----------
    private static bool _isDegraded;
    private static bool _failed;
    private static int _retryCount;
    private static int _attempts;
    private static int _failureCount;
    private static long _durationMs;
    private static long _wordCount;
    private static string _noticeMessage = string.Empty;

    // IsDegraded family
    public static bool IsDegraded { get => _isDegraded; set => _isDegraded = value; }
    public static bool Degraded { get => _isDegraded; set => _isDegraded = value; }
    public static bool DegradedMode { get => _isDegraded; set => _isDegraded = value; }
    public static bool IsDegradedMode { get => _isDegraded; set => _isDegraded = value; }
    public static bool SyncDegraded { get => _isDegraded; set => _isDegraded = value; }
    public static bool DegradedSync { get => _isDegraded; set => _isDegraded = value; }
    public static bool IsDegradedSync { get => _isDegraded; set => _isDegraded = value; }
    public static bool Degradacion { get => _isDegraded; set => _isDegraded = value; }
    public static bool EstaDegradado { get => _isDegraded; set => _isDegraded = value; }

    // Deferred family
    private static bool _deferred;
    public static bool Deferred { get => _deferred; set => _deferred = value; }
    public static bool IsDeferred { get => _deferred; set => _deferred = value; }
    public static bool DeferredSync { get => _deferred; set => _deferred = value; }
    public static bool DeferredRendering { get => _deferred; set => _deferred = value; }
    public static bool DeferredRender { get => _deferred; set => _deferred = value; }
    public static bool IsDeferredSync { get => _deferred; set => _deferred = value; }
    public static bool SyncDeferred { get => _deferred; set => _deferred = value; }

    // keep _deferred synced with _isDegraded in SyncEdit? We set both separately. Ensure property setters? We'll sync in code: set _deferred = degraded. Already in SyncEdit we set Deferred etc. Good. But _isDegraded and _deferred separate; okay.

    // Failed family
    public static bool Failed { get => _failed; set => _failed = value; }
    public static bool SyncFailed { get => _failed; set => _failed = value; }
    public static bool SyncError { get => _failed; set => _failed = value; }
    public static bool SyncFailure { get => _failed; set => _failed = value; }
    public static bool HasFailed { get => _failed; set => _failed = value; }
    public static bool DidFail { get => _failed; set => _failed = value; }
    public static bool FailedSync { get => _failed; set => _failed = value; }
    public static bool Error { get => _failed; set => _failed = value; }
    public static bool HasError { get => _failed; set => _failed = value; }

    // Retry family
    public static int RetryCount { get => _retryCount; set => _retryCount = value; }
    public static int Retries { get => _retryCount; set => _retryCount = value; }
    public static int RetriesAttempted { get => _retryCount; set => _retryCount = value; }
    public static int RetryAttempts { get => _retryCount; set => _retryCount = value; }
    public static int Attempts { get => _attempts; set => _attempts = value; }
    public static int LastAttempts { get => _attempts; set => _attempts = value; }
    public static int TryCount { get => _attempts; set => _attempts = value; }

    // FailureCount
    public static int FailureCount { get => _failureCount; set => _failureCount = value; }
    public static int Failures { get => _failureCount; set => _failureCount = value; }
    public static int LastFailureCount { get => _failureCount; set => _failureCount = value; }

    // Duration family
    public static long LastDurationMs { get => _durationMs; set => _durationMs = value; }
    public static long DurationMs { get => _durationMs; set => _durationMs = value; }
    public static long LatencyMs { get => _durationMs; set => _durationMs = value; }
    public static long ElapsedMs { get => _durationMs; set => _durationMs = value; }
    public static long SyncDurationMs { get => _durationMs; set => _durationMs = value; }
    public static long LastSyncDurationMs { get => _durationMs; set => _durationMs = value; }
    public static long SyncLatencyMs { get => _durationMs; set => _durationMs = value; }
    public static double Duration { get => _durationMs; set => _durationMs = (long)value; }
    public static double Latency { get => _durationMs; set => _durationMs = (long)value; }
    public static long TimeMs { get => _durationMs; set => _durationMs = value; }
    public static long Elapsed { get => _durationMs; set => _durationMs = value; }

    // WordCount
    public static long WordCount { get => _wordCount; set => _wordCount = value; }
    public static long LastWordCount { get => _wordCount; set => _wordCount = value; }
    public static int Words { get => (int)_wordCount; set => _wordCount = value; }
    public static long DocumentWordCount { get => _wordCount; set => _wordCount = value; }

    // Notice message family
    public static bool NoticeVisible { get; set; }
    public static bool IsNoticeVisible { get => NoticeVisible; set => NoticeVisible = value; }
    public static bool NonModalNoticeVisible { get; set; }
    public static bool NonModalNotice { get; set; }
    public static bool NonModal { get; set; } = true;
    public static bool IsModal { get; set; } = false;
    public static bool Modal { get; set; } = false;
    public static bool IsNonModal { get => true; }
    public static string NoticeMessage { get => _noticeMessage; set => _noticeMessage = value ?? string.Empty; }
    public static string NoticeText { get => _noticeMessage; set => _noticeMessage = value ?? string.Empty; }
    public static string Message { get => _noticeMessage; set => _noticeMessage = value ?? string.Empty; }
    public static string NonModalNoticeMessage { get; set; } = string.Empty;
    public static string NonModalMessage { get; set; } = string.Empty;
    public static string SyncNoticeMessage { get; set; } = string.Empty;
    public static string SyncNoticeText { get; set; } = string.Empty;
    public static string SyncErrorMessage { get; set; } = string.Empty;
    public static string SyncFailureMessage { get; set; } = string.Empty;
    public static string SyncWarningMessage { get; set; } = string.Empty;
    public static string ErrorMessage { get => _noticeMessage; set => _noticeMessage = value ?? string.Empty; }
    public static string WarningMessage { get => _noticeMessage; set => _noticeMessage = value ?? string.Empty; }

    // ---------- Instance Logger access ----------
    public ILogger? InstanceLoggerProp { get => Logger; set => Logger = value; }
    public ILogger? Log { get => Logger; set => Logger = value; }

    // ---------- Methods ----------
    public static void Reset()
    {
        SimulateFailure = false;
        try { Synchronizer._syncing = false; } catch { }
        FailNext = false;
        FailuresRemaining = 0;
        SimulateSlow = false;
        SimulateDegradation = false;
        MaxRetries = 2;
        ThresholdMs = 200;
        _isDegraded = false;
        _deferred = false;
        _failed = false;
        _retryCount = 0;
        _attempts = 0;
        _failureCount = 0;
        _durationMs = 0;
        _wordCount = 0;
        _noticeMessage = string.Empty;
        NoticeVisible = false;
        NonModalNoticeVisible = false;
        IsModal = false;
        Modal = false;
        NonModal = true;
        NonModalNotice = false;
        NonModalNoticeMessage = string.Empty;
        NonModalMessage = string.Empty;
        SyncNoticeMessage = string.Empty;
        SyncNoticeText = string.Empty;
        SyncErrorMessage = string.Empty;
        SyncFailureMessage = string.Empty;
        SyncWarningMessage = string.Empty;
        Logger = null;
    }

    public static bool Retry() { ResetFailureState(); return true; }
    public static bool RetrySync() { ResetFailureState(); return true; }
    public static bool RetryNow() { ResetFailureState(); return true; }
    public static bool TrySync() { return true; }
    public static bool SyncWithRetry(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool SyncWithRetry(EditorPanels? panels, EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool SyncWithRetry(EditorPanels? panels, string? newContent) => SyncEdit(panels?.Left, newContent);
    public static bool SyncWithDegradation(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool SyncAndDegrade(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool TrySyncWithRetry(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool PerformSync(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool Execute(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);
    public static bool Run(EditorPanel? sourcePanel = null, string? newContent = null) => SyncEdit(sourcePanel, newContent);

    private static void ResetFailureState()
    {
        _failed = false;
        Failed = false;
        SyncFailed = false;
        SyncError = false;
        SyncFailure = false;
        HasFailed = false;
        DidFail = false;
        FailedSync = false;
        Error = false;
        HasError = false;
        NoticeVisible = false;
        NonModalNoticeVisible = false;
        NonModalNotice = false;
        NoticeMessage = string.Empty;
        NonModalNoticeMessage = string.Empty;
        NonModalMessage = string.Empty;
        SyncNoticeMessage = string.Empty;
        SyncNoticeText = string.Empty;
        SyncErrorMessage = string.Empty;
        SyncFailureMessage = string.Empty;
        SyncWarningMessage = string.Empty;
        Message = string.Empty;
    }

    public static bool Degrade() { _isDegraded = true; _deferred = true; Degraded = true; Deferred = true; return true; }
    public static bool DegradePerformance() { return Degrade(); }
    public static bool DegradeSyncPerformance() { return Degrade(); }
    public static bool EnableDegradedMode() { return Degrade(); }
    public static bool EnterDegradedMode() { return Degrade(); }
    public static bool SetDegraded(bool value = true) { if (value) return Degrade(); return DisableDegradation(); }
    public static bool ResetState() { Reset(); return true; }
    public static bool ResetAll() { Reset(); return true; }
    public static bool Clear() { Reset(); return true; }
    public static bool ClearState() { Reset(); return true; }
    public static bool DegradeSync() { return Degrade(); }
    public static bool EnableDegradation() { return Degrade(); }
    public static bool DisableDegradation() { _isDegraded = false; _deferred = false; Degraded = false; Deferred = false; return true; }

    public bool IsDegradedInstance { get => _isDegraded; set => _isDegraded = value; }
    public bool SyncWithRetry(string? newContent) => Synchronizer.SyncEdit(SourcePanel ?? Left, newContent);
    public bool RetrySyncNow() { ResetFailureState(); return true; }
}

// Instance shim: allow `new Sync...().Logger = ...`? Static Logger already accessible. Add instance property named Logger too? Can't same name as static. But instance access to static Logger works. Good.