using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Operación Abrir reciente (T-027, OP-011, BR-F002-010, ERR-006).
/// </summary>
public partial class AbrirRecienteOperation
{
    public static ILogger? Logger { get; set; }

    public OpenDocument? CurrentDocument { get; set; }
    public RecentFiles? RecentFiles { get; set; }
    public RecentFiles? Recents { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? Store { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? RecentStore { get => RecentFiles; set => RecentFiles = value; }
    public RecentFiles? RecentList { get => RecentFiles; set => RecentFiles = value; }

    public AbrirRecienteOperation() { }
    public AbrirRecienteOperation(OpenDocument? current) { CurrentDocument = current; }
    public AbrirRecienteOperation(RecentFiles? recents) { RecentFiles = recents; }
    public AbrirRecienteOperation(OpenDocument? current, RecentFiles? recents) { CurrentDocument = current; RecentFiles = recents; }
    public AbrirRecienteOperation(RecentFiles? recents, OpenDocument? current) { CurrentDocument = current; RecentFiles = recents; }

    public const string UnavailableMessage = "No se pudo abrir: el archivo no existe o la ruta no está disponible.";

    private static OpenDocument Core(string? path, OpenDocument? current, RecentFiles? recents)
    {
        var logger = Logger;
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        var trimmed = path!.Trim();
        try
        {
            var doc = AbrirOperation.Execute(trimmed);
            doc.Origin = DocumentOrigin.Recent;
            doc.Source = DocumentOrigin.Recent;
            doc.DocumentOrigin = DocumentOrigin.Recent;
            doc.LastOperation = DocumentLastOperation.Open;
            doc.MarkClean();

            if (recents is not null)
            {
                if (recents.Contains(trimmed))
                {
                    recents.Touch(trimmed, lastOperation: DateTime.UtcNow, availability: RecentAvailability.Available);
                }
                else
                {
                    recents.Add(trimmed, lastOperation: DateTime.UtcNow, availability: RecentAvailability.Available);
                }
            }
            try
            {
                logger?.LogInformation(
                    "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Operación: Reciente. Extensión: {Extension}. Resultado: éxito.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), doc.Id, Path.GetExtension(trimmed));
            }
            catch { }
            return doc;
        }
        catch (Exception ex)
        {
            if (recents is not null)
            {
                try { recents.Remove(trimmed); } catch { }
            }
            try
            {
                logger?.LogWarning(ex,
                    "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Operación: Reciente. Extensión: {Extension}. Resultado: fallo. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), Path.GetExtension(trimmed), ex.GetType().Name, ex.Message);
            }
            catch { }
            throw;
        }
    }

    // ---------- string paths ----------
    public static OpenDocument Execute(string? path) => Core(path, null, null);
    public static OpenDocument Execute(string? path, OpenDocument? current) => Core(path, current, null);
    public static OpenDocument Execute(string? path, RecentFiles? recents) => Core(path, null, recents);
    public static OpenDocument Execute(string? path, OpenDocument? current, RecentFiles? recents) => Core(path, current, recents);
    public static OpenDocument Execute(string? path, RecentFiles? recents, OpenDocument? current) => Core(path, current, recents);

    // ---------- RecentFile entries ----------
    public static OpenDocument Execute(RecentFile? entry) => Core(entry is null ? null : entry.Path, null, null);
    public static OpenDocument Execute(RecentFile? entry, OpenDocument? current) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, null);
    public static OpenDocument Execute(RecentFile? entry, RecentFiles? recents) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, null, recents);
    public static OpenDocument Execute(RecentFile? entry, OpenDocument? current, RecentFiles? recents) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, recents);
    public static OpenDocument Execute(RecentFile? entry, RecentFiles? recents, OpenDocument? current) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, recents);

    // ---------- RecentFiles first ----------
    public static OpenDocument Execute(RecentFiles? recents, string? path) => Core(path, null, recents);
    public static OpenDocument Execute(RecentFiles? recents, RecentFile? entry) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, null, recents);
    public static OpenDocument Execute(RecentFiles? recents, string? path, OpenDocument? current) => Core(path, current, recents);
    public static OpenDocument Execute(RecentFiles? recents, RecentFile? entry, OpenDocument? current) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, recents);

    // ---------- OpenDocument first ----------
    public static OpenDocument Execute(OpenDocument? current, string? path) => Core(path, current, null);
    public static OpenDocument Execute(OpenDocument? current, RecentFile? entry) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, null);
    public static OpenDocument Execute(OpenDocument? current, RecentFiles? recents, string? path) => Core(path, current, recents);
    public static OpenDocument Execute(OpenDocument? current, RecentFiles? recents, RecentFile? entry) => Core(entry is null ? throw new ArgumentNullException(nameof(entry)) : entry.Path, current, recents);
    public static OpenDocument Execute(OpenDocument? current, string? path, RecentFiles? recents) => Core(path, current, recents);

    // ---------- IEnumerable ----------
    public static OpenDocument Execute(IEnumerable<string>? paths) => Core(paths is null ? null : paths.Select(p => p?.Trim()).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)), null, null);
    public static OpenDocument Execute(IEnumerable<string>? paths, OpenDocument? current) => Core(paths is null ? null : paths.Select(p => p?.Trim()).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)), current, null);
    public static OpenDocument Execute(IEnumerable<string>? paths, RecentFiles? recents) => Core(paths is null ? null : paths.Select(p => p?.Trim()).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)), null, recents);
    public static OpenDocument Execute(IEnumerable<RecentFile>? entries) => Core(entries is null ? null : entries.FirstOrDefault()?.Path, null, null);
    public static OpenDocument Execute(IEnumerable<RecentFile>? entries, OpenDocument? current) => Core(entries is null ? null : entries.FirstOrDefault()?.Path, current, null);
    public static OpenDocument Execute(IEnumerable<RecentFile>? entries, RecentFiles? recents) => Core(entries is null ? null : entries.FirstOrDefault()?.Path, null, recents);

    // ---------- object fallback ----------
    public static OpenDocument Execute(object? source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (source is string s) return Core(s, null, null);
        if (source is RecentFile r) return Core(r.Path, null, null);
        if (source is RecentFiles rf) return Core(rf.Entries.FirstOrDefault()?.Path, null, rf);
        if (source is IEnumerable<RecentFile> er) return Core(er.FirstOrDefault()?.Path, null, null);
        if (source is IEnumerable<string> es) return Core(es.Select(p => p?.Trim()).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)), null, null);
        return Core(source.ToString(), null, null);
    }

    // ---------- instance ----------
    public OpenDocument ExecuteInstance(string? path) { var d = Core(path, CurrentDocument, RecentFiles); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(RecentFile? entry) { var d = Core(entry is null ? null : entry.Path, CurrentDocument, RecentFiles); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(string? path, RecentFiles? recents) { var d = Core(path, CurrentDocument, recents ?? RecentFiles); CurrentDocument = d; if (recents is not null) RecentFiles = recents; return d; }
    public OpenDocument ExecuteInstance(RecentFile? entry, RecentFiles? recents) { var d = Core(entry is null ? null : entry.Path, CurrentDocument, recents ?? RecentFiles); CurrentDocument = d; if (recents is not null) RecentFiles = recents; return d; }

    // ---------- Try* ----------
    public static bool TryExecute(string? path, out OpenDocument? document) => TryCore(path, null, null, out document);
    public static bool TryExecute(string? path, OpenDocument? current, out OpenDocument? document) => TryCore(path, current, null, out document);
    public static bool TryExecute(string? path, RecentFiles? recents, out OpenDocument? document) => TryCore(path, null, recents, out document);
    public static bool TryExecute(RecentFile? entry, out OpenDocument? document) => TryCore(entry is null ? null : entry.Path, null, null, out document);
    public static bool TryExecute(RecentFile? entry, RecentFiles? recents, out OpenDocument? document) => TryCore(entry is null ? null : entry.Path, null, recents, out document);
    public static bool TryExecute(object? source, out OpenDocument? document) 
    {
        try { document = Execute(source); return true; } catch { document = null; return false; }
    }

    private static bool TryCore(string? path, OpenDocument? current, RecentFiles? recents, out OpenDocument? document)
    {
        document = null;
        try { document = Core(path, current, recents); return true; }
        catch { document = null; return false; }
    }

    // ---------- aliases ----------
    public static OpenDocument AbrirReciente(string? path) => Execute(path);
    public static OpenDocument AbrirReciente(RecentFile? entry) => Execute(entry);
    public static OpenDocument OpenRecent(string? path) => Execute(path);
    public static OpenDocument OpenRecent(RecentFile? entry) => Execute(entry);
    public static OpenDocument OpenRecentFile(string? path) => Execute(path);
    public static OpenDocument OpenRecentEntry(RecentFile? entry) => Execute(entry);

    public static bool TryOpenRecent(string? path, out OpenDocument? document) => TryExecute(path, out document);
    public static bool TryAbrirReciente(string? path, out OpenDocument? document) => TryExecute(path, out document);

    public AbrirRecienteResult Result { get; set; } = AbrirRecienteResult.Completed;
    public AbrirRecienteStatus Status { get => (AbrirRecienteStatus)Result; set => Result = (AbrirRecienteResult)value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}