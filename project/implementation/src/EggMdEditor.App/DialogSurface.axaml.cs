using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace EggMdEditor.App;

/// <summary>
/// Superficie de diálogo genérica SC-002 (T-014). Vista delgada: refleja el
/// estado de <see cref="DialogViewModel"/> y coloca el foco inicial en la
/// acción primaria al abrirse (screens.md, SC-002). No contiene lógica de
/// dominio ni diálogos concretos: en F-001 no hay ninguno conectado.
/// </summary>
public partial class DialogSurface : UserControl
{
    private DialogViewModel? _dialog;

    public DialogSurface()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>Acción primaria de la superficie (foco inicial).</summary>
    public Button PrimaryAction => PrimaryButton;

    /// <summary>Acción secundaria de la superficie.</summary>
    public Button SecondaryAction => SecondaryButton;

    /// <summary>
    /// Recibe el foco inicial al abrirse el diálogo; se repite al cambiar la
    /// visibilidad porque la vinculación puede resolverse después del estado.
    /// </summary>
    public void FocusPrimaryAction()
    {
        if (!IsVisible)
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(
            () =>
            {
                if (IsVisible)
                {
                    PrimaryButton.Focus();
                }
            },
            DispatcherPriority.Loaded);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty && change.GetNewValue<bool>())
        {
            FocusPrimaryAction();
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_dialog is not null)
        {
            _dialog.PropertyChanged -= OnDialogPropertyChanged;
        }

        _dialog = DataContext as DialogViewModel;
        if (_dialog is not null)
        {
            _dialog.PropertyChanged += OnDialogPropertyChanged;
        }
    }

    private void OnDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DialogViewModel.IsOpen) && _dialog?.IsOpen == true)
        {
            FocusPrimaryAction();
        }
    }
}
