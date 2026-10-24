using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace EggMdEditor.App;

/// <summary>
/// Ventana de ayuda SC-003 (T-014): carcasa delgada de
/// <see cref="HelpWindowViewModel"/> con cierre por Esc, navegación por
/// secciones y copia del texto de la sección activa. Sin contenido de
/// dominio conectado.
/// </summary>
public partial class HelpWindow : Window
{
    private readonly HelpWindowViewModel _viewModel;

    public HelpWindow()
        : this(new HelpWindowViewModel())
    {
    }

    public HelpWindow(HelpWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closed += OnClosed;
    }

    /// <summary>Estado y textos de la superficie, útil para pruebas y capturas.</summary>
    public HelpWindowViewModel ViewModel => _viewModel;

    /// <summary>Selecciona una de las secciones de SC-003.</summary>
    public void SelectSection(HelpSection section) => _viewModel.SelectSection(section);

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        HelpSectionList.Focus();
    }

    /// <summary>Cierre con Esc (screens.md, SC-003).</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            Close();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnCopyTextClick(object? sender, RoutedEventArgs e) => _ = CopyActiveSectionAsync();

    private async Task CopyActiveSectionAsync()
    {
        var content = _viewModel.Content;
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(content);
            }
        }
        catch (Exception)
        {
            // El portapapeles puede no estar disponible (plataforma headless):
            // la copia es una acción de superficie y no debe romper la ventana.
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Dispose();
    }

    /// <summary>
    /// BR-013: al relabelizar en caliente se reconstruyen las secciones; se
    /// restablece la selección para que la navegación visible conserve la
    /// sección activa (SC-003, estado «sección activa»).
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HelpWindowViewModel.Sections))
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(
            () =>
            {
                if (_viewModel.ActiveSection is { } active
                    && !ReferenceEquals(HelpSectionList.SelectedItem, active))
                {
                    HelpSectionList.SelectedItem = active;
                }
            },
            DispatcherPriority.Loaded);
    }
}
