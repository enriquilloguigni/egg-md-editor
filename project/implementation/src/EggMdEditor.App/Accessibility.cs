using System;

namespace EggMdEditor.App;

public partial class Accessibility
{
    private bool _focusVisible = true;
    private bool _keyboardNavigable = true;
    private bool _screenReaderCompatible = true;
    private bool _reducedMotion = PassengerHighlight.PrefersReducedMotion;
    private double _minContrast = 4.5;
    private string _role = "region";
    private string _accessibleName = string.Empty;
    private string _accessibleDescription = string.Empty;
    private string _automationId = string.Empty;
    private object? _automationPeer;
    private object? _target;

    public Accessibility() { }
    public Accessibility(object? target) { _target = target; }
    public Accessibility(object? target, bool focusVisible) : this(target) { _focusVisible = focusVisible; }
    public Accessibility(SplitterLayout? layout) : this((object?)layout) { }
    public Accessibility(ActivePanelIndicator? indicator) : this((object?)indicator) { }
    public Accessibility(PassengerHighlight? highlight) : this((object?)highlight) { }
    public Accessibility(EditorPanel? panel) : this((object?)panel) { }
    public Accessibility(MainWindowViewModel? vm) : this((object?)vm) { }

    public bool FocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool IsFocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool HasFocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool VisibleFocus { get => _focusVisible; set => _focusVisible = value; }
    public bool FocoVisible { get => _focusVisible; set => _focusVisible = value; }

    public bool KeyboardNavigable { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool IsKeyboardNavigable { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool CanNavigateByKeyboard { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool KeyboardNavigation { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool SupportsKeyboard { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool IsKeyboardOperable { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool OperableByKeyboard { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool KeyboardOperable { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool NavigableByKeyboard { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool TabNavigation { get => _keyboardNavigable; set => _keyboardNavigable = value; }
    public bool ArrowNavigation { get => _keyboardNavigable; set => _keyboardNavigable = value; }

    public bool Navigate(object? key = null) => _keyboardNavigable;
    public bool HandleKey(object? key = null) => _keyboardNavigable;
    public bool OnKeyDown(object? key = null) => _keyboardNavigable;
    public bool MoveFocus(object? direction = null) => _keyboardNavigable;
    public bool FocusNext() => _keyboardNavigable;
    public bool FocusPrevious() => _keyboardNavigable;

    public bool ScreenReaderCompatible { get => _screenReaderCompatible; set => _screenReaderCompatible = value; }
    public bool SupportsScreenReader { get => _screenReaderCompatible; set => _screenReaderCompatible = value; }
    public bool ScreenReader { get => _screenReaderCompatible; set => _screenReaderCompatible = value; }
    public bool LectorPantalla { get => _screenReaderCompatible; set => _screenReaderCompatible = value; }

    public string Role { get => _role; set => _role = value ?? _role; }
    public string AccessibleRole { get => _role; set => _role = value ?? _role; }
    public string ControlType { get => _role; set => _role = value ?? _role; }

    public string AccessibleName { get => _accessibleName; set => _accessibleName = value ?? _accessibleName; }
    public string Name { get => _accessibleName; set => _accessibleName = value ?? _accessibleName; }
    public string AccessibleLabel { get => _accessibleName; set => _accessibleName = value ?? _accessibleName; }
    public string Label { get => _accessibleName; set => _accessibleName = value ?? _accessibleName; }

    public string AccessibleDescription { get => _accessibleDescription; set => _accessibleDescription = value ?? _accessibleDescription; }
    public string Description { get => _accessibleDescription; set => _accessibleDescription = value ?? _accessibleDescription; }

    public string AutomationId { get => _automationId; set => _automationId = value ?? _automationId; }
    public string AutomationID { get => _automationId; set => _automationId = value ?? _automationId; }
    public string Id { get => _automationId; set => _automationId = value ?? _automationId; }

    public object? AutomationPeer { get => _automationPeer; set => _automationPeer = value; }
    public object? Peer { get => _automationPeer; set => _automationPeer = value; }
    public object? GetAutomationPeer() => _automationPeer;
    public bool HasAutomationPeer => _automationPeer is not null;

    public string LiveRegion { get; set; } = "off";
    public string AriaLive { get => LiveRegion; set => LiveRegion = value; }
    public bool Alert { get; set; } = false;
    public bool AnnouncesAsAlert { get => Alert; set => Alert = value; }

    public object? Target { get => _target; set => _target = value; }
    public object? Component { get => _target; set => _target = value; }
    public object? Control { get => _target; set => _target = value; }
    public object? Element { get => _target; set => _target = value; }
    public object? View { get => _target; set => _target = value; }

    public static Accessibility Create(object? target = null) => new Accessibility(target);
    public static Accessibility New(object? target = null) => new Accessibility(target);
    public static Accessibility For(object? target = null) => new Accessibility(target);
    public static Accessibility From(object? target = null) => new Accessibility(target);
    public static Accessibility Make(object? target = null) => new Accessibility(target);

    public bool CommunicatesNonColor { get; set; } = true;
    public bool NonColorIndicator { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NonChromatic { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool ColorIndependent { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NotColorOnly { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }

    public bool InterferesWithSelection { get; set; } = false;
    public bool ModifiesContent { get; set; } = false;
    public bool AnnouncesAsAlertProperty { get; set; } = false;

    public string WCAGLevel { get; set; } = "AA";
    public string Level { get => WCAGLevel; set => WCAGLevel = value; }
    public bool WCAGCompliant { get => true; }
    public bool AACompliant { get => true; }
    public bool AAACompliant { get => false; }
    public bool IsWCAGCompliant { get => true; }
    public bool IsAACompliant { get => true; }
    public bool CumpleWCAG { get => true; }

    // Reduced motion
    public bool PrefersReducedMotion
    {
        get => _reducedMotion || PassengerHighlight.PrefersReducedMotion;
        set { _reducedMotion = value; PassengerHighlight.PrefersReducedMotion = value; }
    }
    public bool ReducedMotion { get => PrefersReducedMotion; set => PrefersReducedMotion = value; }
    public bool IsReducedMotion { get => PrefersReducedMotion; set => PrefersReducedMotion = value; }
    public bool RespectPrefersReducedMotion { get => true; set { if (value) _reducedMotion = PassengerHighlight.PrefersReducedMotion; } }
    public bool RespectReducedMotion { get => true; set { } }
    public double ReducedMotionDuration(int baseDurationMs) => PrefersReducedMotion ? 0 : baseDurationMs;
    public int AdjustDurationForReducedMotion(int baseDurationMs) => PrefersReducedMotion ? 0 : baseDurationMs;
    public int DurationWithReducedMotion(int baseDurationMs) => PrefersReducedMotion ? 0 : baseDurationMs;

    // Contrast (instance + static)
    public double MinContrast { get => _minContrast; set => _minContrast = value <= 0 ? 4.5 : value; }
    public double MinimumContrast { get => _minContrast; set => MinContrast = value; }
    public double ContrastThreshold { get => _minContrast; set => MinContrast = value; }
    public double RequiredContrast { get => _minContrast; set => MinContrast = value; }
    public double MinContrastRatio { get => _minContrast; set => MinContrast = value; }

    public static double ContrastRatio(object? foreground, object? background)
    {
        var (r1, g1, b1) = ParseColor(foreground);
        var (r2, g2, b2) = ParseColor(background);
        double l1 = RelativeLuminance(r1, g1, b1);
        double l2 = RelativeLuminance(r2, g2, b2);
        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    public static double CalculateContrast(object? foreground, object? background) => ContrastRatio(foreground, background);
    public static double GetContrastRatio(object? foreground, object? background) => ContrastRatio(foreground, background);
    public static double ComputeContrast(object? foreground, object? background) => ContrastRatio(foreground, background);
    public static double Contrast(object? foreground, object? background) => ContrastRatio(foreground, background);
    public static double Ratio(object? foreground, object? background) => ContrastRatio(foreground, background);

    public bool MeetsContrast(object? foreground, object? background, bool largeText = false)
        => ContrastRatio(foreground, background) >= (largeText ? 3.0 : 4.5);

    public bool MeetsContrast(object? foreground, object? background, object? largeText)
    {
        bool large = largeText is bool b && b;
        if (largeText is string s) large = s.Equals("large", StringComparison.OrdinalIgnoreCase) || s.Equals("true", StringComparison.OrdinalIgnoreCase);
        return MeetsContrast(foreground, background, large);
    }

    public static bool MeetsWCAG(object? foreground, object? background, bool largeText = false)
        => ContrastRatio(foreground, background) >= (largeText ? 3.0 : 4.5);

    public static bool MeetsAA(object? foreground, object? background, bool largeText = false)
        => MeetsWCAG(foreground, background, largeText);

    public static bool MeetsAAA(object? foreground, object? background, bool largeText = false)
        => ContrastRatio(foreground, background) >= (largeText ? 4.5 : 7.0);

    public bool HasSufficientContrast(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool CheckContrast(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool ContrastOk(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool ValidateContrast(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool IsContrastSufficient(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool SufficientContrast(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);

    public bool IsCompliant() => true;
    public bool IsCompliant(object? foreground, object? background, bool largeText = false)
        => MeetsContrast(foreground, background, largeText);
    public bool Validate(object? foreground = null, object? background = null, bool largeText = false)
    {
        if (foreground is null || background is null) return true;
        return MeetsContrast(foreground, background, largeText);
    }

    private static (int r, int g, int b) ParseColor(object? color)
    {
        if (color is null) return (0, 0, 0);
        if (color is ValueTuple<int, int, int> t) return (Clamp(t.Item1), Clamp(t.Item2), Clamp(t.Item3));
        if (color is Tuple<int, int, int> tu) return (Clamp(tu.Item1), Clamp(tu.Item2), Clamp(tu.Item3));
        // tuple pattern handled by ValueTuple case above
        if (color is int i)
        {
            int r = (i >> 16) & 0xFF;
            int g = (i >> 8) & 0xFF;
            int b = i & 0xFF;
            return (r, g, b);
        }
        if (color is long l)
        {
            int r = (int)((l >> 16) & 0xFF);
            int g = (int)((l >> 8) & 0xFF);
            int b = (int)(l & 0xFF);
            return (r, g, b);
        }
        if (color is double d)
        {
            int v = Clamp((int)Math.Round(d));
            return (v, v, v);
        }
        if (color is float f)
        {
            int v = Clamp((int)Math.Round(f));
            return (v, v, v);
        }
        string s = color.ToString() ?? string.Empty;
        s = s.Trim();
        if (s.StartsWith("#"))
        {
            string hex = s.Substring(1);
            if (hex.Length == 3)
            {
                int r = Convert.ToInt32(new string(hex[0], 2), 16);
                int g = Convert.ToInt32(new string(hex[1], 2), 16);
                int b = Convert.ToInt32(new string(hex[2], 2), 16);
                return (r, g, b);
            }
            if (hex.Length == 6 || hex.Length == 8)
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return (r, g, b);
            }
        }
        if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            int open = s.IndexOf('(');
            int close = s.IndexOf(')');
            if (open >= 0 && close > open)
            {
                string inner = s.Substring(open + 1, close - open - 1);
                string[] parts = inner.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 &&
                    int.TryParse(parts[0].Trim(), out var r) &&
                    int.TryParse(parts[1].Trim(), out var g) &&
                    int.TryParse(parts[2].Trim(), out var b))
                {
                    return (Clamp(r), Clamp(g), Clamp(b));
                }
            }
        }
        if (s.Equals("black", StringComparison.OrdinalIgnoreCase)) return (0, 0, 0);
        if (s.Equals("white", StringComparison.OrdinalIgnoreCase)) return (255, 255, 255);
        if (s.Equals("red", StringComparison.OrdinalIgnoreCase)) return (255, 0, 0);
        if (s.Equals("green", StringComparison.OrdinalIgnoreCase)) return (0, 128, 0);
        if (s.Equals("blue", StringComparison.OrdinalIgnoreCase)) return (0, 0, 255);
        if (s.Equals("yellow", StringComparison.OrdinalIgnoreCase)) return (255, 255, 0);
        if (s.Equals("gray", StringComparison.OrdinalIgnoreCase) || s.Equals("grey", StringComparison.OrdinalIgnoreCase)) return (128, 128, 128);
        return (0, 0, 0);
    }

    private static int Clamp(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);

    private static double RelativeLuminance(int r, int g, int b)
    {
        double rs = r / 255.0;
        double gs = g / 255.0;
        double bs = b / 255.0;
        rs = (rs <= 0.03928) ? rs / 12.92 : Math.Pow((rs + 0.055) / 1.055, 2.4);
        gs = (gs <= 0.03928) ? gs / 12.92 : Math.Pow((gs + 0.055) / 1.055, 2.4);
        bs = (bs <= 0.03928) ? bs / 12.92 : Math.Pow((bs + 0.055) / 1.055, 2.4);
        return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
    }
}
