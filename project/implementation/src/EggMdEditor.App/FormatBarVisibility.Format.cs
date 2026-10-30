using System;

namespace EggMdEditor.App;

public partial class FormatBarVisibility
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

    public FormatEffect LastFormatEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect FormatEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect LastEffect { get => _lastFormatEffect; set => _lastFormatEffect = value; }
    public FormatEffect Effect { get => _lastFormatEffect; set => _lastFormatEffect = value; }

    public string? LastInsertedSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? InsertedSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? SyntaxInserted { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }
    public string? LastSyntax { get => _lastInsertedSyntax; set => _lastInsertedSyntax = value; }

    public string? LastVisualEffect { get => _lastVisualEffect; set => _lastVisualEffect = value; }
    public string? VisualEffect { get => _lastVisualEffect; set => _lastVisualEffect = value; }

    public bool VisualEffectApplied { get => _visualEffectApplied; set => _visualEffectApplied = value; }
    public int FormatCount { get => _formatCount; set => _formatCount = value; }
    public bool LastFormatResult { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool LastResult { get => _lastFormatResult; set => _lastFormatResult = value; }
    public bool Result { get => _lastFormatResult; set => _lastFormatResult = value; }

    public bool IsVisual => LastFormatEffect == FormatEffect.Visual;
    public bool IsSyntax => LastFormatEffect == FormatEffect.Syntax;
    public bool WasVisual => IsVisual;
    public bool WasSyntax => IsSyntax;

    public bool ApplyFormat(string? controlKey)
    {
        var panel = ActivePanel;
        var f = new FormatBarAction(panel);
        var r = f.ApplyFormat(controlKey);
        _lastFormatKey = f.LastKey;
        _lastFormatEffect = f.LastEffect;
        _lastInsertedSyntax = f.LastInsertedSyntax;
        _lastVisualEffect = f.LastVisualEffect;
        _visualEffectApplied = f.VisualEffectApplied;
        _formatCount = f.FormatCount;
        _lastFormatResult = f.LastResult;
        // sync active panel
        if (panel is not null)
        {
            panel.LastFormatKey = _lastFormatKey;
            panel.LastFormatEffect = _lastFormatEffect;
            panel.LastInsertedSyntax = _lastInsertedSyntax;
            panel.LastVisualEffect = _lastVisualEffect;
            panel.VisualEffectApplied = _visualEffectApplied;
            panel.FormatCount = _formatCount;
            panel.LastFormatResult = _lastFormatResult;
        }
        // sync vm
        if (_vm is not null)
        {
            _vm.LastFormatKey = _lastFormatKey;
            _vm.LastFormatEffect = _lastFormatEffect;
            _vm.LastInsertedSyntax = _lastInsertedSyntax;
            _vm.LastVisualEffect = _lastVisualEffect;
            _vm.VisualEffectApplied = _visualEffectApplied;
            _vm.FormatCount = _formatCount;
            _vm.LastFormatResult = _lastFormatResult;
        }
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
        ActivePanel = panel;
        return ApplyFormat(controlKey);
    }
    public bool ApplyFormat(string? controlKey, PanelSide side)
    {
        SetActivePanel(side);
        return ApplyFormat(controlKey);
    }
    public bool ApplyFormat(string? controlKey, string? side)
    {
        SetActivePanel(side);
        return ApplyFormat(controlKey);
    }
    public bool Apply(string? controlKey, EditorPanel? panel) => ApplyFormat(controlKey, panel);
    public bool Apply(string? controlKey, PanelSide side) => ApplyFormat(controlKey, side);
    public bool Apply(string? controlKey, string? side) => ApplyFormat(controlKey, side);
    public bool Format(string? controlKey, EditorPanel? panel) => ApplyFormat(controlKey, panel);
    public bool Format(string? controlKey, PanelSide side) => ApplyFormat(controlKey, side);
    public bool Format(string? controlKey, string? side) => ApplyFormat(controlKey, side);

    public bool ApplyFormat() => ApplyFormat(LastFormatKey);
    public bool Apply() => ApplyFormat(LastFormatKey);
    public bool Format() => ApplyFormat(LastFormatKey);
}