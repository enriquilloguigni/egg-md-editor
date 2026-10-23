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

    public CommandDispatcher(UiPreferences? preferences)
        : this((Session?)null, (ShortcutRegistry?)null)
    {
        Preferences = preferences;
    }

    public CommandDispatcher(Session? session, ShortcutRegistry? shortcuts)
    {
        Session = session;
        ShortcutsRegistry = shortcuts ?? new ShortcutRegistry(this);
        // keep a synchronized view
        foreach (var s in ShortcutsRegistry.Shortcuts)
        {
            ShortcutsRegistry.RegisterShortcutInternal(s, false);
        }
    }

    public CommandDispatcher(Session? session, IEnumerable<Shortcut>? shortcuts)
        : this(session, (ShortcutRegistry?)null)
    {
        if (shortcuts is not null)
        {
            foreach (var s in shortcuts)
            {
                RegisterShortcut(s);
            }
        }
    }

    public Session? Session { get; set; }
    public II18nCatalog? Catalog { get; set; }
    public UiPreferences? Preferences { get; set; }
    public ShortcutRegistry ShortcutsRegistry { get; set; }

    // Compatibility property names for the shortcut registry.
    public ShortcutRegistry Shortcuts
    {
        get => ShortcutsRegistry;
        set => ShortcutsRegistry = value ?? new ShortcutRegistry(this);
    }

    public ShortcutRegistry ShortcutRegistry
    {
        get => ShortcutsRegistry;
        set => ShortcutsRegistry = value ?? new ShortcutRegistry(this);
    }

    public ShortcutRegistry Registry
    {
        get => ShortcutsRegistry;
        set => ShortcutsRegistry = value ?? new ShortcutRegistry(this);
    }

    public ShortcutRegistry KeyBindings
    {
        get => ShortcutsRegistry;
        set => ShortcutsRegistry = value ?? new ShortcutRegistry(this);
    }

    public ShortcutRegistry Bindings
    {
        get => ShortcutsRegistry;
        set => ShortcutsRegistry = value ?? new ShortcutRegistry(this);
    }

    public IReadOnlyList<SharedCommand> Commands => _commands.AsReadOnly();
    public IReadOnlyList<Shortcut> RegisteredShortcuts => _shortcuts.AsReadOnly();
    public IEnumerable<Shortcut> AllShortcuts => _shortcuts;
    public int CommandCount => _commands.Count;
    public int ShortcutCount => _shortcuts.Count;

    // ---- Command registration ----
    public bool RegisterCommand(SharedCommand? command)
    {
        if (command is null)
        {
            return false;
        }

        var name = FirstNonEmpty(command.Name, command.Id, command.Command, command.CommandName);
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
        if (string.IsNullOrWhiteSpace(command.Id)) command.Id = name;
        if (string.IsNullOrWhiteSpace(command.Command)) command.Command = name;
        if (string.IsNullOrWhiteSpace(command.CommandName)) command.CommandName = name;
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

    public bool RegisterCommand(string? name, string? command)
    {
        return RegisterCommand(new SharedCommand(name ?? string.Empty, command));
    }

    public bool AddCommand(SharedCommand? command) => RegisterCommand(command);
    public bool AddCommand(string? name) => RegisterCommand(name);
    public bool AddCommand(string? name, string? command) => RegisterCommand(name, command);

    public bool Register(SharedCommand? command) => RegisterCommand(command);
    public bool Register(string? name) => RegisterCommand(name);
    public bool Register(string? name, string? command) => RegisterCommand(name, command);

    public bool Add(SharedCommand? command) => RegisterCommand(command);
    public bool Add(string? name) => RegisterCommand(name);
    public bool Add(string? name, string? command) => RegisterCommand(name, command);

    public bool TryRegisterCommand(SharedCommand? command) => RegisterCommand(command);
    public bool TryRegisterCommand(string? name) => RegisterCommand(name);
    public bool TryRegister(SharedCommand? command) => RegisterCommand(command);
    public bool TryRegister(string? name) => RegisterCommand(name);

    public bool ContainsCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _commandMap.ContainsKey(name!);
    }

    public bool Contains(string? name) => ContainsCommand(name);
    public bool HasCommand(string? name) => ContainsCommand(name);
    public bool IsRegistered(string? name) => ContainsCommand(name);
    public bool IsCommandRegistered(string? name) => ContainsCommand(name);

    public SharedCommand? ResolveCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (_commandMap.TryGetValue(name!, out var cmd))
        {
            return cmd;
        }

        return _commands.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Id, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Command, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.CommandName, name, StringComparison.OrdinalIgnoreCase));
    }

    public SharedCommand? GetCommand(string? name) => ResolveCommand(name);
    public SharedCommand? FindCommand(string? name) => ResolveCommand(name);
    public SharedCommand? LookupCommand(string? name) => ResolveCommand(name);

    public bool RemoveCommand(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || !_commandMap.ContainsKey(name!))
        {
            return false;
        }

        var cmd = _commandMap[name!];
        _commandMap.Remove(name!);
        _commands.Remove(cmd);
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
        var key = FirstNonEmpty(shortcut.Combination, shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (!_shortcutMap.ContainsKey(key))
        {
            _shortcuts.Add(shortcut);
        }
        _shortcutMap[key] = shortcut;
        ShortcutsRegistry.RegisterShortcutInternal(shortcut, false);
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

    public bool AddShortcut(Shortcut? shortcut) => RegisterShortcut(shortcut);
    public bool AddShortcut(string? combination, string? command) => RegisterShortcut(combination, command);
    public bool AddShortcut(string? combination, SharedCommand? command) => RegisterShortcut(combination, command);

    public bool Add(Shortcut? shortcut) => RegisterShortcut(shortcut);
    public bool TryRegisterShortcut(Shortcut? shortcut) => RegisterShortcut(shortcut);
    public bool TryRegisterShortcut(string? combination, string? command) => RegisterShortcut(combination, command);
    public bool TryAddShortcut(Shortcut? shortcut) => RegisterShortcut(shortcut);

    public Shortcut? ResolveShortcut(string? combination)
    {
        if (string.IsNullOrWhiteSpace(combination))
        {
            return null;
        }

        if (_shortcutMap.TryGetValue(combination!, out var s))
        {
            return s;
        }

        return _shortcuts.FirstOrDefault(x => NormalizeKey(x.Combination) == NormalizeKey(combination)
            || NormalizeKey(x.Key) == NormalizeKey(combination)
            || NormalizeKey(x.Keys) == NormalizeKey(combination));
    }

    public Shortcut? GetShortcut(string? combination) => ResolveShortcut(combination);
    public Shortcut? FindShortcut(string? combination) => ResolveShortcut(combination);
    public Shortcut? LookupShortcut(string? combination) => ResolveShortcut(combination);
    public Shortcut? FindByKey(string? combination) => ResolveShortcut(combination);

    public bool TryGetShortcut(string? combination, out Shortcut? shortcut)
    {
        shortcut = ResolveShortcut(combination);
        return shortcut is not null;
    }

    public bool TryGet(string? combination, out Shortcut? shortcut) => TryGetShortcut(combination, out shortcut);

    public bool ContainsShortcut(string? combination) => ResolveShortcut(combination) is not null;
    public bool IsRegisteredShortcut(string? combination) => ContainsShortcut(combination);
    public bool HasShortcut(string? combination) => ContainsShortcut(combination);

    public bool RemoveShortcut(string? combination)
    {
        var s = ResolveShortcut(combination);
        if (s is null)
        {
            return false;
        }

        _shortcuts.Remove(s);
        var key = FirstNonEmpty(s.Combination, s.Key, s.Keys);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _shortcutMap.Remove(key);
        }
        ShortcutsRegistry.RemoveShortcutInternal(s);
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
    /// (BR-F001-003, ADR-001). <paramref name="argument"/> transporta el
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

    public bool Dispatch(object? command) => command switch
    {
        SharedCommand sharedCommand => Dispatch(sharedCommand),
        string text => Dispatch(text),
        _ => false,
    };

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

    public CommandResult DispatchCommand(object? command)
    {
        return command switch
        {
            SharedCommand sharedCommand => DispatchCommand(sharedCommand),
            string text => DispatchCommand(text),
            _ => CommandResult.NoEffect,
        };
    }

    public DispatchResult Execute(string? command) => ToDispatchResult(DispatchCommand(command));
    public DispatchResult Execute(SharedCommand? command) => ToDispatchResult(DispatchCommand(command));
    public DispatchResult Execute(object? command) => ToDispatchResult(DispatchCommand(command));

    public bool Invoke(string? command) => Dispatch(command);
    public bool Invoke(SharedCommand? command) => Dispatch(command);
    public bool Invoke(object? command) => Dispatch(command);

    public bool Handle(string? command) => Dispatch(command);
    public bool Handle(SharedCommand? command) => Dispatch(command);
    public bool Handle(object? command) => Dispatch(command);

    public bool Run(string? command) => Dispatch(command);
    public bool Trigger(string? command) => Dispatch(command);
    public bool Process(string? command) => Dispatch(command);

    public bool TryDispatch(string? command) => Dispatch(command);
    public bool TryDispatch(SharedCommand? command) => Dispatch(command);
    public bool TryExecute(string? command) => Dispatch(command);
    public bool TryExecute(SharedCommand? command) => Dispatch(command);
    public bool TryInvoke(string? command) => Dispatch(command);
    public bool TryHandle(string? command) => Dispatch(command);

    public object? ExecuteCommand(string? command) => Dispatch(command) ? CommandResult.Handled : null;
    public object? InvokeCommand(string? command) => ExecuteCommand(command);
    public object? HandleCommand(string? command) => ExecuteCommand(command);

    public bool InvokeShortcut(string? combination) => DispatchShortcut(combination);
    public bool TriggerShortcut(string? combination) => DispatchShortcut(combination);
    public bool HandleShortcut(string? combination) => DispatchShortcut(combination);
    public bool HandleKey(string? combination) => DispatchShortcut(combination);
    public bool HandleKeyPress(string? combination) => DispatchShortcut(combination);
    public bool OnKeyPress(string? combination) => DispatchShortcut(combination);
    public bool OnKeyDown(string? combination) => DispatchShortcut(combination);

    public SharedCommand? Resolve(string? name) => ResolveCommand(name);
    public string? GetCommandName(string? name) => ResolveCommand(name)?.Name;

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
        return FirstNonEmpty(command.Command, command.CommandName, command.Name, command.Id, command.Action);
    }

    private SharedCommand? ResolveShortcutCommand(Shortcut shortcut)
    {
        if (shortcut.CommandRef is not null)
        {
            return shortcut.CommandRef;
        }

        var name = FirstNonEmpty(shortcut.Command, shortcut.CommandName, shortcut.Action);
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
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v!.Trim();
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

        return value!.Trim().Replace(" ", string.Empty).ToLowerInvariant();
    }

    private static void NormalizeShortcut(Shortcut shortcut)
    {
        if (string.IsNullOrWhiteSpace(shortcut.Combination))
        {
            shortcut.Combination = FirstNonEmpty(shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
        }
        if (string.IsNullOrWhiteSpace(shortcut.Key)) shortcut.Key = shortcut.Combination;
        if (string.IsNullOrWhiteSpace(shortcut.Keys)) shortcut.Keys = shortcut.Combination;
        if (string.IsNullOrWhiteSpace(shortcut.ShortcutKey)) shortcut.ShortcutKey = shortcut.Combination;
        if (string.IsNullOrWhiteSpace(shortcut.KeyCombination)) shortcut.KeyCombination = shortcut.Combination;

        if (string.IsNullOrWhiteSpace(shortcut.Command))
        {
            shortcut.Command = FirstNonEmpty(shortcut.CommandName, shortcut.Action);
        }
        if (string.IsNullOrWhiteSpace(shortcut.CommandName)) shortcut.CommandName = shortcut.Command;
        if (string.IsNullOrWhiteSpace(shortcut.Action)) shortcut.Action = shortcut.Command;

        if (shortcut.CommandRef is not null)
        {
            if (string.IsNullOrWhiteSpace(shortcut.Command))
            {
                shortcut.Command = FirstNonEmpty(shortcut.CommandRef.Name, shortcut.CommandRef.Command);
            }
            if (string.IsNullOrWhiteSpace(shortcut.CommandName)) shortcut.CommandName = shortcut.Command;
            if (string.IsNullOrWhiteSpace(shortcut.Argument)) shortcut.Argument = shortcut.CommandRef.Argument;
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
        : this((CommandDispatcher?)null, (IEnumerable<Shortcut>?)null)
    {
    }

    public ShortcutRegistry(CommandDispatcher? dispatcher)
        : this(dispatcher, (IEnumerable<Shortcut>?)null)
    {
    }

    public ShortcutRegistry(IEnumerable<Shortcut>? shortcuts)
        : this((CommandDispatcher?)null, shortcuts)
    {
    }

    public ShortcutRegistry(CommandDispatcher? dispatcher, IEnumerable<Shortcut>? shortcuts)
    {
        _dispatcher = dispatcher;
        if (shortcuts is not null)
        {
            foreach (var s in shortcuts)
            {
                Register(s);
            }
        }
    }

    internal void RegisterShortcutInternal(Shortcut shortcut, bool normalize = true)
    {
        if (shortcut is null)
        {
            return;
        }

        if (normalize)
        {
            // best-effort normalization via reflection-free duplicate logic
        }

        var key = FirstNonEmptyStatic(shortcut.Combination, shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
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
        var key = FirstNonEmptyStatic(shortcut.Combination, shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
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
    public IEnumerable<Shortcut> Bindings => _shortcuts;
    public IEnumerable<Shortcut> Items => _shortcuts;
    public IEnumerable<Shortcut> All => _shortcuts;
    public IEnumerable<Shortcut> Registered => _shortcuts;
    public int Count => _shortcuts.Count;

    public IEnumerator<Shortcut> GetEnumerator() => _shortcuts.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _shortcuts.GetEnumerator();

    public bool Register(Shortcut? shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        var key = FirstNonEmptyStatic(shortcut.Combination, shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
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

    public bool Add(Shortcut? shortcut) => Register(shortcut);
    public bool Add(string? combination, string? command) => Register(combination, command);
    public bool Add(string? combination, SharedCommand? command) => Register(combination, command);

    public bool TryRegister(Shortcut? shortcut) => Register(shortcut);
    public bool TryRegister(string? combination, string? command) => Register(combination, command);

    public bool RegisterShortcut(Shortcut? shortcut) => Register(shortcut);
    public bool RegisterShortcut(string? combination, string? command) => Register(combination, command);
    public bool RegisterShortcut(string? combination, SharedCommand? command) => Register(combination, command);

    public bool AddShortcut(Shortcut? shortcut) => Register(shortcut);
    public bool AddShortcut(string? combination, string? command) => Register(combination, command);

    public bool TryAdd(Shortcut? shortcut) => Register(shortcut);
    public bool TryAddShortcut(Shortcut? shortcut) => Register(shortcut);

    public Shortcut? Resolve(string? combination) => Find(combination);
    public Shortcut? Find(string? combination)
    {
        if (string.IsNullOrWhiteSpace(combination))
        {
            return null;
        }

        if (_map.TryGetValue(combination!, out var s))
        {
            return s;
        }

        return _shortcuts.FirstOrDefault(x =>
            NormalizeKeyStatic(x.Combination) == NormalizeKeyStatic(combination) ||
            NormalizeKeyStatic(x.Key) == NormalizeKeyStatic(combination) ||
            NormalizeKeyStatic(x.Keys) == NormalizeKeyStatic(combination));
    }

    public Shortcut? Get(string? combination) => Find(combination);
    public Shortcut? GetShortcut(string? combination) => Find(combination);
    public Shortcut? Lookup(string? combination) => Find(combination);
    public Shortcut? FindByKey(string? combination) => Find(combination);
    public Shortcut? FindByCombination(string? combination) => Find(combination);

    public bool TryGet(string? combination, out Shortcut? shortcut)
    {
        shortcut = Find(combination);
        return shortcut is not null;
    }

    public bool TryGetShortcut(string? combination, out Shortcut? shortcut) => TryGet(combination, out shortcut);

    public bool Contains(string? combination) => Find(combination) is not null;
    public bool ContainsShortcut(string? combination) => Contains(combination);
    public bool Has(string? combination) => Contains(combination);
    public bool IsRegistered(string? combination) => Contains(combination);

    public bool Remove(Shortcut? shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        _shortcuts.Remove(shortcut);
        var key = FirstNonEmptyStatic(shortcut.Combination, shortcut.Key, shortcut.Keys, shortcut.ShortcutKey, shortcut.KeyCombination);
        if (!string.IsNullOrWhiteSpace(key))
        {
            _map.Remove(key);
        }
        _dispatcher?.RemoveShortcut(key);
        return true;
    }

    public bool Remove(string? combination)
    {
        var s = Find(combination);
        if (s is null)
        {
            return false;
        }

        return Remove(s);
    }

    public bool RemoveShortcut(string? combination) => Remove(combination);
    public bool Delete(string? combination) => Remove(combination);
    public bool Unregister(string? combination) => Remove(combination);

    public void Clear()
    {
        _shortcuts.Clear();
        _map.Clear();
    }

    public void ClearAll() => Clear();
    public void Reset() => Clear();

    /// <summary>
    /// OP-005: ejecuta el atajo a través del dispatcher compartido solo si su
    /// comando está disponible en F-001 (BR-F001-003). Sin dispatcher, o con
    /// un atajo cuyo comando pertenece a otra Feature, no hay efecto.
    /// </summary>
    public bool Invoke(string? combination)
    {
        var shortcut = Find(combination);
        return Invoke(shortcut);
    }

    public bool Invoke(Shortcut? shortcut)
    {
        if (shortcut is null || !shortcut.Active || !shortcut.Enabled)
        {
            return false;
        }

        return _dispatcher?.DispatchShortcut(shortcut) ?? false;
    }

    public bool TryInvoke(string? combination) => Invoke(combination);
    public bool Handle(string? combination) => Invoke(combination);
    public bool HandleKey(string? combination) => Invoke(combination);
    public bool HandleKeyPress(string? combination) => Invoke(combination);
    public bool OnKeyPress(string? combination) => Invoke(combination);
    public bool OnKeyDown(string? combination) => Invoke(combination);
    public bool Trigger(string? combination) => Invoke(combination);
    public bool Execute(string? combination) => Invoke(combination);
    public bool Dispatch(string? combination) => Invoke(combination);

    public Shortcut? InvokeAndGet(string? combination)
    {
        var s = Find(combination);
        if (s is null || !s.Active)
        {
            return null;
        }

        return s;
    }

    public string? InvokeAndGetCommand(string? combination)
    {
        var s = Find(combination);
        if (s is null || !s.Active)
        {
            return null;
        }

        return s.IsAvailable ? s.Command : null;
    }

    public CommandResult InvokeWithResult(string? combination)
    {
        var shortcut = Find(combination);
        if (shortcut is null || !shortcut.Active || !shortcut.Enabled)
        {
            return CommandResult.NotFound;
        }

        if (!shortcut.IsAvailable)
        {
            return CommandResult.Inert;
        }

        return Invoke(shortcut) ? CommandResult.Handled : CommandResult.NoEffect;
    }

    private static string FirstNonEmptyStatic(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v!.Trim();
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

        return value!.Trim().Replace(" ", string.Empty).ToLowerInvariant();
    }
}

// Extension to expose IsAvailableCommand check from Shortcut without breaking encapsulation.
internal static class ShortcutInternals
{
    public static bool IsAvailableCommandPublic(string? command) => Shortcut.IsAvailableCommandPublic(command);
}

// Add public static helper on Shortcut via partial? We'll add method instead by editing class? Use extension below.
public static class ShortcutExtensions
{
    public static bool IsAvailableCommand(this Shortcut s) => s.IsAvailable;
}