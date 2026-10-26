namespace EggMdEditor.App;

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
        return c is "close"
            or "openmenu" or "closemenu" or "togglemenu"
            or "shownotice" or "hidenotice" or "togglenotice"
            or "changelanguage"
            or "changetheme" or "toggletheme"
            or "requestfocus";
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
    public const string OpenMenu = "OpenMenu";
    public const string CloseMenu = "CloseMenu";
    public const string ToggleMenu = "ToggleMenu";
    public const string ShowNotice = "ShowNotice";
    public const string HideNotice = "HideNotice";
    public const string ToggleNotice = "ToggleNotice";
    public const string ChangeLanguage = "ChangeLanguage";
    public const string ChangeTheme = "ChangeTheme";
    public const string ToggleTheme = "ToggleTheme";
    public const string RequestFocus = "RequestFocus";
}

public static class ShortcutNames
{
    public const string Close = "Ctrl+W";
    public const string Escape = "Esc";
}
