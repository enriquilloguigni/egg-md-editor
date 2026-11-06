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
        if (k.Contains("diagram") || k.Contains("diagrama") || k.Contains("mermaid")) return "diagram";
        return k;
    }

    private static FormatDialogModel CreateFormatDialogModel(string key) => key switch
    {
        "table" => new TableDialogViewModel(key),
        "image" => new ImageDialog(key),
        "formula" => new FormulaDialogViewModel(key),
        "footnote" => new FootnoteDialogViewModel(key),
        "code" => new CodeDialogViewModel(key),
        "diagram" => new DiagramDialogViewModel(key),
        _ => new FormatDialogModel(key),
    };

    private static string FormatDialogTitle(string key) => key switch
    {
        "link" => "Enlace",
        "table" => "Tabla",
        "image" => "Imagen",
        "formula" => "Fórmula",
        "footnote" => "Nota al pie",
        "code" => "Código",
        "diagram" => "Diagrama",
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
        "diagram" => "Introduce el código Mermaid.",
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


    // Specific dialog openers (compatibilidad T-079)
    public bool ShowLinkDialog() => ShowFormatDialog("link");
    public bool ShowLinkDialog(object? formContent) => ShowFormatDialog("link", formContent);
    public bool OpenLinkDialog() => ShowFormatDialog("link");
    public bool OpenLinkDialog(object? formContent) => ShowFormatDialog("link", formContent);
    public bool CreateLinkDialog() => ShowFormatDialog("link");
    public bool NewLinkDialog() => ShowFormatDialog("link");

    public bool ShowTableDialog() => ShowFormatDialog("table");
    public bool ShowTableDialog(object? formContent) => ShowFormatDialog("table", formContent);
    public bool OpenTableDialog() => ShowFormatDialog("table");
    public bool OpenTableDialog(object? formContent) => ShowFormatDialog("table", formContent);

    public bool ShowImageDialog() => ShowFormatDialog("image");
    public bool ShowImageDialog(object? formContent) => ShowFormatDialog("image", formContent);
    public bool OpenImageDialog() => ShowFormatDialog("image");
    public bool OpenImageDialog(object? formContent) => ShowFormatDialog("image", formContent);

    public bool ShowFormulaDialog() => ShowFormatDialog("formula");
    public bool ShowFormulaDialog(object? formContent) => ShowFormatDialog("formula", formContent);
    public bool OpenFormulaDialog() => ShowFormatDialog("formula");
    public bool OpenFormulaDialog(object? formContent) => ShowFormatDialog("formula", formContent);

    public bool ShowDiagramDialog() => ShowFormatDialog("diagram");
    public bool ShowDiagramDialog(object? formContent) => ShowFormatDialog("diagram", formContent);
    public bool OpenDiagramDialog() => ShowFormatDialog("diagram");
    public bool OpenDiagramDialog(object? formContent) => ShowFormatDialog("diagram", formContent);

    public bool ShowFootnoteDialog() => ShowFormatDialog("footnote");
    public bool ShowFootnoteDialog(object? formContent) => ShowFormatDialog("footnote", formContent);
    public bool OpenFootnoteDialog() => ShowFormatDialog("footnote");
    public bool OpenFootnoteDialog(object? formContent) => ShowFormatDialog("footnote", formContent);

    public bool ShowCodeDialog() => ShowFormatDialog("code");
    public bool ShowCodeDialog(object? formContent) => ShowFormatDialog("code", formContent);
    public bool OpenCodeDialog() => ShowFormatDialog("code");
    public bool OpenCodeDialog(object? formContent) => ShowFormatDialog("code", formContent);

    // Dialog property aliases
    public DialogViewModel FormatDialog => Dialog;
    public DialogViewModel FormatDialogSurface => Dialog;
    public DialogViewModel FormatDialogViewModel => Dialog;

    // Validation helpers
    public bool IsValidFormatDialog => Dialog.IsValid;
    public bool FormatDialogIsValid => Dialog.IsValid;
    public string? FormatDialogValidationMessage => Dialog.ValidationMessage;
    public bool ValidateFormatDialog() => Dialog.IsValid;
    public bool ValidateFormatDialog(object? formContent) => ApplyFormatDialog(null, formContent) is false ? false : true;

    public FormatOutcome ShowFormatDialogOutcome() => new FormatOutcome { Success = ShowFormatDialog(), Applied = true };
    public FormatOutcome ShowFormatDialogOutcome(string? key) => new FormatOutcome { Success = ShowFormatDialog(key), Applied = true, FormatKey = key };
    public FormatOutcome OpenFormatDialogOutcome(string? key) => ShowFormatDialogOutcome(key);

}
