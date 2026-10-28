using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Operación Guardar / Guardar como (T-028, OP-012/013, BR-F002-005/006/010).
/// </summary>
public partial class GuardarOperation
{
    public static ILogger? Logger { get; set; }

    public OpenDocument? CurrentDocument { get; set; }
    public RecentFiles? RecentFiles { get; set; }
    public RecentFiles? Recents { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? Store { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? RecentStore { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? RecentList { get => RecentFiles; set => RecentFiles = value; }
    public RecoveryCopy? RecoveryCopy { get; set; }
    public RecoveryCopy? Recovery { get => RecoveryCopy; set => RecoveryCopy = value; }
    public RecoveryCopy? Copy { get => RecoveryCopy; set => RecoveryCopy = value; }

    public GuardarOperation() { }
    public GuardarOperation(OpenDocument? current) { CurrentDocument = current; }
    public GuardarOperation(OpenDocument? current, RecentFiles? recents) { CurrentDocument = current; RecentFiles = recents; }
    public GuardarOperation(RecentFiles? recents, OpenDocument? current) { CurrentDocument = current; RecentFiles = recents; }
    public GuardarOperation(OpenDocument? current, RecentFiles? recents, RecoveryCopy? recovery)
    {
        CurrentDocument = current;
        RecentFiles = recents;
        RecoveryCopy = recovery;
    }
    public GuardarOperation(OpenDocument? current, RecoveryCopy? recovery, RecentFiles? recents)
    {
        CurrentDocument = current;
        RecentFiles = recents;
        RecoveryCopy = recovery;
    }
    public GuardarOperation(RecentFiles? recents, OpenDocument? current, RecoveryCopy? recovery)
    {
        CurrentDocument = current;
        RecentFiles = recents;
        RecoveryCopy = recovery;
    }
    public GuardarOperation(RecoveryCopy? recovery, OpenDocument? current, RecentFiles? recents)
    {
        CurrentDocument = current;
        RecentFiles = recents;
        RecoveryCopy = recovery;
    }

    public const string InvalidExtensionMessage = "El archivo debe tener extensión .md.";

    // ---------- Core ----------
    protected static OpenDocument Core(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery, bool saveAs)
    {
        var logger = Logger;
        string? target = null;
        var mode = saveAs ? "Guardar como" : "Guardar";
        try
        {
            if (document is null) throw new ArgumentNullException(nameof(document));

            target = string.IsNullOrWhiteSpace(path) ? document.FilePath : path!.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("path", nameof(path));
            }

            // Ensure .md extension (BR-F002-006 / AC-013)
            if (!string.Equals(Path.GetExtension(target), ".md", StringComparison.OrdinalIgnoreCase))
            {
                target = target + ".md";
            }

            // Write atomically (T-024)
            var bytesWritten = 0;
            try
            {
                var result = FileWriter.Write(target, document.Content);
                bytesWritten = result.BytesWritten;
            }
            catch
            {
                throw;
            }

            // Update document
            var changedPath = !string.Equals(document.FilePath, target, StringComparison.Ordinal);
            document.FilePath = target;
            document.MarkClean();
            document.LastOperation = saveAs ? DocumentLastOperation.SaveAs : DocumentLastOperation.Save;

            // Recents to front (BR-F002-010)
            if (recents is not null)
            {
                try { recents.Add(target); } catch { }
            }

            // Invalidate recovery copy (BR-F002-012/015)
            if (recovery is not null)
            {
                try { recovery.Invalidate(); } catch { try { recovery.MarkObsolete(); } catch { } }
            }

            try
            {
                logger?.LogInformation(
                    "LOG-005 Guardar / Guardar como. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Modo: {Mode}. Extensión: {Extension}. Bytes escritos: {BytesWritten}. Fin de línea: {LineEnding}. Duración: {DurationMs} ms. ¿Cambió la ruta?: {ChangedPath}. Resultado: éxito.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), document.Id, mode, Path.GetExtension(target), bytesWritten, "OS", 0, changedPath ? "sí" : "no");
            }
            catch { }
            return document;
        }
        catch (Exception ex)
        {
            try
            {
                if (ex is UnauthorizedAccessException)
                {
                    logger?.LogError(ex,
                        "LOG-005 Guardar / Guardar como. Hora: {Timestamp:O}. Sesión: {SessionId}. Modo: {Mode}. Extensión: {Extension}. Resultado: permiso denegado. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                        DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), mode, target is null ? string.Empty : Path.GetExtension(target), ex.GetType().Name, ex.Message);
                }
                else
                {
                    logger?.LogError(ex,
                        "LOG-005 Guardar / Guardar como. Hora: {Timestamp:O}. Sesión: {SessionId}. Modo: {Mode}. Extensión: {Extension}. Resultado: fallo. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                        DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), mode, target is null ? string.Empty : Path.GetExtension(target), ex.GetType().Name, ex.Message);
                }
            }
            catch { }
            throw;
        }
    }

    // ---------- Execute overloads (Save) ----------
    public static OpenDocument Execute(OpenDocument? document)
        => Core(document, null, null, null, false);
    public static OpenDocument Execute(OpenDocument? document, string? path)
        => Core(document, path, null, null, false);
    public static OpenDocument Execute(OpenDocument? document, string? path, RecentFiles? recents)
        => Core(document, path, recents, null, false);
    public static OpenDocument Execute(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, string? path, RecoveryCopy? recovery)
        => Core(document, path, null, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, string? path, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecentFiles? recents)
        => Core(document, null, recents, null, false);
    public static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, string? path)
        => Core(document, path, recents, null, false);
    public static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, null, recents, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, string? path, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, null, null, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, null, recents, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, string? path)
        => Core(document, path, null, recovery, false);
    public static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, string? path, RecentFiles? recents)
        => Core(document, path, recents, recovery, false);

    // path-first
    public static OpenDocument Execute(string? path, OpenDocument? document)
        => Core(document, path, null, null, false);
    public static OpenDocument Execute(string? path, OpenDocument? document, RecentFiles? recents)
        => Core(document, path, recents, null, false);
    public static OpenDocument Execute(string? path, OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(string? path, OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, path, null, recovery, false);
    public static OpenDocument Execute(string? path, OpenDocument? document, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, path, recents, recovery, false);

    // recents-first
    public static OpenDocument Execute(RecentFiles? recents, OpenDocument? document)
        => Core(document, null, recents, null, false);
    public static OpenDocument Execute(RecentFiles? recents, OpenDocument? document, string? path)
        => Core(document, path, recents, null, false);
    public static OpenDocument Execute(RecentFiles? recents, OpenDocument? document, string? path, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(RecentFiles? recents, string? path, OpenDocument? document)
        => Core(document, path, recents, null, false);
    public static OpenDocument Execute(RecentFiles? recents, string? path, OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, false);

    // recovery-first
    public static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document)
        => Core(document, null, null, recovery, false);
    public static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document, string? path)
        => Core(document, path, null, recovery, false);
    public static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document, string? path, RecentFiles? recents)
        => Core(document, path, recents, recovery, false);
    public static OpenDocument Execute(RecoveryCopy? recovery, RecentFiles? recents, OpenDocument? document)
        => Core(document, null, recents, recovery, false);
    public static OpenDocument Execute(RecoveryCopy? recovery, RecentFiles? recents, OpenDocument? document, string? path)
        => Core(document, path, recents, recovery, false);

    // ---------- IEnumerable path? (single) ----------
    public static OpenDocument Execute(IEnumerable<string>? paths, OpenDocument? document)
    {
        var p = paths is null ? null : paths.Select(x => x?.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return Core(document, p, null, null, false);
    }
    public static OpenDocument Execute(IEnumerable<string>? paths, OpenDocument? document, RecentFiles? recents)
    {
        var p = paths is null ? null : paths.Select(x => x?.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return Core(document, p, recents, null, false);
    }

    // ---------- object fallback ----------
    public static OpenDocument Execute(object? source)
    {
        if (source is OpenDocument d) return Core(d, null, null, null, false);
        if (source is string s) throw new ArgumentException("document", nameof(source));
        throw new ArgumentException("source", nameof(source));
    }

    // ---------- instance ----------
    public OpenDocument ExecuteInstance() => ExecuteInstance(CurrentDocument, null, RecentFiles, RecoveryCopy);
    public OpenDocument ExecuteInstance(string? path) => ExecuteInstance(CurrentDocument, path, RecentFiles, RecoveryCopy);
    public OpenDocument ExecuteInstance(string? path, RecentFiles? recents) { var d = Core(CurrentDocument, path, recents ?? RecentFiles, RecoveryCopy, false); CurrentDocument = d; if (recents is not null) RecentFiles = recents; return d; }
    public OpenDocument ExecuteInstance(string? path, RecentFiles? recents, RecoveryCopy? recovery) { var d = Core(CurrentDocument, path, recents ?? RecentFiles, recovery ?? RecoveryCopy, false); CurrentDocument = d; if (recents is not null) RecentFiles = recents; if (recovery is not null) RecoveryCopy = recovery; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document) { var d = Core(document, null, RecentFiles, RecoveryCopy, false); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, string? path) { var d = Core(document, path, RecentFiles, RecoveryCopy, false); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, string? path, RecentFiles? recents) { var d = Core(document, path, recents ?? RecentFiles, RecoveryCopy, false); CurrentDocument = d; if (recents is not null) RecentFiles = recents; return d; }
    public OpenDocument ExecuteInstance(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery) { var d = Core(document, path, recents ?? RecentFiles, recovery ?? RecoveryCopy, false); CurrentDocument = d; if (recents is not null) RecentFiles = recents; if (recovery is not null) RecoveryCopy = recovery; return d; }

    // ---------- Try* ----------
    public static bool TryExecute(OpenDocument? document, out OpenDocument? saved) => TryCore(document, null, null, null, false, out saved);
    public static bool TryExecute(OpenDocument? document, string? path, out OpenDocument? saved) => TryCore(document, path, null, null, false, out saved);
    public static bool TryExecute(OpenDocument? document, string? path, RecentFiles? recents, out OpenDocument? saved) => TryCore(document, path, recents, null, false, out saved);
    public static bool TryExecute(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery, out OpenDocument? saved) => TryCore(document, path, recents, recovery, false, out saved);
    public static bool TryExecute(string? path, OpenDocument? document, out OpenDocument? saved) => TryCore(document, path, null, null, false, out saved);

    protected static bool TryCore(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery, bool saveAs, out OpenDocument? saved)
    {
        saved = null;
        try { saved = Core(document, path, recents, recovery, saveAs); return true; }
        catch { saved = null; return false; }
    }

    // (method aliases moved to GuardarOperation.Aliases.cs)

    public GuardarResult Result { get; set; } = GuardarResult.Completed;
    public GuardarStatus Status { get => (GuardarStatus)Result; set => Result = (GuardarResult)value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}

public enum GuardarResult
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
    Completada = 0,
    Cancelada = 1,
    ErrorOperacion = 2,
}

public enum GuardarStatus
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
}