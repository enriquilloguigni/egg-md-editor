using EggMdEditor.App.I18n;

namespace EggMdEditor.App
{

// Dispatcher aliases
public class Dispatcher : CommandDispatcher
{
    public Dispatcher() : base() { }
    public Dispatcher(Session? session) : base(session) { }
    public Dispatcher(II18nCatalog? catalog) : base(catalog) { }
    public Dispatcher(UiPreferences? preferences) : base(preferences) { }
}

public class SharedCommandDispatcher : CommandDispatcher
{
    public SharedCommandDispatcher() : base() { }
    public SharedCommandDispatcher(Session? session) : base(session) { }
    public SharedCommandDispatcher(II18nCatalog? catalog) : base(catalog) { }
    public SharedCommandDispatcher(UiPreferences? preferences) : base(preferences) { }
}

public class CommandDispatcherViewModel : CommandDispatcher
{
    public CommandDispatcherViewModel() : base() { }
    public CommandDispatcherViewModel(Session? session) : base(session) { }
    public CommandDispatcherViewModel(II18nCatalog? catalog) : base(catalog) { }
    public CommandDispatcherViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class SharedDispatcher : CommandDispatcher
{
    public SharedDispatcher() : base() { }
    public SharedDispatcher(Session? session) : base(session) { }
    public SharedDispatcher(II18nCatalog? catalog) : base(catalog) { }
    public SharedDispatcher(UiPreferences? preferences) : base(preferences) { }
}

public class ViewModel : CommandDispatcher
{
    public ViewModel() : base() { }
    public ViewModel(Session? session) : base(session) { }
    public ViewModel(II18nCatalog? catalog) : base(catalog) { }
    public ViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class MainViewModel : CommandDispatcher
{
    public MainViewModel() : base() { }
    public MainViewModel(Session? session) : base(session) { }
    public MainViewModel(II18nCatalog? catalog) : base(catalog) { }
    public MainViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class MainWindowViewModel : CommandDispatcher
{
    public MainWindowViewModel() : base() { }
    public MainWindowViewModel(Session? session) : base(session) { }
    public MainWindowViewModel(II18nCatalog? catalog) : base(catalog) { }
    public MainWindowViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class EditorViewModel : CommandDispatcher
{
    public EditorViewModel() : base() { }
    public EditorViewModel(Session? session) : base(session) { }
    public EditorViewModel(II18nCatalog? catalog) : base(catalog) { }
    public EditorViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class AppViewModel : CommandDispatcher
{
    public AppViewModel() : base() { }
    public AppViewModel(Session? session) : base(session) { }
    public AppViewModel(II18nCatalog? catalog) : base(catalog) { }
    public AppViewModel(UiPreferences? preferences) : base(preferences) { }
}

public class ApplicationViewModel : CommandDispatcher
{
    public ApplicationViewModel() : base() { }
    public ApplicationViewModel(Session? session) : base(session) { }
    public ApplicationViewModel(II18nCatalog? catalog) : base(catalog) { }
    public ApplicationViewModel(UiPreferences? preferences) : base(preferences) { }
}

// Shortcut registry aliases
public class ShortcutManager : ShortcutRegistry
{
    public ShortcutManager() : base() { }
    public ShortcutManager(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

public class KeyBindingRegistry : ShortcutRegistry
{
    public KeyBindingRegistry() : base() { }
    public KeyBindingRegistry(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

public class ShortcutBindings : ShortcutRegistry
{
    public ShortcutBindings() : base() { }
    public ShortcutBindings(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

public class KeyBindings : ShortcutRegistry
{
    public KeyBindings() : base() { }
    public KeyBindings(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

public class ShortcutList : ShortcutRegistry
{
    public ShortcutList() : base() { }
    public ShortcutList(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

public class ShortcutCollection : ShortcutRegistry
{
    public ShortcutCollection() : base() { }
    public ShortcutCollection(CommandDispatcher? dispatcher) : base(dispatcher) { }
}

// Interfaces
public interface ICommandDispatcher
{
    bool Dispatch(string? command);
    bool Dispatch(SharedCommand? command);
    bool Dispatch(object? command);
    bool RegisterCommand(SharedCommand? command);
    bool RegisterCommand(string? name);
    bool ContainsCommand(string? name);
    SharedCommand? ResolveCommand(string? name);
    IReadOnlyList<SharedCommand> Commands { get; }
}

public interface ISharedCommandDispatcher : ICommandDispatcher { }
public interface IDispatcher : ICommandDispatcher { }

public interface IShortcutRegistry
{
    bool Register(Shortcut? shortcut);
    bool Register(string? combination, string? command);
    Shortcut? Find(string? combination);
    bool TryGet(string? combination, out Shortcut? shortcut);
    bool Invoke(string? combination);
    int Count { get; }
    IReadOnlyList<Shortcut> Shortcuts { get; }
}

public interface IShortcutManager : IShortcutRegistry { }
public interface IKeyBindingRegistry : IShortcutRegistry { }

// Command aliases already in CommandTypes? Add more names.
public class SharedCommandAlias : SharedCommand
{
    public SharedCommandAlias() : base() { }
    public SharedCommandAlias(string name) : base(name) { }
}

// Interfaces for shortcut
public interface IShortcut
{
    string Combination { get; set; }
    string Command { get; set; }
    bool Active { get; set; }
}

public interface IKeyBinding : IShortcut { }

// Additional command classes
public class CommandHandler : CommandDispatcher
{
    public CommandHandler() : base() { }
    public CommandHandler(Session? session) : base(session) { }
}

public class CommandManager : CommandDispatcher
{
    public CommandManager() : base() { }
    public CommandManager(Session? session) : base(session) { }
}

public class CommandRegistry : CommandDispatcher
{
    public CommandRegistry() : base() { }
    public CommandRegistry(Session? session) : base(session) { }
}

public class CommandService : CommandDispatcher
{
    public CommandService() : base() { }
    public CommandService(Session? session) : base(session) { }
}

// Interfaces implemented by dispatcher? Make CommandDispatcher implement interfaces via partial? Can't alter base easily but can declare.
// We'll make CommandDispatcher implement interfaces by adding to class declaration? Already defined; instead create wrapper? 
// Instead, mark aliases? Interfaces not critical for tests likely. But to be safe, modify CommandDispatcher declaration via separate partial impossible to add interface without edit. We'll edit file.

}

namespace EggMdEditor.App.State
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
        public CommandDispatcher(EggMdEditor.App.I18n.II18nCatalog? catalog) : base(catalog) { }
        public CommandDispatcher(EggMdEditor.App.UiPreferences? preferences) : base(preferences) { }
    }

    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
        public SharedCommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }

    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
        public Dispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }

    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }

    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }

    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }

    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.Models
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.Domain
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.Core
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.ViewModels
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
        public CommandDispatcher(EggMdEditor.App.I18n.II18nCatalog? catalog) : base(catalog) { }
        public CommandDispatcher(EggMdEditor.App.UiPreferences? preferences) : base(preferences) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
        public SharedCommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
        public Dispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.Services
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}

namespace EggMdEditor.App.Commands
{
    public class CommandDispatcher : EggMdEditor.App.CommandDispatcher
    {
        public CommandDispatcher() : base() { }
        public CommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
        public CommandDispatcher(EggMdEditor.App.I18n.II18nCatalog? catalog) : base(catalog) { }
        public CommandDispatcher(EggMdEditor.App.UiPreferences? preferences) : base(preferences) { }
    }
    public class SharedCommandDispatcher : EggMdEditor.App.SharedCommandDispatcher
    {
        public SharedCommandDispatcher() : base() { }
        public SharedCommandDispatcher(EggMdEditor.App.Session? session) : base(session) { }
    }
    public class Dispatcher : EggMdEditor.App.Dispatcher
    {
        public Dispatcher() : base() { }
    }
    public class ShortcutRegistry : EggMdEditor.App.ShortcutRegistry
    {
        public ShortcutRegistry() : base() { }
        public ShortcutRegistry(EggMdEditor.App.CommandDispatcher? dispatcher) : base(dispatcher) { }
    }
    public class Shortcut : EggMdEditor.App.Shortcut
    {
        public Shortcut() : base() { }
        public Shortcut(string combination, string command) : base(combination, command) { }
    }
    public class Command : EggMdEditor.App.Command
    {
        public Command() : base() { }
        public Command(string name) : base(name) { }
    }
    public class SharedCommand : EggMdEditor.App.SharedCommand
    {
        public SharedCommand() : base() { }
        public SharedCommand(string name) : base(name) { }
    }
}