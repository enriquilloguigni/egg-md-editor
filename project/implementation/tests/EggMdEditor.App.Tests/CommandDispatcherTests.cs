using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

// F-001/T-019: cobertura de comportamiento del dispatcher y del registro de
// atajos (COM-001, ENT-007, OP-005) que sustituye a la antigua API de alias.
public class CommandDispatcherTests
{
    [Fact]
    public void Commands_RegisterResolveAndRemove()
    {
        var dispatcher = new CommandDispatcher();

        Assert.True(dispatcher.RegisterCommand("OpenMenu"));
        Assert.True(dispatcher.ContainsCommand("openmenu"));
        Assert.Equal("OpenMenu", dispatcher.ResolveCommand("OpenMenu")?.Command);
        Assert.Single(dispatcher.Commands);

        Assert.True(dispatcher.RegisterCommand(new SharedCommand("Save", "Save")));
        Assert.True(dispatcher.ContainsCommand("Save"));

        Assert.True(dispatcher.RemoveCommand("OpenMenu"));
        Assert.False(dispatcher.ContainsCommand("OpenMenu"));
        Assert.Null(dispatcher.ResolveCommand("OpenMenu"));
        Assert.False(dispatcher.RemoveCommand("OpenMenu"));
        Assert.False(dispatcher.RemoveCommand(null));

        dispatcher.ClearCommands();
        Assert.Empty(dispatcher.Commands);
    }

    [Fact]
    public void Shortcuts_RegisterResolveAndRemove()
    {
        var dispatcher = new CommandDispatcher();

        Assert.True(dispatcher.RegisterShortcut("Alt+A", CommandNames.OpenMenu));
        Assert.True(dispatcher.ContainsShortcut("alt+a"));

        var shortcut = dispatcher.ResolveShortcut("ALT+A");
        Assert.NotNull(shortcut);
        Assert.True(shortcut!.IsAvailable);
        Assert.True(dispatcher.TryGetShortcut("Alt+A", out var found));
        Assert.Same(shortcut, found);
        Assert.Equal(1, dispatcher.ShortcutCount);
        Assert.Equal(1, dispatcher.ShortcutsRegistry.Count);

        Assert.True(dispatcher.RemoveShortcut("alt+a"));
        Assert.False(dispatcher.ContainsShortcut("Alt+A"));
        Assert.Equal(0, dispatcher.ShortcutCount);
        Assert.False(dispatcher.RemoveShortcut("Alt+A"));
    }

    [Fact]
    public void ShortcutRegistry_RegistersFindsRemovesAndInvokesThroughDispatcher()
    {
        var session = new Session(new I18nCatalog());
        var dispatcher = new CommandDispatcher(session);
        var registry = dispatcher.ShortcutsRegistry;

        Assert.True(registry.Register("Ctrl+M", CommandNames.OpenMenu));
        Assert.Equal(1, registry.Count);
        Assert.True(registry.TryGet("Ctrl+M", out var shortcut));
        Assert.NotNull(shortcut);
        Assert.True(registry.Contains("Ctrl+M"));

        Assert.True(registry.Invoke("Ctrl+M"));
        Assert.True(session.IsMenuOpen);

        Assert.True(registry.Remove("Ctrl+M"));
        Assert.Empty(registry.Shortcuts);
        Assert.False(registry.Invoke("Ctrl+M"));
    }

    [Fact]
    public void Dispatch_AvailableCommandsAffectTheSessionAndUnknownCommandsStayInert()
    {
        var session = new Session(new I18nCatalog());
        var dispatcher = new CommandDispatcher(session);

        Assert.True(dispatcher.Dispatch(CommandNames.OpenMenu));
        Assert.True(session.IsMenuOpen);

        Assert.Equal(DispatchResult.Handled, dispatcher.Execute(CommandNames.CloseMenu));
        Assert.False(session.IsMenuOpen);

        Assert.False(dispatcher.Dispatch("Save"));
        Assert.Equal(CommandResult.NotAvailable, dispatcher.DispatchCommand("Save"));
        Assert.False(dispatcher.Dispatch((string?)null));
        Assert.False(dispatcher.Dispatch((SharedCommand?)null));
        Assert.Equal(CommandResult.NoEffect, dispatcher.DispatchCommand((string?)null));

        // Sin sesión compuesta no hay efecto observable.
        Assert.False(new CommandDispatcher().Dispatch(CommandNames.OpenMenu));
    }

    [Fact]
    public void DispatchShortcut_RespectsAvailabilityAndTheActiveState()
    {
        var session = new Session(new I18nCatalog());
        var dispatcher = new CommandDispatcher(session);

        Assert.True(dispatcher.RegisterShortcut("Alt+A", CommandNames.OpenMenu));
        Assert.True(dispatcher.DispatchShortcut("Alt+A"));
        Assert.True(session.IsMenuOpen);
        session.CloseMenu();

        var shortcut = dispatcher.ResolveShortcut("Alt+A");
        Assert.NotNull(shortcut);

        shortcut!.Active = false;
        Assert.False(dispatcher.DispatchShortcut("Alt+A"));

        shortcut.Active = true;
        shortcut.Enabled = false;
        Assert.False(dispatcher.DispatchShortcut(shortcut));

        shortcut.Enabled = true;
        Assert.True(dispatcher.DispatchShortcut("Alt+A"));
        Assert.True(session.IsMenuOpen);

        // Un atajo registrado cuya acción no existe en F-001 permanece inerte.
        Assert.True(dispatcher.RegisterShortcut("Ctrl+B", "Bold"));
        Assert.False(dispatcher.DispatchShortcut("Ctrl+B"));
    }

    [Fact]
    public void Registration_RejectsInvalidOrEmptyInputs()
    {
        var dispatcher = new CommandDispatcher();

        Assert.False(dispatcher.RegisterCommand((string?)null));
        Assert.False(dispatcher.RegisterCommand(" "));
        Assert.False(dispatcher.RegisterCommand((SharedCommand?)null));
        Assert.False(dispatcher.RegisterShortcut((Shortcut?)null));
        Assert.False(dispatcher.RegisterShortcut(" ", CommandNames.OpenMenu));
        Assert.False(dispatcher.RegisterShortcut(" ", (SharedCommand?)null));
        Assert.False(dispatcher.ContainsCommand(null));
        Assert.Null(dispatcher.ResolveCommand(null));
        Assert.Null(dispatcher.ResolveShortcut(null));
        Assert.False(dispatcher.ContainsShortcut(null));
    }
}
