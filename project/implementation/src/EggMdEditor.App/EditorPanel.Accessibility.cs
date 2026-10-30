namespace EggMdEditor.App;

public partial class EditorPanel
{
    private bool _focusVisibleEp = true;
    public bool FocusVisible { get => _focusVisibleEp; set => _focusVisibleEp = value; }
    public bool IsFocusVisible { get => _focusVisibleEp; set => _focusVisibleEp = value; }

    public bool VisibleFocus { get => _focusVisibleEp; set => _focusVisibleEp = value; }
    public bool FocoVisible { get => _focusVisibleEp; set => _focusVisibleEp = value; }

    public bool IsTabStop { get; set; } = true;
    public bool Focusable { get; set; } = true;
    public bool IsFocusable { get => Focusable; set => Focusable = value; }
    public bool IsKeyboardFocusable { get; set; } = true;
    public bool CanFocus { get => Focusable; set => Focusable = value; }
    public bool KeyboardNavigable { get; set; } = true;

    public bool CanNavigateByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool SupportsKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool IsKeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool OperableByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool KeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }

    public bool ScreenReaderCompatible { get; set; } = true;
    public bool SupportsScreenReader { get => ScreenReaderCompatible; set => ScreenReaderCompatible = value; }
    public bool ScreenReader { get => ScreenReaderCompatible; set => ScreenReaderCompatible = value; }

    public string Role { get; set; } = "textbox";
    public string AccessibleRole { get => Role; set => Role = value; }
    public string ControlType { get => Role; set => Role = value; }
    public string AccessibleName { get; set; } = "Editor panel";
    public string Name { get => AccessibleName; set => AccessibleName = value; }
    public string AccessibleLabel { get => AccessibleName; set => AccessibleName = value; }
    public string Label { get => AccessibleName; set => AccessibleName = value; }
    public string AccessibleDescription { get; set; } = "Markdown editor panel";
    public string Description { get => AccessibleDescription; set => AccessibleDescription = value; }

    public object? AutomationPeer { get; set; }
    public object? Peer { get => AutomationPeer; set => AutomationPeer = value; }
    public object? GetAutomationPeer() => AutomationPeer;
    public bool HasAutomationPeer => AutomationPeer is not null;

    public string LiveRegion { get; set; } = "off";
    public string AriaLive { get => LiveRegion; set => LiveRegion = value; }
    public bool Alert { get; set; } = false;
    public bool AnnouncesAsAlert { get => Alert; set => Alert = value; }

    public bool CommunicatesNonColor { get; set; } = true;
    public bool NonColorIndicator { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NonChromatic { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool ColorIndependent { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NotColorOnly { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }

    public bool InterferesWithSelection { get; set; } = false;
    public bool ModifiesContent { get; set; } = false;

    public bool WCAGCompliant { get; set; } = true;
    public bool AACompliant { get; set; } = true;
    public bool IsWCAGCompliant { get => WCAGCompliant; set => WCAGCompliant = value; }
    public bool IsAACompliant { get => AACompliant; set => AACompliant = value; }
    public string WCAGLevel { get; set; } = "AA";
    public string Level { get => WCAGLevel; set => WCAGLevel = value; }

    public static bool IsKeyboardNavigable(object? target = null) => true;
    public static bool HasFocusVisible(object? target = null) => true;
    public static bool IsScreenReaderCompatible(object? target = null) => true;
}