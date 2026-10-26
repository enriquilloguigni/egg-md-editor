namespace EggMdEditor.App;

/// <summary>
/// Resultado de despachar un comando compartido (COM-001). En F-001 no hay
/// acciones conectadas (ADR-001), por lo que el efecto observable es nulo.
/// </summary>
public enum CommandResult
{
    NoEffect = 0,
    Handled = 1,
    Ignored = 2,
    NotFound = 3,
    NotAvailable = 4,
}

public enum DispatchResult
{
    NoEffect = 0,
    Handled = 1,
    Ignored = 2,
    NotFound = 3,
    NotAvailable = 4,
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
        Command = Name;
    }

    public SharedCommand(string name, string? command)
    {
        Name = name ?? string.Empty;
        Command = command ?? Name;
    }

    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Argument { get; set; } = string.Empty;

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Command) ? Name : Command;
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
        Command = command ?? string.Empty;
        Active = true;
        IsAvailable = IsAvailableCommand(Command);
    }

    public Shortcut(string combination, SharedCommand? command)
        : this(combination, ResolveCommandName(command))
    {
        CommandRef = command;
        Argument = command?.Argument ?? string.Empty;
    }

    public string Combination { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Argument { get; set; } = string.Empty;
    public SharedCommand? CommandRef { get; set; }
    public bool Active { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool IsAvailable { get; set; }

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
            or "changetheme" or "toggletheme"
            or "requestfocus" or "escape" or "esc";
    }

    private static string ResolveCommandName(SharedCommand? command)
    {
        if (command is null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(command.Name) ? command.Command : command.Name;
    }

    public override string ToString() => $"{Combination} -> {Command}";
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
