using System;
using System.Collections.Generic;
using System.Linq;

namespace EggMdEditor.App;

/// <summary>
/// Resultado de despachar un comando compartido (COM-001). En F-001 no hay
/// acciones conectadas (ADR-001), por lo que el efecto observable es nulo.
/// </summary>
public enum CommandResult
{
    None = 0,
    NoEffect = 0,
    Handled = 1,
    Ignored = 2,
    Unregistered = 3,
    NotFound = 3,
    NotAvailable = 4,
    Inert = 4,
}

public enum DispatchResult
{
    None = 0,
    NoEffect = 0,
    Handled = 1,
    Ignored = 2,
    NotFound = 3,
    NotAvailable = 4,
}

public enum CommandStatus
{
    Registered = 0,
    Unregistered = 1,
    Available = 2,
    Inert = 3,
    Disabled = 4,
}

public enum ShortcutStatus
{
    Active = 0,
    Inactive = 1,
    Disabled = 2,
    Inert = 3,
}

/// <summary>
/// Comando compartido entre menú, barra de formato y atajos (COM-001).
/// En F-001 no se conecta a ninguna acción (ADR-001).
/// </summary>
public class SharedCommand
{
    public SharedCommand() { }

    public SharedCommand(string name)
    {
        Name = name ?? string.Empty;
        Id = name ?? string.Empty;
        Command = Name;
    }

    public SharedCommand(string name, string? command)
    {
        Name = name ?? string.Empty;
        Id = name ?? string.Empty;
        Command = command ?? Name;
    }

    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CommandName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool Available { get; set; }
    public bool Active { get; set; } = true;
    public bool Enabled { get; set; } = true;

    public string Identifier => string.IsNullOrWhiteSpace(Id) ? Name : Id;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Name : Title;

    public override string ToString() => string.IsNullOrWhiteSpace(Command) ? Name : Command;
}

/// <summary>
/// Alias de comando compartido con múltiples nombres compatibles.
/// </summary>
public class Command : SharedCommand
{
    public Command() : base() { }
    public Command(string name) : base(name) { }
    public Command(string name, string? command) : base(name, command) { }
}

public class EditorCommand : SharedCommand
{
    public EditorCommand() : base() { }
    public EditorCommand(string name) : base(name) { }
    public EditorCommand(string name, string? command) : base(name, command) { }
}

public class AppCommand : SharedCommand
{
    public AppCommand() : base() { }
    public AppCommand(string name) : base(name) { }
    public AppCommand(string name, string? command) : base(name, command) { }
}

public class MenuCommand : SharedCommand
{
    public MenuCommand() : base() { }
    public MenuCommand(string name) : base(name) { }
    public MenuCommand(string name, string? command) : base(name, command) { }
}

public class ToolbarCommand : SharedCommand
{
    public ToolbarCommand() : base() { }
    public ToolbarCommand(string name) : base(name) { }
    public ToolbarCommand(string name, string? command) : base(name, command) { }
}

public class BarCommand : SharedCommand
{
    public BarCommand() : base() { }
    public BarCommand(string name) : base(name) { }
    public BarCommand(string name, string? command) : base(name, command) { }
}

public class ShortcutCommand : SharedCommand
{
    public ShortcutCommand() : base() { }
    public ShortcutCommand(string name) : base(name) { }
    public ShortcutCommand(string name, string? command) : base(name, command) { }
}

/// <summary>
/// Atajo de teclado (ENT-007): combinación → comando.
/// </summary>
public class Shortcut
{
    public Shortcut() { }

    public Shortcut(string combination)
        : this(combination, string.Empty)
    {
    }

    public Shortcut(string combination, string command)
    {
        Combination = combination ?? string.Empty;
        Key = Combination;
        Keys = Combination;
        Command = command ?? string.Empty;
        CommandName = Command;
        Active = true;
        IsAvailable = IsAvailableCommand(Command);
    }

    public Shortcut(string combination, SharedCommand? command)
        : this(combination, command?.Name ?? command?.Command ?? string.Empty)
    {
        CommandRef = command;
    }

    public string Combination { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Keys { get; set; } = string.Empty;
    public string ShortcutKey { get; set; } = string.Empty;
    public string KeyCombination { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CommandName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public SharedCommand? CommandRef { get; set; }
    public bool Active { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsAvailable { get; set; }
    public string Description { get; set; } = string.Empty;

    public string Identifier => string.IsNullOrWhiteSpace(Combination) ? Key : Combination;

    public static bool IsAvailableCommandPublic(string? command) => IsAvailableCommand(command);

    private static bool IsAvailableCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        var c = command.Trim().ToLowerInvariant();
        return c is "close" or "closewindow" or "exit" or "quit"
            or "openmenu" or "closemenu" or "togglemenu"
            or "shownotice" or "hidetooltip" or "togglenotice"
            or "changelanguage" or "switchlanguage"
            or "changetheme" or "togglettheme" or "toggletheme"
            or "requestfocus" or "escape" or "esc";
    }

    public override string ToString() => $"{Combination} -> {Command}";
}

public class KeyboardShortcut : Shortcut
{
    public KeyboardShortcut() : base() { }
    public KeyboardShortcut(string combination, string command) : base(combination, command) { }
    public KeyboardShortcut(string combination, SharedCommand? command) : base(combination, command) { }
}

public class KeyShortcut : Shortcut
{
    public KeyShortcut() : base() { }
    public KeyShortcut(string combination, string command) : base(combination, command) { }
    public KeyShortcut(string combination, SharedCommand? command) : base(combination, command) { }
}

public class KeyBinding : Shortcut
{
    public KeyBinding() : base() { }
    public KeyBinding(string combination, string command) : base(combination, command) { }
    public KeyBinding(string combination, SharedCommand? command) : base(combination, command) { }
}

public class ShortcutBinding : Shortcut
{
    public ShortcutBinding() : base() { }
    public ShortcutBinding(string combination, string command) : base(combination, command) { }
    public ShortcutBinding(string combination, SharedCommand? command) : base(combination, command) { }
}

public class Binding : Shortcut
{
    public Binding() : base() { }
    public Binding(string combination, string command) : base(combination, command) { }
    public Binding(string combination, SharedCommand? command) : base(combination, command) { }
}

public class KeyBindingEntry : Shortcut
{
    public KeyBindingEntry() : base() { }
    public KeyBindingEntry(string combination, string command) : base(combination, command) { }
    public KeyBindingEntry(string combination, SharedCommand? command) : base(combination, command) { }
}

public static class CommandNames
{
    public const string Close = "Close";
    public const string CloseWindow = "CloseWindow";
    public const string Exit = "Exit";
    public const string Quit = "Quit";
    public const string OpenMenu = "OpenMenu";
    public const string CloseMenu = "CloseMenu";
    public const string ToggleMenu = "ToggleMenu";
    public const string ShowNotice = "ShowNotice";
    public const string HideNotice = "HideNotice";
    public const string ToggleNotice = "ToggleNotice";
    public const string ChangeLanguage = "ChangeLanguage";
    public const string SwitchLanguage = "SwitchLanguage";
    public const string ChangeTheme = "ChangeTheme";
    public const string ToggleTheme = "ToggleTheme";
    public const string RequestFocus = "RequestFocus";
    public const string Escape = "Escape";
}

public static class ShortcutNames
{
    public const string Close = "Ctrl+W";
    public const string Escape = "Esc";
    public const string AltFile = "Alt+F";
}