namespace EggMdEditor.App;

public sealed partial class DialogViewModel
{
    private bool _focusVisibleDialog = true;
    private bool _keyboardNavigableDialog = true;
    private bool _screenReaderCompatibleDialog = true;
    private string _roleDialog = "dialog";
    private string _accessibleNameDialog = "Dialog";
    private string _accessibleDescriptionDialog = "Generic dialog surface";
    private string _automationIdDialog = "Dialog";
    private object? _automationPeerDialog;

    public bool FocusVisible { get => _focusVisibleDialog; set => _focusVisibleDialog = value; }
    public bool IsFocusVisible { get => _focusVisibleDialog; set => _focusVisibleDialog = value; }
    public bool VisibleFocus { get => _focusVisibleDialog; set => _focusVisibleDialog = value; }
    public bool FocoVisible { get => _focusVisibleDialog; set => _focusVisibleDialog = value; }

    public bool KeyboardNavigable { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool CanNavigateByKeyboard { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool SupportsKeyboard { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool IsKeyboardOperable { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool OperableByKeyboard { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool KeyboardOperable { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }
    public bool NavigableByKeyboard { get => _keyboardNavigableDialog; set => _keyboardNavigableDialog = value; }

    public bool ScreenReaderCompatible { get => _screenReaderCompatibleDialog; set => _screenReaderCompatibleDialog = value; }
    public bool SupportsScreenReader { get => _screenReaderCompatibleDialog; set => _screenReaderCompatibleDialog = value; }
    public bool ScreenReader { get => _screenReaderCompatibleDialog; set => _screenReaderCompatibleDialog = value; }

    public string Role { get => _roleDialog; set => _roleDialog = value ?? _roleDialog; }
    public string AccessibleRole { get => _roleDialog; set => _roleDialog = value ?? _roleDialog; }
    public string ControlType { get => _roleDialog; set => _roleDialog = value ?? _roleDialog; }

    public string AccessibleName { get => _accessibleNameDialog; set => _accessibleNameDialog = value ?? _accessibleNameDialog; }
    public string Name { get => _accessibleNameDialog; set => _accessibleNameDialog = value ?? _accessibleNameDialog; }
    public string AccessibleLabel { get => _accessibleNameDialog; set => _accessibleNameDialog = value ?? _accessibleNameDialog; }
    public string Label { get => _accessibleNameDialog; set => _accessibleNameDialog = value ?? _accessibleNameDialog; }

    public string AccessibleDescription { get => _accessibleDescriptionDialog; set => _accessibleDescriptionDialog = value ?? _accessibleDescriptionDialog; }
    public string Description { get => _accessibleDescriptionDialog; set => _accessibleDescriptionDialog = value ?? _accessibleDescriptionDialog; }

    public string AutomationId { get => _automationIdDialog; set => _automationIdDialog = value ?? _automationIdDialog; }
    public string AutomationID { get => _automationIdDialog; set => _automationIdDialog = value ?? _automationIdDialog; }
    public string Id { get => _automationIdDialog; set => _automationIdDialog = value ?? _automationIdDialog; }

    public object? AutomationPeer { get => _automationPeerDialog; set => _automationPeerDialog = value; }
    public object? Peer { get => _automationPeerDialog; set => _automationPeerDialog = value; }
    public object? GetAutomationPeer() => _automationPeerDialog;
    public bool HasAutomationPeer => _automationPeerDialog is not null;

    public string LiveRegion { get; set; } = "polite";
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

    public bool IsModal { get; set; } = true;
    public bool Modal { get => IsModal; set => IsModal = value; }
    public bool IsDialog { get => true; }

    public static bool IsKeyboardNavigable(object? target = null) => true;
    public static bool HasFocusVisible(object? target = null) => true;
    public static bool IsScreenReaderCompatible(object? target = null) => true;
}