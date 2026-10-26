using System;
using System.Collections.Generic;
using System.Linq;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Dispatcher de comandos compartidos entre menú, barra de formato y atajos (COM-001).
/// En F-001 las acciones no están conectadas (ADR-001): el despacho no produce
/// efecto observable sobre el documento ni la sesión.
/// </summary>
public class CommandDispatcher
{
    private readonly List<SharedCommand> _commands = new();
    private readonly Dictionary<string, SharedCommand> _commandMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Shortcut> _shortcuts = new();
    private readonly Dictionary<string, Shortcut> _shortcutMap = new(StringComparer.OrdinalIgnoreCase);

    public CommandDispatcher()
        : this((Session?)null, (ShortcutRegistry?)null)
    {
    }

    public CommandDispatcher(Session? session)
        : this(session, (ShortcutRegistry?)null)
    {
    }

    public CommandDispatcher(II18nCatalog? catalog)
        : this((Session?)null, (ShortcutRegistry?)null)
    {
        Catalog = catalog;
    }

    public CommandDispatcher(Session? session, ShortcutRegistry? shortcuts)
    {
        Session = session;
        ShortcutsRegistry = shortcuts ?? new ShortcutRegistry(this);
        if (shortcuts is not null && shortcuts.Dispatcher is null)
        {
            shortcuts.Dispatcher = this;
        }
    }

    public Session? Session { get; set; }
    public II18nCatalog? Catalog { get; set; }
    public UiPreferences? Preferences { get; set; }
    public ShortcutRegistry ShortcutsRegistry { get; set; }

    public IReadOnlyList<SharedCommand> Commands => _commands.AsReadOnly();
    public int ShortcutCount => _shortcuts.Count;

    // ---- Command registration ----
    public bool RegisterCommand(SharedCommand? command)
    {
        if (command is null)
        {
            return false;
        }

        var name = FirstNonEmpty(command.Name, command.Command);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = command.ToString();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (!_commandMap.ContainsKey(name))
        {
            _commands.Add(command);
        }
        _commandMap[name] = command;
        if (string.IsNullOrWhiteSpace(command.Name)) command.Name = name;
        if (string.IsNullOrWhiteSpace(command.Command)) command.Command = name;
        return true;
    }

    public bool RegisterCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return RegisterCommand(new SharedCommand(name!));
    }

    public bool ContainsCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _commandMap.ContainsKey(name!);
    }

    public SharedCommand? ResolveCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (_commandMap.TryGetValue(name!, out var command))
        {
            return command;
        }

        return _commands.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Command, name, StringComparison.OrdinalIgnoreCase));
    }

    public bool RemoveCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || !_commandMap.ContainsKey(name!))
        {
            return false;
        }

        var command = _commandMap[name!];
        _commandMap.Remove(name!);
        _commands.Remove(command);
        return true;
    }

    public void ClearCommands()
    {
        _commands.Clear();
        _commandMap.Clear();
    }

    // ---- Shortcut registration ----
    public bool RegisterShortcut(Shortcut? shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        NormalizeShortcut(shortcut);
        var key = FirstNonEmpty(shortcut.Combination);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (!_shortcutMap.ContainsKey(key))
        {
            _shortcuts.Add(shortcut);
        }
        _shortcutMap[key] = shortcut;
        ShortcutsRegistry.RegisterShortcutInternal(shortcut);
        return true;
    }

    public bool RegisterShortcut(string? combination, string? command)
    {
        return RegisterShortcut(new Shortcut(combination ?? string.Empty, command ?? string.Empty));
    }

    public bool RegisterShortcut(string? combination, SharedCommand? command)
    {
        return RegisterShortcut(new Shortcut(combination ?? string.Empty, command));
    }

    public Shortcut? ResolveShortcut(string? combination)
    {
        if (string.IsNullOrWhiteSpace(combination))
        {
            return null;
        }

        if (_shortcutMap.TryGetValue(combination!, out var shortcut))
        {
            return shortcut;
        }

        return _shortcuts.FirstOrDefault(x =>
            string.Equals(
                NormalizeKey(x.Combination),
                NormalizeKey(combination),
                StringComparison.Ordinal));
    }

    public bool TryGetShortcut(string? combination, out Shortcut? shortcut)
    {
        shortcut = ResolveShortcut(combination);
        return shortcut is not null;
    }

    public bool ContainsShortcut(string? combination) => ResolveShortcut(combination) is not null;

    public bool RemoveShortcut(string? combination)
    {
        var shortcut = ResolveShortcut(combination);
        if (shortcut is null)
        {
            return false;
        }

        _shortcuts.Remove(shortcut);
        var key = FirstNonEmpty(shortcut.Combination);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _shortcutMap.Remove(key);
        }
        ShortcutsRegistry.RemoveShortcutInternal(shortcut);
        return true;
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
        _shortcutMap.Clear();
        ShortcutsRegistry.Clear();
    }

    // ---- Dispatch (OP-005, COM-001, BR-F001-003) ----
    /// <summary>
    /// Ejecuta un comando registrado (OP-005). Solo los comandos de acciones
    /// disponibles en F-001 producen efecto; el resto permanece inerte
    /// (BR-F001-003, ADR-001). <paramref name="command"/> transporta el
    /// parámetro de la acción (p. ej. la entrada de menú de OP-003).
    /// </summary>
    public bool Dispatch(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        return Dispatch(new SharedCommand(command!));
    }

    public bool Dispatch(SharedCommand? command)
    {
        if (command is null)
        {
            return false;
        }

        var name = ResolveCommandName(command);
        if (!Shortcut.IsAvailableCommandPublic(name))
        {
            // Comando de una acción que aún no existe en F-001: inerte.
            return false;
        }

        return ExecuteAvailableCommand(command, command.Argument);
    }

    /// <summary>
    /// OP-005: invoke de un atajo registrado en ENT-007. Un atajo no
    /// registrado, inactivo o cuyo comando no está disponible en F-001 no
    /// produce efecto observable.
    /// </summary>
    public bool DispatchShortcut(string? combination)
    {
        var shortcut = ResolveShortcut(combination);
        return DispatchShortcut(shortcut);
    }

    public bool DispatchShortcut(Shortcut? shortcut)
    {
        if (shortcut is null || !shortcut.Active || !shortcut.Enabled)
        {
            return false;
        }

        var command = ResolveShortcutCommand(shortcut);
        if (command is null)
        {
            return false;
        }

        var name = ResolveCommandName(command);
        if (!Shortcut.IsAvailableCommandPublic(name))
        {
            // El binding existe pero su acción pertenece a otra Feature: inerte.
            return false;
        }

        var argument = FirstNonEmpty(shortcut.Argument, command.Argument);
        return ExecuteAvailableCommand(command, string.IsNullOrWhiteSpace(argument) ? null : argument);
    }

    public CommandResult DispatchCommand(string? command)
    {
        return ToCommandResult(command, Dispatch(command));
    }

    public CommandResult DispatchCommand(SharedCommand? command)
    {
        return ToCommandResult(command, Dispatch(command));
    }

    public DispatchResult Execute(string? command) => ToDispatchResult(DispatchCommand(command));
    public DispatchResult Execute(SharedCommand? command) => ToDispatchResult(DispatchCommand(command));

    /// <summary>
    /// Punto único de ejecución de las acciones disponibles en F-001
    /// (COM-001). Las Features posteriores sustituyen o amplían este
    /// despacho sin conectar aquí ninguna acción de dominio (ADR-001).
    /// </summary>
    protected virtual bool ExecuteAvailableCommand(SharedCommand command, string? argument)
    {
        switch (NormalizeCommandName(ResolveCommandName(command)))
        {
            case "openmenu":
                // OP-003: la entrada concreta la conserva el ViewModel; la
                // sesión solo refleja {Cerrado, Desplegado}.
                return Session?.TryOpenMenu() ?? false;
            case "closemenu":
            case "escape":
            case "esc":
                // TR-005: solo hay efecto con el menú desplegado.
                return Session?.TryCloseMenu() ?? false;
            case "togglemenu":
                if (Session is null)
                {
                    return false;
                }

                Session.ToggleMenu();
                return true;
            case "close":
            case "closewindow":
            case "exit":
            case "quit":
                if (Session is null)
                {
                    return false;
                }

                Session.CloseWindow();
                return true;
            case "changelanguage":
            case "switchlanguage":
                return !string.IsNullOrWhiteSpace(argument) && (Session?.TrySetLanguage(argument) ?? false);
            case "changetheme":
                return !string.IsNullOrWhiteSpace(argument) && (Session?.TrySetTheme(argument) ?? false);
            case "toggletheme":
                if (Session is null)
                {
                    return false;
                }

                Session.ToggleTheme();
                return true;
            case "shownotice":
                Session?.ShowNotice();
                return Session is not null;
            case "hidenotice":
            case "hidetooltip":
                Session?.HideNotice();
                return Session is not null;
            case "togglenotice":
                Session?.ToggleNotice();
                return Session is not null;
            case "requestfocus":
                Session?.RequestFocus();
                return Session is not null;
            default:
                return false;
        }
    }

    // Events intentionally omitted: no domain actions connected in F-001 (ADR-001).

    protected static string ResolveCommandName(SharedCommand command)
    {
        return FirstNonEmpty(command.Command, command.Name);
    }

    private SharedCommand? ResolveShortcutCommand(Shortcut shortcut)
    {
        if (shortcut.CommandRef is not null)
        {
            return shortcut.CommandRef;
        }

        var name = FirstNonEmpty(shortcut.Command);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return ResolveCommand(name) ?? new SharedCommand(name);
    }

    protected static string NormalizeCommandName(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return string.Empty;
        }

        return command!
            .Trim()
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
    }

    private static CommandResult ToCommandResult(string? command, bool handled)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return CommandResult.NoEffect;
        }

        if (!Shortcut.IsAvailableCommandPublic(command))
        {
            return CommandResult.NotAvailable;
        }

        return handled ? CommandResult.Handled : CommandResult.NoEffect;
    }

    private static CommandResult ToCommandResult(SharedCommand? command, bool handled)
    {
        if (command is null)
        {
            return CommandResult.NoEffect;
        }

        return ToCommandResult(ResolveCommandName(command), handled);
    }

    private static DispatchResult ToDispatchResult(CommandResult result) => result switch
    {
        CommandResult.Handled => DispatchResult.Handled,
        CommandResult.NotAvailable => DispatchResult.NotAvailable,
        CommandResult.Ignored => DispatchResult.Ignored,
        CommandResult.NotFound => DispatchResult.NotFound,
        _ => DispatchResult.NoEffect,
    };

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value!.Trim();
            }
        }

        return string.Empty;
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value!.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }

    private static void NormalizeShortcut(Shortcut shortcut)
    {
        if (shortcut.CommandRef is not null)
        {
            if (string.IsNullOrWhiteSpace(shortcut.Command))
            {
                shortcut.Command = FirstNonEmpty(shortcut.CommandRef.Command, shortcut.CommandRef.Name);
            }
            if (string.IsNullOrWhiteSpace(shortcut.Argument))
            {
                shortcut.Argument = shortcut.CommandRef.Argument;
            }
        }

        var availableName = FirstNonEmpty(
            shortcut.CommandRef?.Command,
            shortcut.CommandRef?.Name,
            shortcut.Command);
        shortcut.IsAvailable = Shortcut.IsAvailableCommandPublic(availableName) || shortcut.IsAvailable;
    }
}

/// <summary>
/// Registro de atajos (ENT-007). Mantiene la lista de bindings y permite
/// consulta/despacho. Sin acciones conectadas en F-001.
/// </summary>
public class ShortcutRegistry : System.Collections.Generic.IEnumerable<Shortcut>
{
    private readonly List<Shortcut> _shortcuts = new();
    private readonly Dictionary<string, Shortcut> _map = new(StringComparer.OrdinalIgnoreCase);
    private CommandDispatcher? _dispatcher;

    public ShortcutRegistry()
        : this((CommandDispatcher?)null)
    {
    }

    public ShortcutRegistry(CommandDispatcher? dispatcher)
    {
        _dispatcher = dispatcher;
    }

    internal void RegisterShortcutInternal(Shortcut shortcut)
    {
        var key = FirstNonEmptyStatic(shortcut.Combination);
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!_map.ContainsKey(key))
        {
            _shortcuts.Add(shortcut);
        }
        _map[key] = shortcut;
    }

    internal void RemoveShortcutInternal(Shortcut shortcut)
    {
        _shortcuts.Remove(shortcut);
        var key = FirstNonEmptyStatic(shortcut.Combination);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _map.Remove(key);
        }
    }

    public CommandDispatcher? Dispatcher
    {
        get => _dispatcher;
        set => _dispatcher = value;
    }

    public IReadOnlyList<Shortcut> Shortcuts => _shortcuts.AsReadOnly();
    public int Count => _shortcuts.Count;

    public IEnumerator<Shortcut> GetEnumerator() => _shortcuts.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _shortcuts.GetEnumerator();

    public bool Register(Shortcut? shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        var key = FirstNonEmptyStatic(shortcut.Combination);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (!_map.ContainsKey(key))
        {
            _shortcuts.Add(shortcut);
        }
        _map[key] = shortcut;
        _dispatcher?.RegisterShortcut(shortcut);
        return true;
    }

    public bool Register(string? combination, string? command)
    {
        return Register(new Shortcut(combination ?? string.Empty, command ?? string.Empty));
    }

    public bool Register(string? combination, SharedCommand? command)
    {
        return Register(new Shortcut(combination ?? string.Empty, command));
    }

    public Shortcut? Find(string? combination)
    {
        if (string.IsNullOrWhiteSpace(combination))
        {
            return null;
        }

        if (_map.TryGetValue(combination!, out var shortcut))
        {
            return shortcut;
        }

        return _shortcuts.FirstOrDefault(x =>
            string.Equals(
                NormalizeKeyStatic(x.Combination),
                NormalizeKeyStatic(combination),
                StringComparison.Ordinal));
    }

    public bool TryGet(string? combination, out Shortcut? shortcut)
    {
        shortcut = Find(combination);
        return shortcut is not null;
    }

    public bool Contains(string? combination) => Find(combination) is not null;

    public bool Remove(Shortcut? shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        _shortcuts.Remove(shortcut);
        var key = FirstNonEmptyStatic(shortcut.Combination);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _map.Remove(key);
        }
        _dispatcher?.RemoveShortcut(key);
        return true;
    }

    public bool Remove(string? combination)
    {
        var shortcut = Find(combination);
        return shortcut is not null && Remove(shortcut);
    }

    public void Clear()
    {
        _shortcuts.Clear();
        _map.Clear();
    }

    /// <summary>
    /// OP-005: ejecuta el atajo a través del dispatcher compartido solo si su
    /// comando está disponible en F-001 (BR-F001-003). Sin dispatcher, o con
    /// un atajo cuyo comando pertenece a otra Feature, no hay efecto.
    /// </summary>
    public bool Invoke(string? combination) => Invoke(Find(combination));

    public bool Invoke(Shortcut? shortcut)
    {
        if (shortcut is null || !shortcut.Active || !shortcut.Enabled)
        {
            return false;
        }

        return _dispatcher?.DispatchShortcut(shortcut) ?? false;
    }

    private static string FirstNonEmptyStatic(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value!.Trim();
            }
        }

        return string.Empty;
    }

    private static string NormalizeKeyStatic(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value!.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }
}
