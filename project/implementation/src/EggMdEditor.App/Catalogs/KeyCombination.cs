namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Keyboard modifiers of a gesture (COM-002), normalized to the Plan's
/// shortcut model: the primary modifier is <c>Ctrl</c> on Windows and Linux
/// and <c>Cmd</c> on macOS, so <c>Ctrl+Shift+L</c> and its macOS form
/// <c>Cmd+Shift+L</c> are the same logical combination of the dispatcher's
/// single register (same for <c>Ctrl+Q</c> and <c>Cmd+Q</c>). Mapping the
/// physical keyboard keys to these modifiers is the responsibility of the
/// window wiring (T-011).
/// </summary>
[Flags]
public enum ShortcutModifiers
{
    None = 0,

    /// <summary>Primary command modifier: <c>Ctrl</c> (Windows/Linux) or <c>Cmd</c> (macOS).</summary>
    Primary = 1,

    /// <summary>Modifier <c>Shift</c> (Mayús).</summary>
    Shift = 2,

    /// <summary>Modifier <c>Alt</c> (Option on macOS).</summary>
    Alt = 4,

    /// <summary>
    /// Mask of the model's modifiers; the remaining bits of a received
    /// gesture are discarded when the combination is normalized.
    /// </summary>
    All = Primary | Shift | Alt,
}

/// <summary>
/// Key combination of a keyboard gesture (COM-002): modifiers plus the main
/// key, admitted as a letter or digit — the combinations assigned in F-001
/// have that shape and the dispatcher's single register will be reviewed with
/// the same shape when the actions of F-028/F-029 are connected. The value is
/// normalized when built: the key is invariantly uppercase and the modifiers
/// are reduced to the model's ones. A combination absent from the
/// dispatcher's single register is an inert shortcut (BR-F001-003, ADR-001).
/// </summary>
public readonly record struct KeyCombination
{
    /// <summary>Normalized modifiers of the gesture.</summary>
    public ShortcutModifiers Modifiers { get; }

    /// <summary>Main key, invariantly uppercase.</summary>
    public char Key { get; }

    /// <summary>Creates the normalized combination from its parts.</summary>
    public KeyCombination(ShortcutModifiers modifiers, char key)
    {
        Modifiers = modifiers & ShortcutModifiers.All;
        Key = char.ToUpperInvariant(key);
    }

    /// <summary>Builds the normalized combination from a received gesture.</summary>
    public static KeyCombination From(ShortcutModifiers modifiers, char key) =>
        new(modifiers, key);

    /// <summary>
    /// Diagnostic representation in the Plan's format, with the primary
    /// modifier in its dual platform form.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>(4);
        if (Modifiers.HasFlag(ShortcutModifiers.Primary))
        {
            parts.Add("Ctrl/Cmd");
        }

        if (Modifiers.HasFlag(ShortcutModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(ShortcutModifiers.Alt))
        {
            parts.Add("Alt");
        }

        parts.Add(Key.ToString());
        return string.Join("+", parts);
    }
}
