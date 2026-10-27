using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EggMdEditor.App;

/// <summary>
/// Operación Guardar como (T-028, OP-013, BR-F002-006).
/// </summary>
public class GuardarComoOperation : GuardarOperation
{
    public GuardarComoOperation() : base() { }
    public GuardarComoOperation(OpenDocument? current) : base(current) { }
    public GuardarComoOperation(OpenDocument? current, RecentFiles? recents) : base(current, recents) { }
    public GuardarComoOperation(RecentFiles? recents, OpenDocument? current) : base(recents, current) { }
    public GuardarComoOperation(OpenDocument? current, RecentFiles? recents, RecoveryCopy? recovery) : base(current, recents, recovery) { }

    // ---------- Execute overloads (SaveAs) ----------
    public new static OpenDocument Execute(OpenDocument? document)
        => Core(document, null, null, null, true);
    public new static OpenDocument Execute(OpenDocument? document, string? path)
        => Core(document, path, null, null, true);
    public new static OpenDocument Execute(OpenDocument? document, string? path, RecentFiles? recents)
        => Core(document, path, recents, null, true);
    public new static OpenDocument Execute(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, string? path, RecoveryCopy? recovery)
        => Core(document, path, null, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, string? path, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, path, recents, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecentFiles? recents)
        => Core(document, null, recents, null, true);
    public new static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, string? path)
        => Core(document, path, recents, null, true);
    public new static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, null, recents, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecentFiles? recents, string? path, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, null, null, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, null, recents, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, string? path)
        => Core(document, path, null, recovery, true);
    public new static OpenDocument Execute(OpenDocument? document, RecoveryCopy? recovery, string? path, RecentFiles? recents)
        => Core(document, path, recents, recovery, true);

    public new static OpenDocument Execute(string? path, OpenDocument? document)
        => Core(document, path, null, null, true);
    public new static OpenDocument Execute(string? path, OpenDocument? document, RecentFiles? recents)
        => Core(document, path, recents, null, true);
    public new static OpenDocument Execute(string? path, OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, true);
    public new static OpenDocument Execute(string? path, OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, path, null, recovery, true);

    public new static OpenDocument Execute(RecentFiles? recents, OpenDocument? document)
        => Core(document, null, recents, null, true);
    public new static OpenDocument Execute(RecentFiles? recents, OpenDocument? document, string? path)
        => Core(document, path, recents, null, true);
    public new static OpenDocument Execute(RecentFiles? recents, OpenDocument? document, string? path, RecoveryCopy? recovery)
        => Core(document, path, recents, recovery, true);

    public new static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document)
        => Core(document, null, null, recovery, true);
    public new static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document, string? path)
        => Core(document, path, null, recovery, true);
    public new static OpenDocument Execute(RecoveryCopy? recovery, OpenDocument? document, string? path, RecentFiles? recents)
        => Core(document, path, recents, recovery, true);

    public new static bool TryExecute(OpenDocument? document, out OpenDocument? saved) => TryCore(document, null, null, null, true, out saved);
    public new static bool TryExecute(OpenDocument? document, string? path, out OpenDocument? saved) => TryCore(document, path, null, null, true, out saved);
    public new static bool TryExecute(OpenDocument? document, string? path, RecentFiles? recents, out OpenDocument? saved) => TryCore(document, path, recents, null, true, out saved);
    public new static bool TryExecute(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery, out OpenDocument? saved) => TryCore(document, path, recents, recovery, true, out saved);
    public new static bool TryExecute(string? path, OpenDocument? document, out OpenDocument? saved) => TryCore(document, path, null, null, true, out saved);

    // ---------- Aliases de método ----------
    public static OpenDocument GuardarComo(OpenDocument? document) => Execute(document);
    public static OpenDocument GuardarComo(OpenDocument? document, string? path) => Execute(document, path);
    public static OpenDocument GuardarComo(OpenDocument? document, string? path, RecentFiles? recents) => Execute(document, path, recents);
    public new static OpenDocument GuardarComo(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery) => Execute(document, path, recents, recovery);

    public static OpenDocument SaveAs(OpenDocument? document) => Execute(document);
    public static OpenDocument SaveAs(OpenDocument? document, string? path) => Execute(document, path);
    public static OpenDocument SaveAs(OpenDocument? document, string? path, RecentFiles? recents) => Execute(document, path, recents);
    public new static OpenDocument SaveAs(OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery) => Execute(document, path, recents, recovery);

    public static OpenDocument SaveCopy(OpenDocument? document, string? path = null) => Execute(document, path);
    public static OpenDocument SaveAsCopy(OpenDocument? document, string? path = null) => Execute(document, path);
    public new static OpenDocument Copy(OpenDocument? document, string? path = null) => Execute(document, path);
    public static OpenDocument SaveTo(OpenDocument? document, string? path = null) => Execute(document, path);

    public new GuardarComoResult Result { get; set; } = GuardarComoResult.Completed;
    public new GuardarComoStatus Status { get => (GuardarComoStatus)Result; set => Result = (GuardarComoResult)value; }
}

public enum GuardarComoResult
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
    Completada = 0,
    Cancelada = 1,
    ErrorOperacion = 2,
}

public enum GuardarComoStatus
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
}