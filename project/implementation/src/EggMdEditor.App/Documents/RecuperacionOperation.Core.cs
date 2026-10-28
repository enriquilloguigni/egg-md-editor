using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public class RecuperacionOperation
{
    public static ILogger? Logger { get; set; }

    public OpenDocument? CurrentDocument { get; set; }
    public RecoveryCopy? RecoveryCopy { get; set; }
    public RecoveryCopy? Recovery { get => RecoveryCopy; set => RecoveryCopy = value; }
    public RecoveryCopy? Copy { get => RecoveryCopy; set => RecoveryCopy = value; }
    public string? StoragePath { get; set; }
    public string? RecoveryPath { get => StoragePath; set => StoragePath = value; }
    public string? Path { get => StoragePath; set => StoragePath = value; }

    public const string FailureMessage = "No se pudo recuperar el último estado. Se ha abierto un documento vacío; tu archivo original no se ha modificado.";

    public RecuperacionOperation() { }
    public RecuperacionOperation(OpenDocument? current) { CurrentDocument = current; }
    public RecuperacionOperation(RecoveryCopy? recovery) { RecoveryCopy = recovery; }
    public RecuperacionOperation(string? storagePath) { StoragePath = storagePath; }

    public OpenDocument? Document { get => CurrentDocument; set => CurrentDocument = value; }

    protected static OpenDocument Core(RecoveryCopy? recovery, string? storagePath, OpenDocument? currentDocument)
    {
        var logger = Logger;
        RecoveryCopy? copy = recovery;
        string? path = storagePath;

        if (copy is null && !string.IsNullOrWhiteSpace(path))
        {
            if (RecoveryCopy.TryLoad(path!, out var loaded) && loaded is not null)
            {
                copy = loaded;
            }
            else
            {
                try { if (File.Exists(path!)) RecoveryCopy.Invalidate(path!); } catch { }
                if (File.Exists(path!))
                {
                    try { logger?.LogWarning("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Marca de tiempo: {TimestampCopy:O}. Duración: {DurationMs} ms. Resultado: copia obsoleta o fallo.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow, 0); } catch { }
                }
                else
                {
                    try { logger?.LogDebug("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Duración: {DurationMs} ms. Resultado: no había copia.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), 0); } catch { }
                }
                return new OpenDocument();
            }
        }

        if (copy is null && string.IsNullOrWhiteSpace(path))
        {
            var defaultPath = RecoveryCopy.GetDefaultPath();
            if (RecoveryCopy.TryLoad(defaultPath, out var loaded) && loaded is not null)
            {
                copy = loaded;
                path = defaultPath;
            }
            else
            {
                if (File.Exists(defaultPath))
                {
                    try { RecoveryCopy.Invalidate(defaultPath); } catch { }
                }
                try { logger?.LogDebug("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Duración: {DurationMs} ms. Resultado: no había copia.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), 0); } catch { }
                return new OpenDocument();
            }
        }

        if (copy is null)
        {
            try { logger?.LogDebug("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Duración: {DurationMs} ms. Resultado: no había copia.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), 0); } catch { }
            return new OpenDocument();
        }

        if (!copy.IsValid)
        {
            try { copy.MarkObsolete(); } catch { try { copy.MarkObsoleta(); } catch { } }
            var sp = path ?? copy.StoragePath;
            if (!string.IsNullOrWhiteSpace(sp))
            {
                try { RecoveryCopy.Invalidate(sp!); } catch { }
            }
            try { logger?.LogWarning("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Marca de tiempo: {TimestampCopy:O}. Duración: {DurationMs} ms. Resultado: copia obsoleta.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), copy.Timestamp, 0); } catch { }
            return new OpenDocument();
        }

        try
        {
            var doc = new OpenDocument(copy.Content, copy.OriginalPath, DocumentOrigin.Recovered, DocumentLastOperation.Recover);
            doc.MarkChanged();
            try { logger?.LogInformation("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Marca de tiempo: {TimestampCopy:O}. Duración: {DurationMs} ms. Resultado: restaurada.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), copy.Timestamp, 0); } catch { }
            return doc;
        }
        catch
        {
            var sp = path ?? copy.StoragePath;
            if (!string.IsNullOrWhiteSpace(sp))
            {
                try { RecoveryCopy.Invalidate(sp!); } catch { }
            }
            try { copy.MarkObsolete(); } catch { try { copy.MarkObsoleta(); } catch { } }
            try { logger?.LogWarning("LOG-007 Recuperación al iniciar. Hora: {Timestamp:O}. Sesión: {SessionId}. Marca de tiempo: {TimestampCopy:O}. Duración: {DurationMs} ms. Resultado: fallo.", DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), copy.Timestamp, 0); } catch { }
            return new OpenDocument();
        }
    }

    public static OpenDocument Execute() => Core(null, null, null);
    public static OpenDocument Execute(RecoveryCopy? recovery) => Core(recovery, recovery?.StoragePath, null);
    public static OpenDocument Execute(string? storagePath) => Core(null, storagePath, null);
    public static OpenDocument Execute(RecoveryCopy? recovery, string? storagePath) => Core(recovery, storagePath, null);
    public static OpenDocument Execute(string? storagePath, RecoveryCopy? recovery) => Core(recovery, storagePath, null);

    public static OpenDocument Execute(OpenDocument? current) => Core(null, null, current);
    public static OpenDocument Execute(OpenDocument? current, RecoveryCopy? recovery) => Core(recovery, recovery?.StoragePath, current);
    public static OpenDocument Execute(OpenDocument? current, string? storagePath) => Core(null, storagePath, current);
    public static OpenDocument Execute(OpenDocument? current, RecoveryCopy? recovery, string? storagePath) => Core(recovery, storagePath, current);
    public static OpenDocument Execute(OpenDocument? current, string? storagePath, RecoveryCopy? recovery) => Core(recovery, storagePath, current);

    public static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? current) => Core(recovery, recovery?.StoragePath, current);
    public static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? current, string? storagePath) => Core(recovery, storagePath, current);
    public static OpenDocument Execute(RecoveryCopy? recovery, string? storagePath, OpenDocument? current) => Core(recovery, storagePath, current);

    public static OpenDocument Execute(string? storagePath, OpenDocument? current) => Core(null, storagePath, current);
    public static OpenDocument Execute(string? storagePath, OpenDocument? current, RecoveryCopy? recovery) => Core(recovery, storagePath, current);
    public static OpenDocument Execute(string? storagePath, RecoveryCopy? recovery, OpenDocument? current) => Core(recovery, storagePath, current);

    public static OpenDocument Execute(object? source)
    {
        if (source is RecoveryCopy rc) return Core(rc, rc.StoragePath, null);
        if (source is string s) return Core(null, s, null);
        if (source is OpenDocument d) return Core(null, null, d);
        return Core(null, null, null);
    }

    public static OpenDocument Recover() => Execute();
    public static OpenDocument Recover(RecoveryCopy? recovery) => Execute(recovery);
    public static OpenDocument Recover(string? storagePath) => Execute(storagePath);
    public static OpenDocument Recover(RecoveryCopy? recovery, string? storagePath) => Execute(recovery, storagePath);
    public static OpenDocument Recover(string? storagePath, RecoveryCopy? recovery) => Execute(storagePath, recovery);

    public static OpenDocument Restore() => Execute();
    public static OpenDocument Restore(RecoveryCopy? recovery) => Execute(recovery);
    public static OpenDocument Restore(string? storagePath) => Execute(storagePath);
    public static OpenDocument Restore(RecoveryCopy? recovery, string? storagePath) => Execute(recovery, storagePath);
    public static OpenDocument Restore(string? storagePath, RecoveryCopy? recovery) => Execute(storagePath, recovery);

    public static OpenDocument RecoverStartup() => Execute();
    public static OpenDocument RecoverOnStartup() => Execute();
    public static OpenDocument StartupRecover() => Execute();
    public static OpenDocument AutoRecover() => Execute();
    public static OpenDocument AutomaticRecovery() => Execute();
    public static OpenDocument RecoverAutomatically() => Execute();

    public static bool TryExecute(out OpenDocument? document) { document = null; try { document = Execute(); return true; } catch { document = null; return false; } }
    public static bool TryExecute(RecoveryCopy? recovery, out OpenDocument? document) { document = null; try { document = Execute(recovery); return true; } catch { document = null; return false; } }
    public static bool TryExecute(string? storagePath, out OpenDocument? document) { document = null; try { document = Execute(storagePath); return true; } catch { document = null; return false; } }
    public static bool TryExecute(RecoveryCopy? recovery, string? storagePath, out OpenDocument? document) { document = null; try { document = Execute(recovery, storagePath); return true; } catch { document = null; return false; } }
    public static bool TryExecute(string? storagePath, RecoveryCopy? recovery, out OpenDocument? document) { document = null; try { document = Execute(storagePath, recovery); return true; } catch { document = null; return false; } }

    public OpenDocument ExecuteInstance() => ExecuteInstance(CurrentDocument, RecoveryCopy, StoragePath);
    public OpenDocument ExecuteInstance(RecoveryCopy? recovery) => ExecuteInstance(CurrentDocument, recovery, StoragePath);
    public OpenDocument ExecuteInstance(string? storagePath) => ExecuteInstance(CurrentDocument, RecoveryCopy, storagePath);
    public OpenDocument ExecuteInstance(OpenDocument? document) { var d = Core(RecoveryCopy, StoragePath, document); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, RecoveryCopy? recovery) { var d = Core(recovery ?? RecoveryCopy, StoragePath, document); CurrentDocument = document; if (recovery is not null) RecoveryCopy = recovery; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, string? storagePath) { var d = Core(RecoveryCopy, storagePath ?? StoragePath, document); CurrentDocument = document; if (storagePath is not null) StoragePath = storagePath; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, RecoveryCopy? recovery, string? storagePath)
    {
        var d = Core(recovery ?? RecoveryCopy, storagePath ?? StoragePath, document);
        CurrentDocument = document;
        if (recovery is not null) RecoveryCopy = recovery;
        if (storagePath is not null) StoragePath = storagePath;
        return d;
    }
}