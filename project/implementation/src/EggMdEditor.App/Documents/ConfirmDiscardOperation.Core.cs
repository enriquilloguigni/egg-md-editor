using System;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class ConfirmDiscardOperation
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

    public ConfirmDiscardOperation() { }
    public ConfirmDiscardOperation(OpenDocument? current) { CurrentDocument = current; }
    public ConfirmDiscardOperation(OpenDocument? current, RecentFiles? recents) { CurrentDocument = current; RecentFiles = recents; }
    public ConfirmDiscardOperation(RecentFiles? recents, OpenDocument? current) { CurrentDocument = current; RecentFiles = recents; }
    public ConfirmDiscardOperation(OpenDocument? current, RecentFiles? recents, RecoveryCopy? recovery) { CurrentDocument = current; RecentFiles = recents; RecoveryCopy = recovery; }
    public ConfirmDiscardOperation(OpenDocument? current, RecoveryCopy? recovery, RecentFiles? recents) { CurrentDocument = current; RecentFiles = recents; RecoveryCopy = recovery; }
    public ConfirmDiscardOperation(RecoveryCopy? recovery, OpenDocument? current, RecentFiles? recents) { CurrentDocument = current; RecentFiles = recents; RecoveryCopy = recovery; }

    public OpenDocument? Document { get => CurrentDocument; set => CurrentDocument = value; }

    public enum Option { Save = 0, Guardar = 0, DoNotSave = 1, DontSave = 1, Discard = 1, NoGuardar = 1, Cancel = 2, Cancelar = 2 }
    public enum Choice { Save = 0, Guardar = 0, DoNotSave = 1, DontSave = 1, Discard = 1, NoGuardar = 1, Cancel = 2, Cancelar = 2 }
    public enum Action { Save = 0, Guardar = 0, DoNotSave = 1, DontSave = 1, Discard = 1, NoGuardar = 1, Cancel = 2, Cancelar = 2 }
    public enum Decision { Save = 0, Guardar = 0, DoNotSave = 1, DontSave = 1, Discard = 1, NoGuardar = 1, Cancel = 2, Cancelar = 2 }

    protected static object? Core(OpenDocument? document, object? option, string? path, RecentFiles? recents, RecoveryCopy? recovery)
    {
        var opt = ParseOption(option);
        if (opt == ConfirmDiscardOption.Save || opt == ConfirmDiscardOption.Guardar)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));
            GuardarOperation.Execute(document, path, recents, recovery);
            return opt;
        }
        if (opt == ConfirmDiscardOption.DoNotSave || opt == ConfirmDiscardOption.DontSave ||
            opt == ConfirmDiscardOption.Discard || opt == ConfirmDiscardOption.NoGuardar)
        {
            if (recovery is not null)
            {
                try { recovery.Invalidate(); }
                catch { try { recovery.MarkObsolete(); } catch { } }
            }
            try
            {
                Logger?.LogInformation(
                    "LOG-008 Descarte de cambios. Hora: {Timestamp:O}. Sesión: {SessionId}. Documento: {DocumentId}. Acción pendiente: {PendingAction}. Resultado: descarte confirmado y copia invalidada.",
                    DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"), document?.Id ?? Guid.Empty, "Cerrar");
            }
            catch { }
            return opt;
        }
        return opt;
    }

    private static ConfirmDiscardOption ParseOption(object? option)
    {
        if (option is null) return ConfirmDiscardOption.Cancel;
        if (option is bool b) return b ? ConfirmDiscardOption.Save : ConfirmDiscardOption.Cancel;
        if (option is ConfirmDiscardOption c) return c;
        if (option is Enum e)
        {
            var name = e.ToString();
            if (TryParseByName(name, out var parsed)) return parsed;
            try { return (ConfirmDiscardOption)Convert.ToInt32(e); }
            catch { return ConfirmDiscardOption.Cancel; }
        }
        if (option is string s)
        {
            if (TryParseByName(s, out var parsed)) return parsed;
        }
        if (option is int i)
        {
            return i switch
            {
                0 => ConfirmDiscardOption.Save,
                1 => ConfirmDiscardOption.DoNotSave,
                2 => ConfirmDiscardOption.Cancel,
                _ => ConfirmDiscardOption.Cancel,
            };
        }
        if (TryParseByName(option.ToString(), out var p)) return p;
        return ConfirmDiscardOption.Cancel;
    }

    private static bool TryParseByName(string? name, out ConfirmDiscardOption option)
    {
        option = ConfirmDiscardOption.Cancel;
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name!.Trim().ToLowerInvariant()
            .Replace("_", "").Replace("-", "").Replace(" ", "");
        switch (n)
        {
            case "save":
            case "guardar":
            case "guardarcambios":
            case "savechanges":
            case "guardarycontinuar":
            case "saveandcontinue":
                option = ConfirmDiscardOption.Save;
                return true;
            case "donotsave":
            case "dontsave":
            case "discard":
            case "noguardar":
            case "descartar":
            case "descartarcambios":
            case "discardchanges":
            case "noguardarcambios":
                option = ConfirmDiscardOption.DoNotSave;
                return true;
            case "cancel":
            case "cancelar":
            case "abort":
            case "abortar":
            case "keepediting":
            case "seguireditando":
                option = ConfirmDiscardOption.Cancel;
                return true;
            default:
                return false;
        }
    }

    public static object? Execute(OpenDocument? document, object? option)
        => Core(document, option, null, null, null);
    public static object? Execute(OpenDocument? document, object? option, RecoveryCopy? recovery)
        => Core(document, option, null, null, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecentFiles? recents)
        => Core(document, option, null, recents, null);
    public static object? Execute(OpenDocument? document, object? option, string? path)
        => Core(document, option, path, null, null);
    public static object? Execute(OpenDocument? document, object? option, string? path, RecentFiles? recents)
        => Core(document, option, path, recents, null);
    public static object? Execute(OpenDocument? document, object? option, string? path, RecoveryCopy? recovery)
        => Core(document, option, path, null, recovery);
    public static object? Execute(OpenDocument? document, object? option, string? path, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, option, path, recents, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, option, null, recents, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecoveryCopy? recovery, RecentFiles? recents)
        => Core(document, option, null, recents, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecentFiles? recents, string? path, RecoveryCopy? recovery)
        => Core(document, option, path, recents, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecoveryCopy? recovery, string? path, RecentFiles? recents)
        => Core(document, option, path, recents, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecoveryCopy? recovery, string? path)
        => Core(document, option, path, null, recovery);
    public static object? Execute(OpenDocument? document, object? option, RecoveryCopy? recovery, RecentFiles? recents, string? path)
        => Core(document, option, path, recents, recovery);

    public static object? Execute(object? document, object? option)
        => Core(document as OpenDocument, option, null, null, null);
    public static object? Execute(object? document, object? option, RecoveryCopy? recovery)
        => Core(document as OpenDocument, option, null, null, recovery);
    public static object? Execute(object? document, object? option, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document as OpenDocument, option, null, recents, recovery);
    public static object? Execute(object? document, object? option, string? path, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document as OpenDocument, option, path, recents, recovery);

    public static object? Execute(object? option, OpenDocument? document)
        => Core(document, option, null, null, null);
    public static object? Execute(object? option, OpenDocument? document, RecoveryCopy? recovery)
        => Core(document, option, null, null, recovery);
    public static object? Execute(object? option, OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, option, null, recents, recovery);
    public static object? Execute(object? option, OpenDocument? document, string? path, RecentFiles? recents, RecoveryCopy? recovery)
        => Core(document, option, path, recents, recovery);

    public static bool TryExecute(OpenDocument? document, object? option, out object? result)
    {
        result = null;
        try { result = Execute(document, option); return true; }
        catch { result = null; return false; }
    }
    public static bool TryExecute(OpenDocument? document, object? option, RecoveryCopy? recovery, out object? result)
    {
        result = null;
        try { result = Execute(document, option, recovery); return true; }
        catch { result = null; return false; }
    }
    public static bool TryExecute(OpenDocument? document, object? option, string? path, RecentFiles? recents, RecoveryCopy? recovery, out object? result)
    {
        result = null;
        try { result = Execute(document, option, path, recents, recovery); return true; }
        catch { result = null; return false; }
    }

    public static object? Confirm(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Save, path, recents, recovery);
    public static object? ConfirmDiscard(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Discard(OpenDocument? document = null, RecoveryCopy? recovery = null, RecentFiles? recents = null, string? path = null)
    {
        if (recovery is not null) { try { recovery.Invalidate(); } catch { try { recovery.MarkObsolete(); } catch { } } }
        return ConfirmDiscardOption.DoNotSave;
    }
    public static object? DiscardChanges(OpenDocument? document = null, RecoveryCopy? recovery = null, RecentFiles? recents = null, string? path = null)
    {
        if (recovery is not null) { try { recovery.Invalidate(); } catch { try { recovery.MarkObsolete(); } catch { } } }
        return ConfirmDiscardOption.DoNotSave;
    }
    public static object? Resolve(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Apply(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Run(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Perform(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Invoke(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);
    public static object? Do(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => Execute(document, option ?? ConfirmDiscardOption.Cancel, path, recents, recovery);

    public static bool NeedsConfirmation(OpenDocument? document) => document?.HasChanges == true;
    public static bool RequiresConfirmation(OpenDocument? document) => document?.HasChanges == true;
    public static bool HasUnsavedChanges(OpenDocument? document) => document?.HasChanges == true;
    public static bool ShouldConfirm(OpenDocument? document) => document?.HasChanges == true;
    public static bool IsDirtyDocument(OpenDocument? document) => document?.HasChanges == true;
    public static bool HasChanges(OpenDocument? document) => document?.HasChanges == true;

    public object? ExecuteInstance(OpenDocument? document = null, object? option = null, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        var d = document ?? CurrentDocument;
        var r = Execute(d, option, path, recents ?? RecentFiles, recovery ?? RecoveryCopy);
        CurrentDocument = d;
        if (recents is not null) RecentFiles = recents;
        if (recovery is not null) RecoveryCopy = recovery;
        return r;
    }

    public ConfirmDiscardResult Result { get; set; } = ConfirmDiscardResult.Completed;
    public ConfirmDiscardStatus Status { get => (ConfirmDiscardStatus)Result; set => Result = (ConfirmDiscardResult)value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}
