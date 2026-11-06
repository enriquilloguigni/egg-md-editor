using System;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    public bool ShowFormatDialog(string? formatKey = null) => true;
    public bool ApplyFormatDialog(string? formatKey = null) => true;
    public FormatOutcome ShowFormatDialogOutcome(string? formatKey = null) => new FormatOutcome { Success = true, Applied = true, FormatKey = formatKey };
    public FormatOutcome ApplyFormatDialogOutcome(string? formatKey = null) => new FormatOutcome { Success = true, Applied = true, FormatKey = formatKey };
}
