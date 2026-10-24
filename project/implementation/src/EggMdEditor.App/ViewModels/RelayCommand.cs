using System;
using System.Windows.Input;

namespace EggMdEditor.App;

/// <summary>
/// Comando síncrono sin parámetro para MVVM (COM-001). Se usa en acciones de
/// superficie que no transportan argumento (p. ej. las de SC-002).
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Comando síncrono mínimo para MVVM (COM-001). El parámetro debe ser del
/// tipo declarado; un parámetro ausente o de otro tipo no ejecuta la acción.
/// </summary>
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) =>
        TryGetValue(parameter, out var value) && (_canExecute?.Invoke(value) ?? true);

    public void Execute(object? parameter)
    {
        if (TryGetValue(parameter, out var value))
        {
            _execute(value);
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private static bool TryGetValue(object? parameter, out T? value)
    {
        if (parameter is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }
}
