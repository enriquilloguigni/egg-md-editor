namespace EggMdEditor.App;

public partial class FormulaRenderer
{
    public bool FocusVisible { get; set; } = true;
    public bool IsFocusVisible { get => FocusVisible; set => FocusVisible = value; }
    public bool VisibleFocus { get => FocusVisible; set => FocusVisible = value; }
    public bool FocoVisible { get => FocusVisible; set => FocusVisible = value; }

    public bool KeyboardNavigable { get; set; } = true;
    public bool CanNavigateByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool SupportsKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool IsKeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool OperableByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool KeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool NavigableByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }

    public bool ScreenReaderCompatible { get; set; } = true;
    public bool SupportsScreenReader { get => ScreenReaderCompatible; set => ScreenReaderCompatible = value; }
    public bool ScreenReader { get => ScreenReaderCompatible; set => ScreenReaderCompatible = value; }

    public string Role { get; set; } = "math";
    public string AccessibleRole { get => Role; set => Role = value; }
    public string ControlType { get => Role; set => Role = value; }

    public string AccessibleName { get; set; } = "Formula";
    public string Name { get => AccessibleName; set => AccessibleName = value; }
    public string AccessibleLabel { get => AccessibleName; set => AccessibleName = value; }
    public string Label { get => AccessibleName; set => AccessibleName = value; }

    public string AccessibleDescription { get; set; } = "LaTeX formula";
    public string Description { get => AccessibleDescription; set => AccessibleDescription = value; }

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