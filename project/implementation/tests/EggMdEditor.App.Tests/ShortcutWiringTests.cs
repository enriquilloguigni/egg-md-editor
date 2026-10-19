using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Wiring of COM-002 in SC-001 (T-011): the window's keyboard translates each
// gesture to the combination model and dispatches it (T-005). The primary
// modifier is Ctrl on Windows/Linux and Cmd on macOS —the same logical
// combination of the single register—, the rest of shortcuts stays inert
// (BR-F001-003, ADR-001) and the dispatched gesture stops propagating.
public class ShortcutWiringTests
{
    private static readonly KeyCombination ToggleLanguage =
        new(ShortcutModifiers.Primary | ShortcutModifiers.Shift, 'L');

    private static readonly KeyCombination CloseApplication =
        new(ShortcutModifiers.Primary, 'Q');

    // --- Translation of physical gestures to the model (COM-002, T-011) ---

    [Fact]
    public void Ctrl_shift_l_translates_to_the_register_combination()
    {
        var combination = GestureTranslator.Translate(
            Key.L, KeyModifiers.Control | KeyModifiers.Shift, isMacOS: false);

        Assert.Equal(ToggleLanguage, combination);
    }

    [Fact]
    public void Cmd_shift_l_on_macos_translates_to_the_same_logical_combination()
    {
        var combination = GestureTranslator.Translate(
            Key.L, KeyModifiers.Meta | KeyModifiers.Shift, isMacOS: true);

        Assert.Equal(ToggleLanguage, combination);
    }

    [Fact]
    public void Ctrl_q_and_cmd_q_translate_to_the_close_shortcut()
    {
        Assert.Equal(
            CloseApplication,
            GestureTranslator.Translate(Key.Q, KeyModifiers.Control, isMacOS: false));
        Assert.Equal(
            CloseApplication,
            GestureTranslator.Translate(Key.Q, KeyModifiers.Meta, isMacOS: true));
    }

    [Fact]
    public void The_physical_ctrl_on_macos_is_not_the_primary_modifier()
    {
        // On macOS the model's primary is Cmd: a physical Ctrl+Shift+L is not
        // the assigned shortcut and remains a combination without primary,
        // inert.
        var combination = GestureTranslator.Translate(
            Key.L, KeyModifiers.Control | KeyModifiers.Shift, isMacOS: true);

        Assert.Equal(new KeyCombination(ShortcutModifiers.Shift, 'L'), combination);
    }

    [Fact]
    public void Alt_translates_to_the_alternative_modifier()
    {
        var combination = GestureTranslator.Translate(Key.L, KeyModifiers.Alt, isMacOS: false);

        Assert.Equal(new KeyCombination(ShortcutModifiers.Alt, 'L'), combination);
    }

    [Fact]
    public void Digits_translate_in_both_keyboard_blocks()
    {
        Assert.Equal(
            new KeyCombination(ShortcutModifiers.None, '1'),
            GestureTranslator.Translate(Key.D1, KeyModifiers.None, isMacOS: false));
        Assert.Equal(
            new KeyCombination(ShortcutModifiers.None, '5'),
            GestureTranslator.Translate(Key.NumPad5, KeyModifiers.None, isMacOS: false));
    }

    [Fact]
    public void Keys_without_letter_or_digit_are_not_expressible()
    {
        Assert.Null(GestureTranslator.Translate(Key.Enter, KeyModifiers.Control, isMacOS: false));
        Assert.Null(GestureTranslator.Translate(Key.F5, KeyModifiers.None, isMacOS: false));
        Assert.Null(GestureTranslator.Translate(Key.Left, KeyModifiers.None, isMacOS: false));
        Assert.Null(GestureTranslator.Translate(Key.Escape, KeyModifiers.None, isMacOS: true));
    }

    // --- Wiring of SC-001's keyboard (COM-002) ---

    [Fact]
    public void Ctrl_shift_l_in_the_window_toggles_the_language_with_relabeling()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();

        window.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control | RawInputModifiers.Shift);

        Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);
        Assert.Equal("File", MenuHeaderOf(window, "menu.archivo"));

        // Two presses return to Spanish with its labels (toggling).
        window.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control | RawInputModifiers.Shift);

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal("Archivo", MenuHeaderOf(window, "menu.archivo"));
    }

    [Fact]
    public void Ctrl_q_in_the_window_closes_the_application()
    {
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();
        var closed = false;
        window.Closed += (_, _) => closed = true;

        window.KeyPressQwerty(PhysicalKey.Q, RawInputModifiers.Control);

        Assert.True(closed);
    }

    [Fact]
    public void Unassigned_combinations_remain_inert()
    {
        // BR-F001-003 (ADR-001): no observable effect on the interface.
        var session = new ApplicationSession();
        var window = new MainWindow(session);
        window.Show();

        // Ctrl+X is not assigned; Ctrl+L without Shift is not either (it only
        // alters Ctrl+Shift+L); Shift+L without primary, not either.
        window.KeyPressQwerty(PhysicalKey.X, RawInputModifiers.Control | RawInputModifiers.Shift);
        window.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Control);
        window.KeyPressQwerty(PhysicalKey.L, RawInputModifiers.Shift);

        Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        Assert.Equal("Archivo", MenuHeaderOf(window, "menu.archivo"));
    }

    private static string MenuHeaderOf(MainWindow window, string key)
    {
        var bar = window.FindControl<Avalonia.Controls.Menu>("MenuBar")!;
        var items = bar.Items
            .OfType<Avalonia.Controls.MenuItem>()
            .SelectMany(menu => menu.Items.OfType<Avalonia.Controls.MenuItem>())
            .Concat(bar.Items.OfType<Avalonia.Controls.MenuItem>());

        return items.Single(item => (string?)item.Tag == key).Header as string
            ?? throw new InvalidOperationException($"Item {key} has no label.");
    }
}
