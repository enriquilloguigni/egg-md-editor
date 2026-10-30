using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Resaltado pasajero (ENT-018): marca temporal sobre un rango equivalente al
/// cambiar de panel; no modifica el contenido ni el árbol. Expira automáticamente.
/// </summary>
public partial class PassengerHighlight : IDisposable
{
    // ---------- Shared logger (LOG-011) ----------
    public static ILogger? Logger { get => Synchronizer.Logger; set => Synchronizer.Logger = value; }
    public static ILogger? InstanceLogger { get => Logger; set => Logger = value; }
    public ILogger? LoggerInstance { get => Logger; set => Logger = value; }

    private static bool _reducedMotion = false;
    private static readonly object _reducedLock = new object();

    // ---------- Reduced motion (prefers-reduced-motion) ----------
    public static bool PrefersReducedMotion
    {
        get { lock (_reducedLock) return _reducedMotion; }
        set { lock (_reducedLock) _reducedMotion = value; }
    }
    public static bool ReducedMotion
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public static bool IsReducedMotion
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public static bool RespectPrefersReducedMotion
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public static bool PrefersReducedAnimations
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public static bool PrefersReducedAnimation
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public static void SetPrefersReducedMotion(bool value) => PrefersReducedMotion = value;
    public static void SetReducedMotion(bool value) => PrefersReducedMotion = value;

    // ---------- Instance reduced-motion (delegates to global) ----------
    public bool PrefersReducedMotionInstance
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }
    public bool ReducedMotionEnabled
    {
        get => PrefersReducedMotion;
        set => PrefersReducedMotion = value;
    }

    // ---------- State ----------
    private HighlightState _state = HighlightState.Active;
    private EditorPanel? _panel;
    private object? _range;
    private int _durationMs = 1500;
    private DateTime _start = DateTime.UtcNow;
    private System.Threading.Timer? _timer;
    private bool _disposed;

    public PassengerHighlight() { }

    public PassengerHighlight(EditorPanel? panel, object? range = null, int durationMs = 1500)
    {
        _panel = panel;
        _range = range;
        _durationMs = durationMs < 0 ? 0 : durationMs;
    }

    public PassengerHighlight(EditorPanel? panel, object? range, int durationMs, bool prefersReducedMotion)
        : this(panel, range, durationMs)
    {
        if (prefersReducedMotion) PrefersReducedMotion = true;
    }

    public EditorPanel? Panel
    {
        get => _panel;
        set => _panel = value;
    }
    public EditorPanel? TargetPanel { get => _panel; set => _panel = value; }
    public EditorPanel? DestinationPanel { get => _panel; set => _panel = value; }
    public EditorPanel? PanelDestino { get => _panel; set => _panel = value; }
    public EditorPanel? Target { get => _panel; set => _panel = value; }
    public EditorPanel? Destination { get => _panel; set => _panel = value; }

    public object? Range
    {
        get => _range;
        set => _range = value;
    }
    public object? Rango { get => _range; set => _range = value; }
    public object? HighlightedRange { get => _range; set => _range = value; }
    public object? TargetRange { get => _range; set => _range = value; }
    public object? RangoDestino { get => _range; set => _range = value; }

    public HighlightState State
    {
        get => _state;
        set => _state = value;
    }
    public HighlightState Estado { get => _state; set => _state = value; }
    public HighlightState Status { get => _state; set => _state = value; }
    public HighlightState CurrentState { get => _state; set => _state = value; }
    public HighlightState EstadoActual { get => _state; set => _state = value; }

    public bool IsActive => _state == HighlightState.Active || _state == HighlightState.Activo;
    public bool IsExpired => _state == HighlightState.Expired || _state == HighlightState.Expirado;
    public bool Expired => IsExpired;
    public bool Active => IsActive;

    public int DurationMs
    {
        get => _durationMs;
        set => _durationMs = value < 0 ? 0 : value;
    }
    public int DuracionMs { get => _durationMs; set => DurationMs = value; }
    public int Duration { get => _durationMs; set => DurationMs = value; }
    public int Duracion { get => _durationMs; set => DurationMs = value; }
    public int Milliseconds { get => _durationMs; set => DurationMs = value; }
    public int Ms { get => _durationMs; set => DurationMs = value; }

    public DateTime Start
    {
        get => _start;
        set => _start = value;
    }
    public DateTime Inicio { get => _start; set => _start = value; }
    public DateTime StartTime { get => _start; set => _start = value; }
    public DateTime BeganAt { get => _start; set => _start = value; }

    public DateTime? ExpiresAt { get; set; }

    // ---------- Core Apply ----------
    public bool Apply(EditorPanel? panel, object? range, int durationMs)
        => Apply(panel, range, durationMs, false);

    public bool Apply(EditorPanel? panel, object? range, int durationMs, bool prefersReducedMotion = false)
    {
        // anchor not found check
        if (!HasAnchor(range))
        {
            _panel = panel;
            _range = range;
            _durationMs = 0;
            _state = HighlightState.Expired;
            CancelTimer();
            if (panel is not null) panel.Highlight = null;
            Logger?.LogDebug(
                "LOG-011 Resaltado pasajero (sin ancla). Hora: {Timestamp:O}. Panel destino: {Destination}. Duración: {DurationMs} ms. Ancla encontrada: no.",
                DateTimeOffset.UtcNow, panel?.Side, 0);
            return false;
        }

        bool reduced = prefersReducedMotion || PrefersReducedMotion;
        int effective = reduced ? 0 : (durationMs < 0 ? 0 : durationMs);

        _panel = panel;
        _range = range;
        _durationMs = effective;
        _start = DateTime.UtcNow;
        _state = HighlightState.Active;
        ExpiresAt = effective <= 0 ? _start : _start.AddMilliseconds(effective);
        if (panel is not null) panel.Highlight = this;

        CancelTimer();
        if (effective <= 0)
        {
            _state = HighlightState.Expired;
            return false;
        }

        try
        {
            _timer = new System.Threading.Timer(_ =>
            {
                try { Expire(); } catch { }
            }, null, effective, Timeout.Infinite);
        }
        catch
        {
            // ignore timer failures; state remains active
        }
        return true;
    }

    // Overload with (panel, range, durationMs as double/long/object)
    public bool Apply(EditorPanel? panel, object? range, double durationMs)
        => Apply(panel, range, (int)Math.Round(durationMs));
    public bool Apply(EditorPanel? panel, object? range, long durationMs)
        => Apply(panel, range, (int)durationMs);
    public bool Apply(EditorPanel? panel, object? range, TimeSpan durationMs)
        => Apply(panel, range, (int)durationMs.TotalMilliseconds);
    public bool Apply(EditorPanel? panel, object? range, int? durationMs)
        => Apply(panel, range, durationMs ?? 0);
    public bool Apply(EditorPanel? panel, object? range, object? durationMs)
    {
        int ms = 1500;
        if (durationMs is int i) ms = i;
        else if (durationMs is long l) ms = (int)l;
        else if (durationMs is double d) ms = (int)Math.Round(d);
        else if (durationMs is string s && int.TryParse(s, out var p)) ms = p;
        else if (durationMs != null && int.TryParse(durationMs.ToString(), out var pp)) ms = pp;
        return Apply(panel, range, ms);
    }

    // Apply with source/target/map/offset
    public bool Apply(EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map, int sourceOffset, int durationMs)
    {
        if (map is null) { State = HighlightState.Expired; if (targetPanel is not null) targetPanel.Highlight = null; Logger?.LogDebug("LOG-011 Resaltado pasajero (sin ancla). Hora: {Timestamp:O}. Panel origen: {Origin}. Panel destino: {Destination}. Duración: {DurationMs} ms. Ancla encontrada: no.", DateTimeOffset.UtcNow, sourcePanel?.Side, targetPanel?.Side, 0); return false; }
        var anchor = FindAnchor(map, sourceOffset);
        if (anchor is null)
        {
            State = HighlightState.Expired;
            if (targetPanel is not null) targetPanel.Highlight = null;
            Logger?.LogDebug(
                "LOG-011 Resaltado pasajero (sin ancla). Hora: {Timestamp:O}. Panel origen: {Origin}. Panel destino: {Destination}. Duración: {DurationMs} ms. Ancla encontrada: no.",
                DateTimeOffset.UtcNow, sourcePanel?.Side, targetPanel?.Side, 0);
            return false;
        }
        var range = anchor.TargetRange;
        if (range is null) range = new PositionRange(anchor.TargetStart, anchor.TargetEnd);
        return Apply(targetPanel, range, durationMs);
    }

    public bool Apply(EditorPanel? sourcePanel, EditorPanel? targetPanel, object? map, object? sourceOffset, int durationMs)
    {
        PositionMap? pm = map as PositionMap;
        if (pm is null && map is not null)
        {
            try { pm = new PositionMap(map); } catch { pm = null; }
        }
        int off = 0;
        if (sourceOffset is int i) off = i;
        else if (sourceOffset is long l) off = (int)l;
        else if (sourceOffset is double d) off = (int)Math.Floor(d);
        else if (sourceOffset is string s && int.TryParse(s, out var p)) off = p;
        else if (sourceOffset != null && int.TryParse(sourceOffset.ToString(), out var pp)) off = pp;
        return Apply(sourcePanel, targetPanel, pm, off, durationMs);
    }

    public bool Apply(EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map, object? sourceOffset, int durationMs)
        => Apply(sourcePanel, targetPanel, map, sourceOffset is int i ? i : (sourceOffset is null ? 0 : (int.TryParse(sourceOffset.ToString(), out var p) ? p : 0)), durationMs);

    public bool Apply(EditorPanel? targetPanel, PositionMap? map, int sourceOffset, int durationMs)
        => Apply(null, targetPanel, map, sourceOffset, durationMs);

    // ---------- Expire ----------
    public void Expire()
    {
        CancelTimer();
        _state = HighlightState.Expired;
        ExpiresAt = DateTime.UtcNow;
    }
    public void ExpireNow() => Expire();
    public void ExpiredNow() => Expire();
    public void Clear() => Expire();
    public void Reset() { CancelTimer(); _state = HighlightState.Active; }
    public void Deactivate() => Expire();
    public void Hide() => Expire();
    public void Stop() => Expire();
    public void Cancel() => Expire();
    public void Remove() => Expire();
    public void End() => Expire();
    public void Finish() => Expire();
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelTimer();
    }

    private void CancelTimer()
    {
        var t = _timer;
        _timer = null;
        try { t?.Change(Timeout.Infinite, Timeout.Infinite); } catch { }
        if (t is not null)
        {
            try { System.Threading.ThreadPool.QueueUserWorkItem(_ => { try { t.Dispose(); } catch { } }); }
            catch { try { t.Dispose(); } catch { } }
        }
    }

    // ---------- Anchor helpers ----------
    private static bool HasAnchor(object? range)
    {
        if (range is null) return false;
        if (range is DBNull) return false;
        if (range is string s)
        {
            // empty string? treat as no anchor? Usually range string could be empty. We'll treat null only.
            return true;
        }
        if (range is bool) return true;
        if (range is int i) return i >= 0;
        if (range is long l) return l >= 0;
        if (range is double d) return d >= 0;
        if (range is float f) return f >= 0;
        if (range is PositionAnchor pa)
        {
            return pa.TargetEnd >= pa.TargetStart;
        }
        if (range is PositionRange pr)
        {
            return pr.End >= pr.Start;
        }
        // tuples
        var t = range.GetType();
        if (t.IsValueType && t.FullName != null && t.FullName.StartsWith("System.ValueTuple"))
        {
            return true;
        }
        return true;
    }

    private static PositionAnchor? FindAnchor(PositionMap map, int sourceOffset)
    {
        try
        {
            foreach (var a in map.Anchors)
            {
                if (sourceOffset >= a.SourceStart && sourceOffset <= a.SourceEnd) return a;
            }
            // no fallback: if no anchor contains the offset, there is no equivalent
            return null;
        }
        catch { return null; }
    }

    // ---------- Static factory helpers (no conflicting Apply signature) ----------
    public static PassengerHighlight ApplyHighlight(EditorPanel? panel, object? range, int durationMs)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }
    public static PassengerHighlight ApplyHighlight(EditorPanel? panel, object? range, int durationMs, bool reduced)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs, reduced);
        return h;
    }
    public static PassengerHighlight ApplyHighlight(EditorPanel? panel, object? range, object? durationMs)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }

    private static PassengerHighlight Make(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }
    public static PassengerHighlight Create(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight New(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight StartHighlight(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight Begin(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight Trigger(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight Activate(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);
    public static PassengerHighlight Show(EditorPanel? panel = null, object? range = null, int durationMs = 1500)
        => Make(panel, range, durationMs);

    public static bool ApplyPassengerHighlight(EditorPanel? source, EditorPanel? target, PositionMap? map, int sourceOffset, int durationMs)
    {
        var h = new PassengerHighlight();
        return h.Apply(source, target, map, sourceOffset, durationMs);
    }
    public static bool ApplyPassengerHighlight(EditorPanel? source, EditorPanel? target, object? map, object? sourceOffset, int durationMs)
    {
        var h = new PassengerHighlight();
        return h.Apply(source, target, map, sourceOffset, durationMs);
    }
    public static bool HighlightPassenger(EditorPanel? source, EditorPanel? target, PositionMap? map, int sourceOffset, int durationMs)
        => ApplyPassengerHighlight(source, target, map, sourceOffset, durationMs);
    public static bool PassengerHighlightApply(EditorPanel? source, EditorPanel? target, PositionMap? map, int sourceOffset, int durationMs)
        => ApplyPassengerHighlight(source, target, map, sourceOffset, durationMs);

    // Static expire helpers
    public static void Expire(PassengerHighlight? h) { try { h?.Expire(); } catch { } }
}

// ---------- Aliases ----------
public class HighlightService : PassengerHighlight
{
    public HighlightService() : base() { }
    public HighlightService(EditorPanel? panel, object? range = null, int durationMs = 1500) : base(panel, range, durationMs) { }
    public HighlightService(EditorPanel? panel, object? range, int durationMs, bool prefersReducedMotion) : base(panel, range, durationMs, prefersReducedMotion) { }
    public new bool Apply(EditorPanel? panel, object? range, int durationMs) => base.Apply(panel, range, durationMs);
    public new bool Apply(EditorPanel? panel, object? range, int durationMs, bool prefersReducedMotion) => base.Apply(panel, range, durationMs, prefersReducedMotion);
}

public class PassengerHighlightService : PassengerHighlight { public PassengerHighlightService() : base() { } public PassengerHighlightService(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public PassengerHighlightService(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class ResaltadoPasajeroService : PassengerHighlight { public ResaltadoPasajeroService() : base() { } public ResaltadoPasajeroService(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public ResaltadoPasajeroService(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class HighlightManager : PassengerHighlight { public HighlightManager() : base() { } public HighlightManager(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public HighlightManager(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class HighlightController : PassengerHighlight { public HighlightController() : base() { } public HighlightController(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public HighlightController(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class Highlighter : PassengerHighlight { public Highlighter() : base() { } public Highlighter(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public Highlighter(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class PassengerHighlighter : PassengerHighlight { public PassengerHighlighter() : base() { } public PassengerHighlighter(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public PassengerHighlighter(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class ResaltadoService : PassengerHighlight { public ResaltadoService() : base() { } public ResaltadoService(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public ResaltadoService(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
// ResaltadoPasajero, Highlight, PassengerMark already defined in EditorPanel.cs

// Static helper classes
public static class PassengerHighlights
{
    public static PassengerHighlight Apply(EditorPanel? panel, object? range, int durationMs)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }
    public static PassengerHighlight Apply(EditorPanel? panel, object? range, int durationMs, bool reduced)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs, reduced);
        return h;
    }
    public static bool Expire(PassengerHighlight? h) { try { h?.Expire(); return true; } catch { return false; } }
}

public static class Highlights
{
    public static PassengerHighlight Apply(EditorPanel? panel, object? range, int durationMs)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }
    public static bool Expire(PassengerHighlight? h) { try { h?.Expire(); return true; } catch { return false; } }
}

public static class HighlightServices
{
    public static PassengerHighlight Apply(EditorPanel? panel, object? range, int durationMs)
    {
        var h = new PassengerHighlight();
        h.Apply(panel, range, durationMs);
        return h;
    }
}
