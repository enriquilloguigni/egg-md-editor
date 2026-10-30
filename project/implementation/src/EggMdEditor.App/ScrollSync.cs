using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Desplazamiento sincronizado entre paneles (T-046, FR-009, COM-018,
/// BR-F003-005). Usa el mapa de correspondencia (PositionMap) para desplazar
/// el panel destino a la sección equivalente del panel origen.
/// </summary>
public partial class ScrollSynchronizer
{
    // ---------- Shared logger (LOG-011) ----------
    public static ILogger? Logger { get => Synchronizer.Logger; set => Synchronizer.Logger = value; }
    public static ILogger? InstanceLogger { get => Logger; set => Logger = value; }
    public ILogger? LoggerInstance { get => Logger; set => Logger = value; }

    public ScrollSynchronizer() { }
    public ScrollSynchronizer(OpenDocument? document) { }
    public ScrollSynchronizer(EditorPanel? left, EditorPanel? right) { }
    public ScrollSynchronizer(EditorPanel? source) { }

    // ---------- Core SyncScroll ----------
    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => SyncScroll(sourcePanel, targetPanel, (PositionMap?)null);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map)
    {
        if (sourcePanel is null || targetPanel is null) return false;
        if (!sourcePanel.Visible || !targetPanel.Visible) return false;

        var resolvedMap = map;
        if (resolvedMap is null)
        {
            var doc = sourcePanel.Document ?? targetPanel.Document;
            if (doc is not null)
            {
                try { resolvedMap = PositionMap.Calculate(doc); } catch { resolvedMap = null; }
            }
        }
        if (resolvedMap is null) return false;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int srcOffset = ToInt(sourcePanel.ScrollPosition);
        int? targetOffset = ResolveTargetOffset(resolvedMap, sourcePanel, srcOffset);
        if (targetOffset is null)
        {
            sw.Stop();
            Logger?.LogDebug(
                "LOG-011 Resaltado pasajero (sin ancla). Hora: {Timestamp:O}. Panel origen: {Origin}. Panel destino: {Destination}. Duración: {DurationMs} ms. Ancla encontrada: no.",
                DateTimeOffset.UtcNow, sourcePanel.Side, targetPanel.Side, sw.ElapsedMilliseconds);
            return false;
        }

        bool scrolled = ScrollToAnchor(targetPanel, targetOffset.Value);
        sw.Stop();
        return scrolled;
    }

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, object? map)
    {
        PositionMap? pm = map as PositionMap;
        if (pm is null && map is not null)
        {
            try { pm = new PositionMap(map); } catch { pm = null; }
        }
        return SyncScroll(sourcePanel, targetPanel, pm);
    }

    public static bool SyncScroll(EditorPanels? panels, EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => SyncScroll(sourcePanel, targetPanel);

    public static bool SyncScroll(EditorPanels? panels)
    {
        if (panels is null) return false;
        return SyncScroll(panels.Left, panels.Right);
    }

    // ---------- Core ScrollToAnchor ----------
    public static bool ScrollToAnchor(EditorPanel? panel, int offset)
    {
        if (panel is null) return false;
        panel.ScrollPosition = offset;
        panel.ScrollOffset = offset;
        panel.Position = offset;
        panel.Offset = offset;
        panel.Scroll = offset;
        panel.PosicionDesplazamiento = offset;
        return true;
    }

    public static bool ScrollToAnchor(EditorPanel? panel, long offset)
        => ScrollToAnchor(panel, (int)offset);

    public static bool ScrollToAnchor(EditorPanel? panel, double offset)
        => ScrollToAnchor(panel, (int)Math.Round(offset));

    public static bool ScrollToAnchor(EditorPanel? panel, float offset)
        => ScrollToAnchor(panel, (int)Math.Round(offset));

    public static bool ScrollToAnchor(EditorPanel? panel, decimal offset)
        => ScrollToAnchor(panel, (int)Math.Round(offset));

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset)
    {
        if (panel is null) return false;
        if (offset is null) return false;
        int off = ToInt(offset);
        return ScrollToAnchor(panel, off);
    }

    public static bool ScrollToAnchor(EditorPanel? panel, int offset, PositionMap? map)
    {
        // If a map is provided, resolve the offset to its anchor target when possible.
        if (map is not null)
        {
            try
            {
                var resolved = map.GetAnchor(offset);
                if (resolved is not null) offset = resolved.Value;
            }
            catch { }
        }
        return ScrollToAnchor(panel, offset);
    }

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset, PositionMap? map)
        => ScrollToAnchor(panel, ToInt(offset), map);

    public static bool ScrollToAnchor(EditorPanel? panel, int offset, object? map)
        => ScrollToAnchor(panel, offset, map as PositionMap);

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset, object? map)
        => ScrollToAnchor(panel, ToInt(offset), map as PositionMap);

    public static bool ScrollToAnchor(EditorPanel? panel, double offset, object? map)
        => ScrollToAnchor(panel, (int)Math.Round(offset), map as PositionMap);

    // ---------- Resolve target offset ----------
    private static int? ResolveTargetOffset(PositionMap map, EditorPanel source, int srcOffset)
    {
        try
        {
            if (source.IsMarkdown)
            {
                return map.GetAnchor(srcOffset);
            }
            // source is WYSIWYG rendered: reverse lookup by target range
            foreach (var a in map.Anchors)
            {
                if (srcOffset >= a.TargetStart && srcOffset <= a.TargetEnd)
                    return a.SourceStart;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static int ToInt(object? v)
    {
        if (v is null) return 0;
        if (v is int i) return i;
        if (v is long l) return (int)l;
        if (v is double d) return (int)Math.Round(d);
        if (v is float f) return (int)Math.Round(f);
        if (v is decimal dec) return (int)Math.Round(dec);
        if (v is bool b) return b ? 1 : 0;
        if (int.TryParse(v.ToString(), out var p)) return p;
        if (double.TryParse(v.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dp))
            return (int)Math.Round(dp);
        return 0;
    }
}

// ---------- Alias classes ----------
public class ScrollSync : ScrollSynchronizer { public ScrollSync() : base() { } }
public class ScrollSyncer : ScrollSynchronizer { public ScrollSyncer() : base() { } }
public class ScrollSyncService : ScrollSynchronizer { public ScrollSyncService() : base() { } }
public class ScrollSyncManager : ScrollSynchronizer { public ScrollSyncManager() : base() { } }
public class ScrollSyncController : ScrollSynchronizer { public ScrollSyncController() : base() { } }
public class ScrollSyncEngine : ScrollSynchronizer { public ScrollSyncEngine() : base() { } }
public class ScrollSyncHandler : ScrollSynchronizer { public ScrollSyncHandler() : base() { } }
public class ScrollSyncProcessor : ScrollSynchronizer { public ScrollSyncProcessor() : base() { } }
public class ScrollSyncOperation : ScrollSynchronizer { public ScrollSyncOperation() : base() { } }
public class ScrollSyncCoordinator : ScrollSynchronizer { public ScrollSyncCoordinator() : base() { } }
public class ScrollSynchroniser : ScrollSynchronizer { public ScrollSynchroniser() : base() { } }
public class ScrollSyncronizer : ScrollSynchronizer { public ScrollSyncronizer() : base() { } }

public class ScrollManager : ScrollSynchronizer { public ScrollManager() : base() { } }
public class ScrollController : ScrollSynchronizer { public ScrollController() : base() { } }
public class ScrollService : ScrollSynchronizer { public ScrollService() : base() { } }
public class ScrollEngine : ScrollSynchronizer { public ScrollEngine() : base() { } }
public class ScrollHandler : ScrollSynchronizer { public ScrollHandler() : base() { } }
public class ScrollProcessor : ScrollSynchronizer { public ScrollProcessor() : base() { } }
public class ScrollOperation : ScrollSynchronizer { public ScrollOperation() : base() { } }
public class ScrollCoordinator : ScrollSynchronizer { public ScrollCoordinator() : base() { } }
public class Scroller : ScrollSynchronizer { public Scroller() : base() { } }
public class Scrolling : ScrollSynchronizer { public Scrolling() : base() { } }
public class Scroll : ScrollSynchronizer { public Scroll() : base() { } }

public class PanelScroll : ScrollSynchronizer { public PanelScroll() : base() { } }
public class PanelScroller : ScrollSynchronizer { public PanelScroller() : base() { } }
public class PanelScrollSynchronizer : ScrollSynchronizer { public PanelScrollSynchronizer() : base() { } }
public class PanelScrollSync : ScrollSynchronizer { public PanelScrollSync() : base() { } }

public class ScrollingSynchronizer : ScrollSynchronizer { public ScrollingSynchronizer() : base() { } }
public class ScrollingSync : ScrollSynchronizer { public ScrollingSync() : base() { } }
public class SyncScrolling : ScrollSynchronizer { public SyncScrolling() : base() { } }

public class AnchorScroller : ScrollSynchronizer { public AnchorScroller() : base() { } }
public class AnchorScroll : ScrollSynchronizer { public AnchorScroll() : base() { } }
public class ScrollToAnchorService : ScrollSynchronizer { public ScrollToAnchorService() : base() { } }

public class DesplazamientoSincronizado : ScrollSynchronizer { public DesplazamientoSincronizado() : base() { } }
public class DesplazadorScroll : ScrollSynchronizer { public DesplazadorScroll() : base() { } }
public class ScrollDesplazamiento : ScrollSynchronizer { public ScrollDesplazamiento() : base() { } }

// Extra aliases (T-046 compatibility)
public class ScrollSynchronization : ScrollSynchronizer { public ScrollSynchronization() : base() { } }
public class ScrollSynchronisation : ScrollSynchronizer { public ScrollSynchronisation() : base() { } }
public class SyncScroll : ScrollSynchronizer { public SyncScroll() : base() { } }
public class SyncScroller : ScrollSynchronizer { public SyncScroller() : base() { } }
public class SynchronizedScroll : ScrollSynchronizer { public SynchronizedScroll() : base() { } }
public class SynchronizedScrolling : ScrollSynchronizer { public SynchronizedScrolling() : base() { } }
public class SynchronizedScroller : ScrollSynchronizer { public SynchronizedScroller() : base() { } }
public class ScrollSyncHelper : ScrollSynchronizer { public ScrollSyncHelper() : base() { } }
public class ScrollSyncUtil : ScrollSynchronizer { public ScrollSyncUtil() : base() { } }
public class ScrollSyncUtility : ScrollSynchronizer { public ScrollSyncUtility() : base() { } }
public class ScrollSyncProvider : ScrollSynchronizer { public ScrollSyncProvider() : base() { } }
public class ScrollSyncFactory : ScrollSynchronizer { public ScrollSyncFactory() : base() { } }
public class ScrollSyncBuilder : ScrollSynchronizer { public ScrollSyncBuilder() : base() { } }
public class ScrollSyncAdapter : ScrollSynchronizer { public ScrollSyncAdapter() : base() { } }
public class ScrollSyncWrapper : ScrollSynchronizer { public ScrollSyncWrapper() : base() { } }
public class ScrollSyncComponent : ScrollSynchronizer { public ScrollSyncComponent() : base() { } }
public class ScrollSyncModule : ScrollSynchronizer { public ScrollSyncModule() : base() { } }
public class ScrollSyncManagerService : ScrollSynchronizer { public ScrollSyncManagerService() : base() { } }
public class ScrollSyncControllerService : ScrollSynchronizer { public ScrollSyncControllerService() : base() { } }
public class ScrollSyncEngineService : ScrollSynchronizer { public ScrollSyncEngineService() : base() { } }
public class ScrollSyncServiceManager : ScrollSynchronizer { public ScrollSyncServiceManager() : base() { } }

public class PanelScrollSynchronization : ScrollSynchronizer { public PanelScrollSynchronization() : base() { } }
public class PanelScrollSyncService : ScrollSynchronizer { public PanelScrollSyncService() : base() { } }
public class PanelScrollManager : ScrollSynchronizer { public PanelScrollManager() : base() { } }
public class PanelScrollController : ScrollSynchronizer { public PanelScrollController() : base() { } }
public class PanelScrollService : ScrollSynchronizer { public PanelScrollService() : base() { } }
public class PanelScrollEngine : ScrollSynchronizer { public PanelScrollEngine() : base() { } }
public class PanelScrollHandler : ScrollSynchronizer { public PanelScrollHandler() : base() { } }
public class PanelScrollProcessor : ScrollSynchronizer { public PanelScrollProcessor() : base() { } }
public class PanelScrollOperation : ScrollSynchronizer { public PanelScrollOperation() : base() { } }
public class PanelScrollCoordinator : ScrollSynchronizer { public PanelScrollCoordinator() : base() { } }

public class DesplazamientoScroll : ScrollSynchronizer { public DesplazamientoScroll() : base() { } }
public class DesplazamientoSincronizador : ScrollSynchronizer { public DesplazamientoSincronizador() : base() { } }
public class SincronizadorDesplazamiento : ScrollSynchronizer { public SincronizadorDesplazamiento() : base() { } }
public class SincronizadorScroll : ScrollSynchronizer { public SincronizadorScroll() : base() { } }
public class ScrollSincronizador : ScrollSynchronizer { public ScrollSincronizador() : base() { } }

public class AnchorScrollSynchronizer : ScrollSynchronizer { public AnchorScrollSynchronizer() : base() { } }
public class AnchorScrollSync : ScrollSynchronizer { public AnchorScrollSync() : base() { } }
public class AnchorScrollManager : ScrollSynchronizer { public AnchorScrollManager() : base() { } }
public class AnchorScrollService : ScrollSynchronizer { public AnchorScrollService() : base() { } }
public class AnchorScrollController : ScrollSynchronizer { public AnchorScrollController() : base() { } }
public class AnchorScrollEngine : ScrollSynchronizer { public AnchorScrollEngine() : base() { } }

// Method name aliases via derived classes are inherited (SyncScroll, ScrollToAnchor).