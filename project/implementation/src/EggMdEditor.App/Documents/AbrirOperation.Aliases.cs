using System;
using System.Collections.Generic;
using System.Linq;

namespace EggMdEditor.App;

public partial class AbrirOperation
{
    public OpenDocument Document { get => CurrentDocument ?? new OpenDocument(); set => CurrentDocument = value; }

    public static OpenDocument Abrir(string? path) => Execute(path);
    public static OpenDocument Open(string? path) => Execute(path);
    public static OpenDocument Load(string? path) => Execute(path);
    public static OpenDocument Read(string? path) => Execute(path);
    public static OpenDocument OpenFile(string? path) => Execute(path);
    public static OpenDocument OpenDocument(string? path) => Execute(path);
    public static OpenDocument OpenMarkdown(string? path) => Execute(path);
    public static OpenDocument OpenMd(string? path) => Execute(path);
    public static OpenDocument OpenMdFile(string? path) => Execute(path);
    public static OpenDocument OpenMarkdownFile(string? path) => Execute(path);
    public static OpenDocument LoadFile(string? path) => Execute(path);
    public static OpenDocument LoadDocument(string? path) => Execute(path);
    public static OpenDocument LoadMarkdown(string? path) => Execute(path);
    public static OpenDocument LoadMd(string? path) => Execute(path);
    public static OpenDocument ReadFile(string? path) => Execute(path);
    public static OpenDocument ReadDocument(string? path) => Execute(path);
    public static OpenDocument ReadMarkdown(string? path) => Execute(path);
    public static OpenDocument ReadMd(string? path) => Execute(path);
    public static OpenDocument FromFile(string? path) => Execute(path);
    public static OpenDocument FromPath(string? path) => Execute(path);
    public static OpenDocument FromString(string? path) => Execute(path);

    public static OpenDocument OpenFromSelector(string? path) => Execute(path);
    public static OpenDocument OpenFromDialog(string? path) => Execute(path);
    public static OpenDocument OpenFromPicker(string? path) => Execute(path);
    public static OpenDocument OpenSelected(string? path) => Execute(path);
    public static OpenDocument OpenChosen(string? path) => Execute(path);

    public static OpenDocument AbrirDesdeSelector(string? path) => Execute(path);
    public static OpenDocument AbrirArchivo(string? path) => Execute(path);
    public static OpenDocument AbrirDocumento(string? path) => Execute(path);
    public static OpenDocument AbrirMarkdown(string? path) => Execute(path);
    public static OpenDocument AbrirMd(string? path) => Execute(path);

    public static OpenDocument Execute(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);
    public static OpenDocument Execute(RecentFile? entry, OpenDocument? current) => AbrirRecienteOperation.Execute(entry, current);
    public static OpenDocument Execute(RecentFile? entry, RecentFiles? recents) => AbrirRecienteOperation.Execute(entry, recents);
    public static OpenDocument Execute(RecentFiles? recents, RecentFile? entry) => AbrirRecienteOperation.Execute(recents, entry);

    public static OpenDocument ExecuteDrag(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument ExecuteDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument ExecuteDragDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument ExecuteDragAndDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenByDrag(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenByDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenFromDrag(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenFromDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenFromDragDrop(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenDropped(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument OpenDragged(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument DragOpen(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument DropOpen(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument AbrirArrastrando(IEnumerable<string>? paths) => Execute(paths);
    public static OpenDocument AbrirPorArrastre(IEnumerable<string>? paths) => Execute(paths);

    public OpenDocument ExecuteInstance(string? path) { var d = Execute(path); CurrentDocument = d; return d; }
    public OpenDocument ExecuteInstance(IEnumerable<string>? paths) { var d = Execute(paths); CurrentDocument = d; return d; }

    public static OpenDocument ExecuteRecent(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument ExecuteRecent(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);
    public static OpenDocument AbrirReciente(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument AbrirReciente(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);
    public static OpenDocument OpenRecent(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument OpenRecent(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);

    public AbrirResult Result { get; set; } = AbrirResult.Completed;
    public AbrirStatus Status { get => (AbrirStatus)Result; set => Result = (AbrirResult)value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}