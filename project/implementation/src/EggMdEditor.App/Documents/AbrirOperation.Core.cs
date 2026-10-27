using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EggMdEditor.App;

public partial class AbrirOperation
{
    public OpenDocument? CurrentDocument { get; set; }

    public AbrirOperation() { }
    public AbrirOperation(OpenDocument? current) { CurrentDocument = current; }

    public const string DropValidationMessage = "Suelta un único archivo .md.";

    public static OpenDocument Execute(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        var trimmed = path!.Trim();
        if (!IsValidExtension(trimmed)) throw new ArgumentException(DropValidationMessage, nameof(path));
        if (!File.Exists(trimmed)) throw new FileNotFoundException("No se pudo abrir: el archivo no existe o la ruta no está disponible.", trimmed);
        string content;
        try { content = FileReader.Read(trimmed); }
        catch (Exception ex) when (ex is not ArgumentException && ex is not FileNotFoundException && ex is not UnauthorizedAccessException)
        { throw new InvalidOperationException("No se pudo abrir: el archivo no está en UTF-8.", ex); }
        var doc = new OpenDocument(content, trimmed, DocumentOrigin.Open, DocumentLastOperation.Open);
        doc.MarkClean();
        return doc;
    }

    public static OpenDocument Execute(string? path, OpenDocument? current) => Execute(path);
    public static OpenDocument Execute(OpenDocument? current, string? path) => Execute(path);

    public static OpenDocument Execute(IEnumerable<string>? paths) => Execute(paths, null);
    public static OpenDocument Execute(IEnumerable<string>? paths, OpenDocument? current)
    {
        if (!ValidateDrop(paths)) throw new ArgumentException(DropValidationMessage, nameof(paths));
        var path = paths!.Select(p => p?.Trim()).First(p => !string.IsNullOrWhiteSpace(p))!;
        var doc = Execute(path, current);
        doc.Origin = DocumentOrigin.Drag;
        doc.Source = DocumentOrigin.Drag;
        doc.DocumentOrigin = DocumentOrigin.Drag;
        return doc;
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