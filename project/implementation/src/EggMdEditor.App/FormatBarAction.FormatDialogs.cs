using System;

namespace EggMdEditor.App;

public partial class FormatBarAction
{
    public bool ShowFormatDialog() => ShowFormatDialog((string?)null);

    public bool ShowFormatDialog(string? formatKey)
    {
        if (_vm is not null) return _vm.ShowFormatDialog(formatKey);
        return true;
    }

    public bool ShowFormatDialog(object? formContent) => ShowFormatDialog(null, formContent);

    public bool ShowFormatDialog(string? formatKey, object? formContent)
    {
        if (_vm is not null) return _vm.ShowFormatDialog(formatKey, formContent);
        return true;
    }

    // swapped overload removed to avoid ambiguity

    public bool ApplyFormatDialog() => ApplyFormatDialog((string?)null, null);

    public bool ApplyFormatDialog(string? formatKey)
    {
        if (_vm is not null) return _vm.ApplyFormatDialog(formatKey);
        return true;
    }

    public bool ApplyFormatDialog(object? formContent) => ApplyFormatDialog(null, formContent);

    public bool ApplyFormatDialog(string? formatKey, object? formContent)
    {
        if (_vm is not null) return _vm.ApplyFormatDialog(formatKey, formContent);
        return true;
    }

    // swapped overload removed to avoid ambiguity

    public bool OpenFormatDialog() => ShowFormatDialog();
    public bool OpenFormatDialog(string? formatKey) => ShowFormatDialog(formatKey);
    public bool OpenFormatDialog(string? formatKey, object? formContent) => ShowFormatDialog(formatKey, formContent);

    public bool CloseFormatDialog() => _vm is not null ? _vm.CloseFormatDialog() : true;
    public bool CancelFormatDialog() => _vm is not null ? _vm.CancelFormatDialog() : true;
}
