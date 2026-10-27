using System;
using System.Collections.Generic;

namespace EggMdEditor.App;

public partial class AbrirRecienteOperation
{
    public OpenDocument Document { get => CurrentDocument ?? new OpenDocument(); set => CurrentDocument = value; }

    public static OpenDocument Abrir(string? path) => Execute(path);
    public static OpenDocument Abrir(RecentFile? entry) => Execute(entry);
    public static OpenDocument Open(string? path) => Execute(path);
    public static OpenDocument Open(RecentFile? entry) => Execute(entry);
    public static OpenDocument Load(string? path) => Execute(path);
    public static OpenDocument Load(RecentFile? entry) => Execute(entry);
    public static OpenDocument Read(string? path) => Execute(path);
    public static OpenDocument Read(RecentFile? entry) => Execute(entry);

    public static OpenDocument AbrirRecienteArchivo(string? path) => Execute(path);
    public static OpenDocument AbrirArchivoReciente(string? path) => Execute(path);
    public static OpenDocument AbrirRecienteDocumento(string? path) => Execute(path);
    public static OpenDocument OpenRecentDocument(string? path) => Execute(path);
    public static OpenDocument OpenRecentFileAlias(string? path) => Execute(path);
    public static OpenDocument OpenRecentMd(string? path) => Execute(path);
    public static OpenDocument OpenRecentMarkdown(string? path) => Execute(path);

    public static OpenDocument ExecuteRecent(string? path) => Execute(path);
    public static OpenDocument ExecuteRecent(RecentFile? entry) => Execute(entry);
    public static OpenDocument ExecuteReciente(string? path) => Execute(path);
    public static OpenDocument ExecuteReciente(RecentFile? entry) => Execute(entry);

    public static OpenDocument FromRecent(string? path) => Execute(path);
    public static OpenDocument FromRecent(RecentFile? entry) => Execute(entry);
    public static OpenDocument FromReciente(string? path) => Execute(path);
    public static OpenDocument FromReciente(RecentFile? entry) => Execute(entry);

    public static bool TryExecuteRecent(string? path, out OpenDocument? document) => TryExecute(path, out document);
    public static bool TryOpenRecentEntry(string? path, out OpenDocument? document) => TryExecute(path, out document);

    public OpenDocument ExecuteInstanceRecent(string? path) => ExecuteInstance(path);
    public OpenDocument ExecuteInstanceRecent(RecentFile? entry) => ExecuteInstance(entry);
}