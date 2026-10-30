using System;

namespace EggMdEditor.App;

public partial class EditorPanel
{
    private string? _lastFormatKey;
    private FormatEffect _lastFormatEffect = FormatEffect.Visual;
    private string? _lastInsertedSyntax;
    private string? _lastVisualEffect;
    private int _formatCount;
    private bool _lastFormatResult = true;
    private bool _visualEffectApplied;

    public string? LastFormatKey { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastFormatControl { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastFormat { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? FormatKey { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? ControlKey { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastControl { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastControlKey { get => _lastFormatKey; set => _lastFormatKey = value; }

    public FormatEffect LastFormatEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect FormatEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect LastEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect Effect { get => _lastFormatEffect; set => _lastFormatEffect = value; }

    public string? LastInsertedSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? InsertedSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? SyntaxInserted { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? LastSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? Syntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }

    public string? LastVisualEffect { get => _lastVisualEffect; set => _lastVisualEffect = value; }
    public string? VisualEffect { get => _lastVisualEffect; set => _lastVisualEffect = value; }
    public string? Visual { get => _lastVisualEffect; set => _lastVisualEffect = value; }

    public bool VisualEffectApplied { get => _visualEffectApplied; set => _visualEffectApplied = value; }
    public bool VisualApplied { get => _visualEffectApplied; set => _visualEffectApplied = value; }
    public bool EffectApplied { get => _visualEffectApplied; set => _visualEffectApplied = value; }

    public int FormatCount { get => _formatCount; set => _formatCount = value; }
    public int ApplyCount { get => _formatCount; set => _formatCount = value; }
    public int Total { get => _formatCount; set => _formatCount = value; }

    public bool LastFormatResult { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool LastResult { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool Result { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool Success { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool Ok { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool Applied { get => _lastFormatResult; set => _lastFormatResult = value; }

    public bool IsVisual => LastFormatEffect == FormatEffect.Visual;
    public bool IsSyntax => LastFormatEffect == FormatEffect.Syntax;
    public bool WasVisual => IsVisual;
    public bool WasSyntax => IsSyntax;
    public bool LastWasVisual => IsVisual;
    public bool LastWasSyntax => IsSyntax;

    public bool ApplyFormat(string? controlKey)
    {
        var f = new FormatBarAction(this);
        var r = f.ApplyFormat(controlKey);
        // sync state from f
        _lastFormatKey = f.LastKey;
        _lastFormatEffect = f.LastEffect;
        _lastInsertedSyntax = f.LastInsertedSyntax;
        _lastVisualEffect = f.LastVisualEffect;
        _visualEffectApplied = f.VisualEffectApplied;
        _formatCount = f.FormatCount;
        _lastFormatResult = f.LastResult;
        return r;
    }
    public bool Apply(string? controlKey) => ApplyFormat(controlKey);
    public bool Format(string? controlKey) => ApplyFormat(controlKey);
    public bool Invoke(string? controlKey) => ApplyFormat(controlKey);
    public bool InvokeFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Execute(string? controlKey) => ApplyFormat(controlKey);
    public bool ExecuteFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Run(string? controlKey) => ApplyFormat(controlKey);
    public bool RunFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Perform(string? controlKey) => ApplyFormat(controlKey);
    public bool PerformFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Do(string? controlKey) => ApplyFormat(controlKey);
    public bool DoFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Trigger(string? controlKey) => ApplyFormat(controlKey);
    public bool TriggerFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyControl(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatControl(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyFormatControl(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyKey(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyCommand(string? controlKey) => ApplyFormat(controlKey);
    public bool Handle(string? controlKey) => ApplyFormat(controlKey);
    public bool HandleFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Process(string? controlKey) => ApplyFormat(controlKey);
    public bool ProcessFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Act(string? controlKey) => ApplyFormat(controlKey);
    public bool ActOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatActive(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatPanel(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyToActive(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyToPanel(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyOnPanel(string? controlKey) => ApplyFormat(controlKey);

    public bool ApplyFormat(string? controlKey, EditorPanel? panel)
    {
        if (panel is not null) { /* apply to this panel regardless */ }
        return ApplyFormat(controlKey);
    }
}