using System;
using System.Collections.Generic;
using System.Linq;

namespace EggMdEditor.App;

public partial class GuardarOperation
{
    public OpenDocument Document { get => CurrentDocument ?? new OpenDocument(); set => CurrentDocument = value; }

    public static OpenDocument Guardar(OpenDocument? document) => Execute(document);
    public static OpenDocument Guardar(OpenDocument? document, string? path) => Execute(document, path);
    public static OpenDocument Guardar(OpenDocument? document, string? path, RecentFiles? recents) => Execute(document, path, recents);
    public static OpenDocument Guardar(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery) => Execute(document, path, recents, recovery);

    public static OpenDocument Save(OpenDocument? document) => Execute(document);
    public static OpenDocument Save(OpenDocument? document, string? path) => Execute(document, path);
    public static OpenDocument Save(OpenDocument? document, string? path, RecentFiles? recents) => Execute(document, path, recents);
    public static OpenDocument Save(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery) => Execute(document, path, recents, recovery);

    public static OpenDocument SaveDocument(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument SaveFile(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument SaveCurrent(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument SaveTo(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument Write(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument Persist(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument Flush(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);
    public static OpenDocument Commit(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, path, recents, recovery);

    public static OpenDocument SaveAs(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument GuardarComo(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);

    public OpenDocument ExecuteInstanceSave(string? path = null) { var d = Execute(CurrentDocument, path, RecentFiles, RecoveryCopy); CurrentDocument = d; return d; }
}