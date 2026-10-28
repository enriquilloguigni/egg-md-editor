using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class AbrirOperation
{
    public static ILogger? Logger { get; set; }
    public ILogger? InstanceLogger { get; set; }
    public ILogger? LoggerInstance { get => InstanceLogger; set => InstanceLogger = value; }

    public OpenDocument? CurrentDocument { get; set; }

    public AbrirOperation() { }
    public AbrirOperation(OpenDocument? current) { CurrentDocument = current; }

    public const string DropValidationMessage = "Suelta un único archivo .md.";

    public static OpenDocument Execute(string? path)
    {
        var logger = Logger;
        string? trimmed = null;
        long bytes = 0;
        try
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
            trimmed = path!.Trim();
            if (!IsValidExtension(trimmed)) throw new ArgumentException(DropValidationMessage, nameof(path));
            if (!File.Exists(trimmed)) throw new FileNotFoundException("No se pudo abrir: el archivo no existe o la ruta no está disponible.", trimmed);
            bytes = new FileInfo(trimmed).Length;
            string content;
            try { content = FileReader.Read(trimmed); }
            catch (Exception ex) when (ex is not ArgumentException && ex is not FileNotFoundException && ex is not UnauthorizedAccessException)
            { throw new InvalidOperationException("No se pudo abrir: el archivo no está en UTF-8.", ex); }
            var doc = new OpenDocument(content, trimmed, DocumentOrigin.Open, DocumentLastOperation.Open);
            doc.MarkClean();
            try
            {
                logger?.LogInformation(
                    "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Operación: Abrir. Extensión: {Extension}. Bytes leídos: {BytesRead}. Codificación: {Encoding}. Fin de línea: {LineEnding}. Duración: {DurationMs} ms. Resultado: éxito.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), doc.Id, Path.GetExtension(trimmed), bytes, "UTF-8", "LF", 0);
            }
            catch { }
            return doc;
        }
        catch (Exception ex)
        {
            try
            {
                if (ex is FileNotFoundException || ex is ArgumentException)
                {
                    logger?.LogWarning(ex,
                        "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Operación: Abrir. Extensión: {Extension}. Bytes leídos: {BytesRead}. Resultado: validación rechazada. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                        DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), Guid.Empty, trimmed is null ? string.Empty : Path.GetExtension(trimmed), bytes, ex.GetType().Name, ex.Message);
                }
                else
                {
                    logger?.LogError(ex,
                        "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Operación: Abrir. Extensión: {Extension}. Bytes leídos: {BytesRead}. Resultado: fallo. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                        DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), Guid.Empty, trimmed is null ? string.Empty : Path.GetExtension(trimmed), bytes, ex.GetType().Name, ex.Message);
                }
            }
            catch { }
            throw;
        }
    }

    public static OpenDocument Execute(string? path, OpenDocument? current) => Execute(path);
    public static OpenDocument Execute(OpenDocument? current, string? path) => Execute(path);

    public static OpenDocument Execute(IEnumerable<string>? paths) => Execute(paths, null);
    public static OpenDocument Execute(IEnumerable<string>? paths, OpenDocument? current)
    {
        var logger = Logger;
        try
        {
            if (!ValidateDrop(paths)) throw new ArgumentException(DropValidationMessage, nameof(paths));
            var path = paths!.Select(p => p?.Trim()).First(p => !string.IsNullOrWhiteSpace(p))!;
            var doc = Execute(path, current);
            doc.Origin = DocumentOrigin.Drag;
            doc.Source = DocumentOrigin.Drag;
            doc.DocumentOrigin = DocumentOrigin.Drag;
            try
            {
                logger?.LogInformation(
                    "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Operación: Arrastrar. Extensión: {Extension}. Resultado: éxito.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), doc.Id, Path.GetExtension(path));
            }
            catch { }
            return doc;
        }
        catch (Exception ex)
        {
            try
            {
                logger?.LogWarning(ex,
                    "LOG-004 Abrir archivo. Hora: {Timestamp:O}. Sesión: {SessionId}. Operación: Arrastrar. Resultado: validación rechazada. Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), ex.GetType().Name, ex.Message);
            }
            catch { }
            throw;
        }
    }
    public static OpenDocument Execute(OpenDocument? current, IEnumerable<string>? paths) => Execute(paths, current);

    public static bool IsValidExtension(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return string.Equals(Path.GetExtension(path!.Trim()), ".md", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        var t = path!.Trim();
        return IsValidExtension(t) && File.Exists(t);
    }
    public static bool IsValid(string? path) => IsValidPath(path);
    public static bool IsValidFile(string? path) => IsValidPath(path);
    public static bool IsMarkdown(string? path) => IsValidExtension(path);
    public static bool IsMd(string? path) => IsValidExtension(path);
    public static bool IsValidMd(string? path) => IsValidExtension(path);

    public static bool ValidateDrop(IEnumerable<string>? paths)
    {
        if (paths is null) return false;
        var list = paths.Select(p => p?.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (list.Count != 1) return false;
        return IsValidExtension(list[0]);
    }
    public static bool ValidateDrag(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool ValidateDragDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool IsValidDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool IsValidDrag(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool IsValidDragDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool CanDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool CanDragDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool AcceptsDrop(IEnumerable<string>? paths) => ValidateDrop(paths);
    public static bool AcceptsDrag(IEnumerable<string>? paths) => ValidateDrop(paths);

    public static bool CanOpen(string? path)
    {
        if (!IsValidPath(path)) return false;
        try { FileReader.Read(path!.Trim()); return true; }
        catch { return false; }
    }

    public static bool TryExecute(string? path, out OpenDocument? document)
    {
        document = null;
        try { document = Execute(path); return true; }
        catch { return false; }
    }
    public static bool TryExecute(IEnumerable<string>? paths, out OpenDocument? document)
    {
        document = null;
        try { document = Execute(paths); return true; }
        catch { return false; }
    }

    public static bool TryOpen(string? path, out OpenDocument? document) => TryExecute(path, out document);
    public static bool TryAbrir(string? path, out OpenDocument? document) => TryExecute(path, out document);
    public static bool TryLoad(string? path, out OpenDocument? document) => TryExecute(path, out document);
    public static bool TryRead(string? path, out OpenDocument? document) => TryExecute(path, out document);
}