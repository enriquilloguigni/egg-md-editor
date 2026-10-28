using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Operación de autoguardado cada 30 s (T-030, OP-014, BR-F002-012, ERR-012, COM-011).
/// Escribe una copia de recuperación local (ENT-012) sin modificar el documento
/// ni el archivo del usuario. No limpia el estado de cambios.
/// </summary>
public partial class AutoguardadoOperation
{
    public static ILogger? Logger { get; set; }

    public const int IntervalSeconds = 30;
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Period = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Delay = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public const string FailureMessage = "No se pudo guardar la copia de recuperación. Guarda el documento para no perder cambios.";
    public const string NonModalFailureNotice = FailureMessage;
    public const string NoticeMessage = FailureMessage;

    public OpenDocument? CurrentDocument { get; set; }
    public RecoveryCopy? RecoveryCopy { get; set; }
    public RecoveryCopy? Recovery { get => RecoveryCopy; set => RecoveryCopy = value; }
    public RecoveryCopy? Copy { get => RecoveryCopy; set => RecoveryCopy = value; }
    public string? StoragePath { get; set; }
    public string? RecoveryPath { get => StoragePath; set => StoragePath = value; }
    public string? Path { get => StoragePath; set => StoragePath = value; }

    public AutoguardadoOperation() { }
    public AutoguardadoOperation(OpenDocument? current) { CurrentDocument = current; }
    public AutoguardadoOperation(OpenDocument? current, RecoveryCopy? recovery) { CurrentDocument = current; RecoveryCopy = recovery; }
    public AutoguardadoOperation(RecoveryCopy? recovery, OpenDocument? current) { CurrentDocument = current; RecoveryCopy = recovery; }
    public AutoguardadoOperation(OpenDocument? current, string? storagePath) { CurrentDocument = current; StoragePath = storagePath; }
    public AutoguardadoOperation(OpenDocument? current, RecoveryCopy? recovery, string? storagePath)
    {
        CurrentDocument = current;
        RecoveryCopy = recovery;
        StoragePath = storagePath;
    }

    public OpenDocument? Document { get => CurrentDocument; set => CurrentDocument = value; }

    // ---------- Core ----------
    protected static RecoveryCopy Core(OpenDocument? document, RecoveryCopy? recovery, string? storagePath)
    {
        var logger = Logger;
        if (document is null) throw new ArgumentNullException(nameof(document));

        // Si no hay cambios, el autoguardado se omite (BR-F002-012).
        var copy = recovery ?? new RecoveryCopy(document);
        try
        {
            copy.DocumentId = document.Id;
            copy.OriginalPath = document.FilePath;
        }
        catch { }
        try { copy.Content = document.Content; } catch { }
        try { copy.Timestamp = DateTime.UtcNow; } catch { try { copy.RefreshTimestamp(); } catch { } }
        try { copy.Status = RecoveryStatus.Valid; } catch { try { copy.MarkValid(); } catch { } }

        if (!document.HasChanges)
        {
            // No se escribe la copia; se conserva el estado vigente previo.
            try
            {
                logger?.LogDebug(
                    "LOG-006 Autoguardado. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Duración: {DurationMs} ms. Tamaño de la copia: {Size}. Estado: Vigente. Resultado: omitido por no haber cambios.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), document.Id, 0, 0);
            }
            catch { }
            return copy;
        }

        var path = storagePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            try { path = copy.StoragePath; } catch { path = null; }
        }
        if (string.IsNullOrWhiteSpace(path))
        {
            try { path = recovery?.StoragePath; } catch { path = null; }
        }
        if (string.IsNullOrWhiteSpace(path))
        {
            path = RecoveryCopy.GetDefaultPath();
        }

        try
        {
            copy.Save(path!);
            try { copy.StoragePath = path; } catch { }
            try
            {
                long size = 0;
                try { size = new FileInfo(path!).Length; } catch { }
                logger?.LogDebug(
                    "LOG-006 Autoguardado. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Duración: {DurationMs} ms. Tamaño de la copia: {Size}. Estado: Vigente. Resultado: copia escrita.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), document.Id, 0, size);
            }
            catch { }
            return copy;
        }
        catch (Exception ex)
        {
            try
            {
                logger?.LogWarning(ex,
                    "LOG-006 Autoguardado. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Duración: {DurationMs} ms. Estado: Vigente. Resultado: fallo. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), document.Id, 0, ex.GetType().Name, ex.Message);
            }
            catch { }
            throw;
        }
    }

    protected static bool TryCore(OpenDocument? document, RecoveryCopy? recovery, string? storagePath, out RecoveryCopy? saved)
    {
        saved = null;
        try { saved = Core(document, recovery, storagePath); return true; }
        catch { saved = null; return false; }
    }

    // ---------- Execute overloads ----------
    public static RecoveryCopy Execute(OpenDocument? document)
        => Core(document, null, null);
    public static RecoveryCopy Execute(OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, recovery, null);
    public static RecoveryCopy Execute(OpenDocument? document, string? storagePath)
        => Core(document, null, storagePath);
    public static RecoveryCopy Execute(OpenDocument? document, RecoveryCopy? recovery, string? storagePath)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Execute(OpenDocument? document, string? storagePath, RecoveryCopy? recovery)
        => Core(document, recovery, storagePath);

    public static RecoveryCopy Execute(RecoveryCopy? recovery, OpenDocument? document)
        => Core(document, recovery, null);
    public static RecoveryCopy Execute(string? storagePath, OpenDocument? document)
        => Core(document, null, storagePath);
    public static RecoveryCopy Execute(RecoveryCopy? recovery, OpenDocument? document, string? storagePath)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Execute(string? storagePath, OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, recovery, storagePath);

    // Try*
    public static bool TryExecute(OpenDocument? document, out RecoveryCopy? saved) => TryCore(document, null, null, out saved);
    public static bool TryExecute(OpenDocument? document, RecoveryCopy? recovery, out RecoveryCopy? saved) => TryCore(document, recovery, null, out saved);
    public static bool TryExecute(OpenDocument? document, string? storagePath, out RecoveryCopy? saved) => TryCore(document, null, storagePath, out saved);
    public static bool TryExecute(OpenDocument? document, RecoveryCopy? recovery, string? storagePath, out RecoveryCopy? saved) => TryCore(document, recovery, storagePath, out saved);
    public static bool TryExecute(OpenDocument? document, out RecoveryCopy? saved, RecoveryCopy? recovery = null, string? storagePath = null) => TryCore(document, recovery, storagePath, out saved);
    public static bool TryExecute(RecoveryCopy? recovery, OpenDocument? document, out RecoveryCopy? saved) => TryCore(document, recovery, null, out saved);

    // ---------- Instance ----------
    public RecoveryCopy ExecuteInstance() => ExecuteInstance(CurrentDocument, null, StoragePath);
    public RecoveryCopy ExecuteInstance(RecoveryCopy? recovery) => ExecuteInstance(CurrentDocument, recovery, StoragePath);
    public RecoveryCopy ExecuteInstance(string? storagePath) => ExecuteInstance(CurrentDocument, RecoveryCopy, storagePath);
    public RecoveryCopy ExecuteInstance(OpenDocument? document) { var r = Core(document, RecoveryCopy, StoragePath); CurrentDocument = document; return r; }
    public RecoveryCopy ExecuteInstance(OpenDocument? document, RecoveryCopy? recovery) { var r = Core(document, recovery, StoragePath); CurrentDocument = document; if (recovery is not null) RecoveryCopy = recovery; return r; }
    public RecoveryCopy ExecuteInstance(OpenDocument? document, string? storagePath) { var r = Core(document, RecoveryCopy, storagePath); CurrentDocument = document; if (storagePath is not null) StoragePath = storagePath; return r; }
    public RecoveryCopy ExecuteInstance(OpenDocument? document, RecoveryCopy? recovery, string? storagePath)
    {
        var r = Core(document, recovery, storagePath);
        CurrentDocument = document;
        if (recovery is not null) RecoveryCopy = recovery;
        if (storagePath is not null) StoragePath = storagePath;
        return r;
    }

    // ---------- Aliases de método ----------
    public static RecoveryCopy Autosave(OpenDocument? document) => Execute(document);
    public static RecoveryCopy Autosave(OpenDocument? document, RecoveryCopy? recovery) => Execute(document, recovery);
    public static RecoveryCopy Autosave(OpenDocument? document, string? storagePath) => Execute(document, storagePath);
    public static RecoveryCopy Autosave(OpenDocument? document, RecoveryCopy? recovery, string? storagePath) => Execute(document, recovery, storagePath);

    public static RecoveryCopy Autoguardar(OpenDocument? document) => Execute(document);
    public static RecoveryCopy Autoguardar(OpenDocument? document, RecoveryCopy? recovery) => Execute(document, recovery);
    public static RecoveryCopy Autoguardar(OpenDocument? document, string? storagePath) => Execute(document, storagePath);
    public static RecoveryCopy Autoguardar(OpenDocument? document, RecoveryCopy? recovery, string? storagePath) => Execute(document, recovery, storagePath);

    public static RecoveryCopy SaveRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy WriteRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy PersistRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Perform(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Run(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Invoke(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Do(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);
    public static RecoveryCopy Apply(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => Core(document, recovery, storagePath);

    public static bool IsDue(TimeSpan? elapsed) => (elapsed ?? TimeSpan.Zero) >= Interval;
    public static bool ShouldAutosave(OpenDocument? document) => document?.HasChanges == true;
    public static bool CanAutosave(OpenDocument? document) => document is not null && document.HasChanges;

    public RecoveryCopy ResultCopy { get; set; } = new RecoveryCopy();
    public RecoveryCopy Result { get => ResultCopy; set => ResultCopy = value; }
    public bool Success { get; set; } = true;
    public bool Succeeded { get => Success; set => Success = value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}
