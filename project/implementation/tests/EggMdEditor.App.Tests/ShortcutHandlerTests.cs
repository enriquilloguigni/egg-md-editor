using System;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

// F-001/T-011: handler de atajos disponibles en F-001 (OP-005,
// BR-F001-003, AC-004). Solo responden Alt+letra (entradas del menú), Esc
// (cierre del menú) y Ctrl+W (cierre de la ventana); el resto permanece
// inerte y no altera el documento ni el estado de la sesión (ADR-001).
public class ShortcutHandlerTests
{
    private static MainWindowViewModel CreateViewModel(out Session session)
    {
        session = new Session(new I18nCatalog());
        return new MainWindowViewModel(session);
    }

    [Fact]
    public void ViewModel_RegistersOnlyTheShortcutsAvailableInF001()
    {
        var viewModel = CreateViewModel(out _);

        // Alt+letra: una combinación por cada menú de ENT-004, con el
        // mnemónico del rótulo español activo (defecto, BR-F001-004).
        Assert.Equal("Menu.File", viewModel.MenuShortcuts["Alt+A"]);
        Assert.Equal("Menu.Edit", viewModel.MenuShortcuts["Alt+E"]);
        Assert.Equal("Menu.View", viewModel.MenuShortcuts["Alt+V"]);
        Assert.Equal("Menu.Format", viewModel.MenuShortcuts["Alt+F"]);
        Assert.Equal("Menu.Tools", viewModel.MenuShortcuts["Alt+H"]);
        Assert.Equal("Menu.Help", viewModel.MenuShortcuts["Alt+Y"]);

        Assert.Equal("Menu.File", viewModel.ResolveShortcut("Alt+A")?.Argument);
        Assert.Equal("Menu.Format", viewModel.ResolveShortcut("Alt+F")?.Argument);
        Assert.Equal(CommandNames.CloseMenu, viewModel.ResolveShortcut(ShortcutNames.Escape)?.Command);
        Assert.Equal(CommandNames.Close, viewModel.ResolveShortcut(ShortcutNames.Close)?.Command);

        // Solo los atajos de acciones disponibles en F-001 están registrados.
        Assert.True(viewModel.ResolveShortcut("Alt+A")!.IsAvailable);
        Assert.Equal(6 + 2, viewModel.ShortcutCount);
        Assert.Null(viewModel.ResolveShortcut("Ctrl+B"));
        Assert.Null(viewModel.ResolveShortcut("Ctrl+S"));
    }

    [Fact]
    public void ViewModel_RebindsAltLetterMnemonicWhenTheLanguageChanges()
    {
        var viewModel = CreateViewModel(out _);

        Assert.True(viewModel.TryChangeLanguage("en"));

        // En inglés el mnemónico sigue al rótulo visible: File=F, Format=O,
        // Tools=T, Help=H; los mnemónicos exclusivos del español desaparecen.
        Assert.Equal("Menu.File", viewModel.MenuShortcuts["Alt+F"]);
        Assert.Equal("Menu.Format", viewModel.MenuShortcuts["Alt+O"]);
        Assert.Equal("Menu.Tools", viewModel.MenuShortcuts["Alt+T"]);
        Assert.Equal("Menu.Help", viewModel.MenuShortcuts["Alt+H"]);
        Assert.False(viewModel.MenuShortcuts.ContainsKey("Alt+A"));
        Assert.False(viewModel.MenuShortcuts.ContainsKey("Alt+Y"));
        Assert.Null(viewModel.ResolveShortcut("Alt+A"));
        Assert.Equal("Menu.File", viewModel.ResolveShortcut("Alt+F")?.Argument);

        // Esc y el cierre siguen disponibles tras la conmutación.
        Assert.NotNull(viewModel.ResolveShortcut(ShortcutNames.Escape));
        Assert.NotNull(viewModel.ResolveShortcut(ShortcutNames.Close));
    }

    [Fact]
    public void Dispatcher_ExecutesAvailableShortcutsAndLeavesTheRestInert()
    {
        var viewModel = CreateViewModel(out var session);

        Assert.True(viewModel.DispatchShortcut("Alt+A"));
        Assert.True(viewModel.IsMenuOpen);
        Assert.Equal(MenuState.Open, session.MenuState);
        Assert.Equal("Menu.File", viewModel.OpenMenuKey);

        // TR-005: Esc solo cierra el menú desplegado.
        Assert.True(viewModel.DispatchShortcut(ShortcutNames.Escape));
        Assert.False(viewModel.IsMenuOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.False(viewModel.DispatchShortcut(ShortcutNames.Escape));

        // Atajo no registrado: sin efecto observable (BR-F001-003).
        Assert.False(viewModel.DispatchShortcut("Ctrl+B"));
        Assert.False(viewModel.IsMenuOpen);
        Assert.False(viewModel.NoticeVisible);

        // Atajo registrado cuya acción no existe en F-001: inerte.
        Assert.True(viewModel.RegisterShortcut("Ctrl+S", "Save"));
        Assert.False(viewModel.DispatchShortcut("Ctrl+S"));
        Assert.False(viewModel.IsMenuOpen);
    }

    [Fact]
    public void Dispatcher_CloseShortcutRequestsTheWindowClose()
    {
        var viewModel = CreateViewModel(out var session);
        var closeRequested = false;
        viewModel.CloseRequested += (_, _) => closeRequested = true;

        Assert.True(viewModel.DispatchShortcut(ShortcutNames.Close));
        Assert.True(closeRequested);
        Assert.False(session.WindowOpen);

        // OP-006/TR-001: el cierre no deja el menú ni el aviso activos.
        Assert.False(viewModel.IsMenuOpen);
        Assert.False(viewModel.NoticeVisible);
    }

    [Fact]
    public void Dispatcher_ExecutesAvailableCommandsFromTheSharedDispatcher()
    {
        var session = new Session(new I18nCatalog());
        var dispatcher = new CommandDispatcher(session);

        Assert.True(dispatcher.RegisterShortcut("Ctrl+M", CommandNames.OpenMenu));
        Assert.True(dispatcher.RegisterShortcut(ShortcutNames.Escape, CommandNames.CloseMenu));

        Assert.True(dispatcher.DispatchShortcut("Ctrl+M"));
        Assert.Equal(MenuState.Open, session.MenuState);

        Assert.True(dispatcher.DispatchShortcut(ShortcutNames.Escape));
        Assert.Equal(MenuState.Closed, session.MenuState);

        // Atajos no registrados o de acciones de otras Features: inertes.
        Assert.False(dispatcher.DispatchShortcut("Ctrl+B"));
        Assert.False(dispatcher.Dispatch("Save"));
        Assert.Equal(CommandResult.NotAvailable, dispatcher.DispatchCommand("Save"));

        Assert.True(dispatcher.Dispatch(CommandNames.OpenMenu));
        Assert.Equal(DispatchResult.Handled, dispatcher.Execute(CommandNames.CloseMenu));
        Assert.Equal(MenuState.Closed, session.MenuState);
    }

    [Theory]
    [InlineData(Key.A, KeyModifiers.Alt, PhysicalKey.A, "Alt+A")]
    [InlineData(Key.F, KeyModifiers.Alt, PhysicalKey.F, "Alt+F")]
    [InlineData(Key.W, KeyModifiers.Control, PhysicalKey.W, "Ctrl+W")]
    [InlineData(Key.B, KeyModifiers.Control, PhysicalKey.B, "Ctrl+B")]
    [InlineData(Key.Escape, KeyModifiers.None, PhysicalKey.Escape, "Esc")]
    [InlineData(Key.System, KeyModifiers.Alt, PhysicalKey.F, "Alt+F")]
    public void Handler_MapsThePressedKeyToItsCanonicalCombination(
        Key key,
        KeyModifiers modifiers,
        PhysicalKey physicalKey,
        string expected)
    {
        Assert.Equal(expected, ShortcutHandler.GetCombination(key, modifiers, physicalKey));
    }

    [Fact]
    public void Handler_DoesNotConsumeUnmodifiedKeys()
    {
        Assert.Null(ShortcutHandler.GetCombination(Key.A, KeyModifiers.None, PhysicalKey.A));
        Assert.Null(ShortcutHandler.GetCombination(Key.F1, KeyModifiers.None, PhysicalKey.F1));
    }

    [AvaloniaFact]
    public void Window_AltLetterOpensItsMenuAndEscapeClosesIt()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // OP-005/TR-004: Alt+letra abre la entrada de ENT-004.
            window.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.Alt);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(viewModel.IsMenuOpen);
            Assert.Equal("Menu.File", viewModel.OpenMenuKey);

            // TR-005: Esc cierra el menú sin acción de dominio (ADR-001).
            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
            Assert.Null(viewModel.OpenMenuKey);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_UnavailableShortcutsRemainInert()
    {
        var viewModel = CreateViewModel(out var session);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.B, RawInputModifiers.Control);
            window.KeyPressQwerty(PhysicalKey.Q, RawInputModifiers.Alt);
            window.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.IsMenuOpen);
            Assert.False(viewModel.NoticeVisible);
            Assert.Equal(MenuState.Closed, session.MenuState);
            Assert.True(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_CloseShortcutClosesTheWindow()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            window.KeyPressQwerty(PhysicalKey.W, RawInputModifiers.Control);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_AltLetterFocusesTheOpenedEntryAndLeavesOtherEntriesUntouched()
    {
        var viewModel = CreateViewModel(out _);
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // En español Formato usa F (File no existe como rótulo).
            window.KeyPressQwerty(PhysicalKey.F, RawInputModifiers.Alt);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("Menu.Format", viewModel.OpenMenuKey);

            var formatMenu = window.FindControl<MenuItem>("FormatMenu");
            var fileMenu = window.FindControl<MenuItem>("FileMenu");
            Assert.NotNull(formatMenu);
            Assert.NotNull(fileMenu);
            Assert.True(formatMenu.IsFocused);
            Assert.False(fileMenu.IsFocused);
        }
        finally
        {
            window.Close();
        }
    }
}
