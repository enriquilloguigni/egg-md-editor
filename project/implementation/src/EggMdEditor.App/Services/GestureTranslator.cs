using System.Runtime.InteropServices;
using Avalonia.Input;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Translation of physical keyboard gestures to the shortcut model (COM-002,
/// T-011): converts the key and modifiers reported by SC-001's keyboard input
/// into the dispatcher's single-register <see cref="KeyCombination"/> —its
/// translation is the window wiring's responsibility (COM-002)— or into
/// nothing if the gesture is not expressible in the model.
///
/// <para>
/// The primary modifier is <c>Ctrl</c> on Windows and Linux and <c>Cmd</c> (⌘)
/// on macOS, per the Plan's model: on Windows and Linux it translates
/// <c>Control</c>, and on macOS <c>Meta</c>; the other one is not primary and
/// is discarded —for example, a physical <c>Ctrl+Q</c> on macOS is not the
/// assigned close shortcut, which is <c>Cmd+Q</c>—. <c>Shift</c> translates to
/// <see cref="ShortcutModifiers.Shift"/> and <c>Alt</c> to
/// <see cref="ShortcutModifiers.Alt"/>; any other modifier does not belong to
/// the model and is discarded. The main key is a letter (A–Z) or a digit
/// (digits of the main block or of the numeric keypad) — the shape of the
/// combinations assigned in F-001 and of the ones F-028/F-029 will connect,
/// reviewed over the same single register—; the rest of the keys is not
/// expressible and yields a gesture without translation, which the wiring
/// treats as an inert shortcut (BR-F001-003, ADR-001).
/// </para>
/// </summary>
public static class GestureTranslator
{
    /// <summary>
    /// Translates a keyboard gesture received from the window, according to
    /// the platform the process runs on (macOS vs. Windows/Linux).
    /// </summary>
    public static KeyCombination? Translate(Key key, KeyModifiers modifiers) =>
        Translate(key, modifiers, IsMacOS());

    /// <summary>
    /// Translates a gesture with the given platform, for deterministic wiring
    /// in tests: <c>isMacOS</c> chooses which physical modifier is the
    /// model's primary one (<c>Meta</c> on macOS, <c>Control</c> on Windows
    /// and Linux).
    /// </summary>
    public static KeyCombination? Translate(Key key, KeyModifiers modifiers, bool isMacOS)
    {
        var character = CharacterOf(key);

        if (character is null)
        {
            return null;
        }

        var modelModifiers = ShortcutModifiers.None;

        var physicalPrimary = isMacOS ? KeyModifiers.Meta : KeyModifiers.Control;
        if (modifiers.HasFlag(physicalPrimary))
        {
            modelModifiers |= ShortcutModifiers.Primary;
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            modelModifiers |= ShortcutModifiers.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            modelModifiers |= ShortcutModifiers.Alt;
        }

        return new KeyCombination(modelModifiers, character.Value);
    }

    /// <summary>Does the process run on macOS? The model's physical primary depends on it.</summary>
    private static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Character of the main key if it is a letter (A–Z), a digit of the main
    /// block (D0–D9) or a digit of the numeric keypad (NumPad0–NumPad9),
    /// invariantly uppercase; null for any other key, which is not expressible
    /// as a model combination.
    /// </summary>
    private static char? CharacterOf(Key key)
    {
        if (key is >= Key.A and <= Key.Z)
        {
            return (char)('A' + key - Key.A);
        }

        if (key is >= Key.D0 and <= Key.D9)
        {
            return (char)('0' + key - Key.D0);
        }

        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            return (char)('0' + key - Key.NumPad0);
        }

        return null;
    }
}
