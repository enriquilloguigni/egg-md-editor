using System;

namespace EggMdEditor.App;

public partial class MainWindowViewModel
{
    private string? _lastFormatDialogKey;

    private static string NormalizeFormatDialogKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return "link";
        if (k.Contains("link") || k.Contains("enlace") || k.Contains("url") || k.Contains("anchor") || k.Contains("ancla") || k.Contains("href") || k.Contains("hyperlink") || k.Contains("hipervinc")) return "link";
        if (k.Contains("table") || k.Contains("tabla")) return "table";
        if (k.Contains("image") || k.Contains("imagen")) return "image";
        if (k.Contains("formula") || k.Contains("fórmula") || k.Contains("latex") || k.Contains("math") || k.Contains("matematic")) return "formula";
        if (k.Contains("footnote") || k.Contains("nota") || k.Contains("pie")) return "footnote";
        if (k.Contains("code") || k.Contains("codigo") || k.Contains("código") || k.Contains("language") || k.Contains("lenguaje") || k.Contains("lang")) return "code";
        return k;
    }

    private static FormatDialogModel CreateFormatDialogModel(string key) => key switch
    {
        "table" => new TableFormatDialog(key),
        "image" => new ImageFormatDialog(key),
        "formula" => new FormulaFormatDialog(key),
        "footnote" => new FootnoteFormatDialog(key),
        "code" => new CodeFormatDialog(key),
        _ => new LinkFormatDialog(key),
    };

    private static string FormatDialogTitle(string key) => key switch
    {
        "link" => "Enlace",
        "table" => "Tabla",
        "image" => "Imagen",
        "formula" => "Fórmula",
        "footnote" => "Nota al pie",
        "code" => "Código",
        _ => key,
    };

    private static string FormatDialogMessage(string key) => key switch
    {
        "link" => "Introduce el texto y el destino del enlace.",
        "table" => "Configura las filas, columnas y alineación.",
        "image" => "Introduce el origen y el texto alternativo.",
        "formula" => "Introduce el código LaTeX.",
        "footnote" => "Introduce el identificador y el texto.",
        "code" => "Introduce el lenguaje del código.",
        _ => "Configura el formato.",
    };

    public bool ShowFormatDialog() => ShowFormatDialog((string?)null);

    public bool ShowFormatDialog(string? formatKey) => ShowFormatDialog(formatKey, null);

    public bool ShowFormatDialog(object? formContent) => ShowFormatDialog(null, formContent);

    public bool ShowFormatDialog(string? formatKey, object? formContent)
    {
        var key = NormalizeFormatDialogKey(formatKey);
        object content = formContent ?? CreateFormatDialogModel(key);
        if (content is FormatDialogModel fdm && string.IsNullOrWhiteSpace(fdm.FormatKey))
        {
            fdm.FormatKey = key;
        }
        if (content is FormatDialogModel fdm2)
        {
            fdm2.FormatKey = key;
        }
        _lastFormatDialogKey = key;
        return Dialog.Open(
            title: FormatDialogTitle(key),
            message: FormatDialogMessage(key),
            formContent: content,
            primaryText: null,
            secondaryText: null,
            hasUnsavedChanges: false);
    }

    // swapped overload removed to avoid ambiguity

    public bool ApplyFormatDialog() => ApplyFormatDialog((string?)null, null);

    public bool ApplyFormatDialog(string? formatKey) => ApplyFormatDialog(formatKey, null);

    public bool ApplyFormatDialog(object? formContent) => ApplyFormatDialog(null, formContent);

    public bool ApplyFormatDialog(string? formatKey, object? formContent)
    {
        var form = formContent as FormatDialogModel ?? Dialog.FormContent as FormatDialogModel;
        var key = NormalizeFormatDialogKey(formatKey ?? form?.FormatKey ?? _lastFormatDialogKey);

        if (form is not null)
        {
            var msg = form.ValidateFor(key);
            if (!string.IsNullOrEmpty(msg))
            {
                Dialog.SetValidationMessage(msg);
                return false;
            }
        }

        _lastFormatDialogKey = key;

        try
        {
            ApplyFormat(key);
        }
        catch
        {
        }

        try
        {
            Dialog.InvokePrimary();
        }
        catch
        {
            Dialog.Close(DialogCloseReason.Primary);
        }

        return true;
    }

    // swapped overload removed to avoid ambiguity

    // outcome-returning helpers
    public FormatOutcome ApplyFormatDialogOutcome() => ApplyFormatDialogOutcome(null, null);

    public FormatOutcome ApplyFormatDialogOutcome(string? formatKey, object? formContent)
    {
        var ok = ApplyFormatDialog(formatKey, formContent);
        return new FormatOutcome { Success = ok, Applied = ok, FormatKey = NormalizeFormatDialogKey(formatKey ?? _lastFormatDialogKey) };
    }

    // aliases
    public bool ShowDialogFormat() => ShowFormatDialog();
    public bool ShowDialogFormat(string? formatKey) => ShowFormatDialog(formatKey);
    public bool OpenFormatDialog() => ShowFormatDialog();
    public bool OpenFormatDialog(string? formatKey) => ShowFormatDialog(formatKey);
    public bool OpenFormatDialog(string? formatKey, object? formContent) => ShowFormatDialog(formatKey, formContent);

    public bool ApplyDialogFormat() => ApplyFormatDialog();
    public bool ApplyDialogFormat(string? formatKey) => ApplyFormatDialog(formatKey);

    public bool CloseFormatDialog() => Dialog.Close(DialogCloseReason.Secondary);
    public bool CancelFormatDialog() => Dialog.RequestClose();

    public string? LastFormatDialogKey { get => _lastFormatDialogKey; set => _lastFormatDialogKey = value; }
    public string? FormatDialogKey { get => _lastFormatDialogKey; set => _lastFormatDialogKey = value; }
    public string? CurrentFormatDialogKey { get => _lastFormatDialogKey; set => _lastFormatDialogKey = value; }
}
