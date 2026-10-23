using System;
using System.Windows.Input;

namespace EggMdEditor.App;

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
