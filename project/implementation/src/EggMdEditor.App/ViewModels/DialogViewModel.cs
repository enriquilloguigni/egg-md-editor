using System;
using System.ComponentModel;
using System.Windows.Input;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Motivo por el que se cierra la superficie de diálogo SC-002. En F-001 no
/// hay diálogos concretos conectados: la superficie existe para que las
/// Features que la usen (F-002…F-010) solo aporten su formulario y sus
/// textos.
/// </summary>
public enum DialogCloseReason
{
    /// <summary>Sin cierre: la superficie sigue abierta.</summary>
    None = 0,

    /// <summary>El usuario activó la acción primaria (aceptar).</summary>
    Primary = 1,

    /// <summary>El usuario activó la acción secundaria o cerró con Esc.</summary>
    Secondary = 2,

    /// <summary>El usuario confirmó que descarta los cambios sin guardar.</summary>
    Discarded = 3,
}

/// <summary>Datos del cierre de un diálogo de SC-002.</summary>
public sealed class DialogClosedEventArgs : EventArgs
{
    public DialogClosedEventArgs(DialogCloseReason reason)
    {
        Reason = reason;
    }

    /// <summary>Motivo del cierre (acción primaria, secundaria o descarte).</summary>
    public DialogCloseReason Reason { get; }
}

/// <summary>
/// Estado de la superficie de diálogo genérica SC-002 (T-014): título,
/// mensaje, formulario opcional, acciones primaria/secundaria, validación
/// inline y confirmación al cerrar con cambios sin guardar. La superficie es
/// reutilizable por los diálogos concretos de F-002…F-010; en F-001 no hay
/// ninguno conectado (screens.md, SC-002).
/// </summary>
public sealed class DialogViewModel : INotifyPropertyChanged
{
    public const string TitleKey = "Dialog.Title";
    public const string MessageKey = "Dialog.Message";
    public const string UnsavedChangesKey = "Dialog.UnsavedChanges";
    public const string DiscardKey = "Dialog.Discard";
    public const string KeepEditingKey = "Dialog.KeepEditing";
    public const string PrimaryKey = "Common.Ok";
    public const string SecondaryKey = "Common.Cancel";

    private readonly Func<II18nCatalog> _catalogProvider;
    private bool _isOpen;
    private string? _title;
    private string? _message;
    private string? _belowMessage;
    private string? _primaryText;
    private string? _secondaryText;
    private string? _validationMessage;
    private bool _hasUnsavedChanges;
    private bool _closeConfirmationVisible;
    private object? _formContent;
    private ICommand? _primaryCommand;
    private ICommand? _secondaryCommand;
    private ICommand? _discardCommand;
    private ICommand? _keepEditingCommand;

    public DialogViewModel()
        : this((Func<II18nCatalog>?)null)
    {
    }

    /// <summary>
    /// Crea la superficie cerrada. El catálogo se resuelve de forma diferida
    /// para que la conmutación de idioma en caliente relabelice los textos
    /// propios del diálogo (COM-002, BR-013).
    /// </summary>
    public DialogViewModel(Func<II18nCatalog>? catalogProvider)
    {
        _catalogProvider = catalogProvider ?? (() => I18nCatalog.Instance);
    }

    private II18nCatalog Catalog => _catalogProvider();

    /// <summary>Superficie visible (ST correspondiente a diálogo abierto).</summary>
    public bool IsOpen => _isOpen;

    /// <summary>Título del diálogo, resuelto en el idioma activo.</summary>
    public string Title => _title ?? Catalog.Get(TitleKey);

    /// <summary>Mensaje principal del diálogo, resuelto en el idioma activo.</summary>
    public string Message => _message ?? Catalog.Get(MessageKey);

    /// <summary>
    /// Texto complementario opcional (p. ej. el del formulario concreto).
    /// No se muestra si está vacío.
    /// </summary>
    public string? BelowMessage => _belowMessage;

    public bool HasBelowMessage => !string.IsNullOrWhiteSpace(_belowMessage);

    /// <summary>
    /// Contenido del formulario concreto. En F-001 es nulo: ningún diálogo
    /// aporta campos (SC-002, Campos: ninguno activo en F-001).
    /// </summary>
    public object? FormContent => _formContent;

    /// <summary>Rótulo de la acción primaria, en el idioma activo.</summary>
    public string PrimaryText => _primaryText ?? Catalog.Get(PrimaryKey);

    /// <summary>Rótulo de la acción secundaria, en el idioma activo.</summary>
    public string SecondaryText => _secondaryText ?? Catalog.Get(SecondaryKey);

    /// <summary>Error inline asociado al campo del formulario, si lo hay.</summary>
    public string? ValidationMessage => _validationMessage;

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(_validationMessage);

    /// <summary>
    /// El formulario es válido mientras no haya error inline asociado al
    /// campo (SC-002, estados válido/inválido).
    /// </summary>
    public bool IsValid => !HasValidationMessage;

    /// <summary>
    /// Hay cambios sin guardar: cerrar por Esc o por la acción secundaria
    /// exige confirmación explícita (SC-002, estados).
    /// </summary>
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    /// <summary>Confirmación de descarte visible dentro de la superficie.</summary>
    public bool CloseConfirmationVisible => _closeConfirmationVisible;

    public string UnsavedChangesMessage => Catalog.Get(UnsavedChangesKey);

    public string DiscardText => Catalog.Get(DiscardKey);

    public string KeepEditingText => Catalog.Get(KeepEditingKey);

    /// <summary>
    /// Apertura de la superficie con los textos del diálogo concreto. Los
    /// valores nulos dejan los textos genéricos del catálogo. Sin acción
    /// conectada en F-001: el llamador es una Feature posterior.
    /// </summary>
    public bool Open(
        string? title = null,
        string? message = null,
        object? formContent = null,
        string? primaryText = null,
        string? secondaryText = null,
        bool hasUnsavedChanges = false,
        string? belowMessage = null)
    {
        _title = title;
        _message = message;
        _belowMessage = belowMessage;
        _formContent = formContent;
        _primaryText = primaryText;
        _secondaryText = secondaryText;
        _validationMessage = null;
        _hasUnsavedChanges = hasUnsavedChanges;
        _closeConfirmationVisible = false;
        _isOpen = true;
        RaiseAll();
        return true;
    }

    /// <summary>
    /// Actualiza el error inline asociado al campo (validación inline).
    /// </summary>
    public void SetValidationMessage(string? message)
    {
        if (string.Equals(_validationMessage, message, StringComparison.Ordinal))
        {
            return;
        }

        _validationMessage = message;
        OnPropertyChanged(nameof(ValidationMessage));
        OnPropertyChanged(nameof(HasValidationMessage));
        OnPropertyChanged(nameof(IsValid));
    }

    /// <summary>
    /// Marca o desmarca la existencia de cambios sin guardar del formulario
    /// concreto, que activa la confirmación de descarte al cerrar.
    /// </summary>
    public void SetHasUnsavedChanges(bool hasUnsavedChanges)
    {
        if (_hasUnsavedChanges == hasUnsavedChanges)
        {
            return;
        }

        _hasUnsavedChanges = hasUnsavedChanges;
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }

    /// <summary>Acción primaria (aceptar): cierra la superficie.</summary>
    public bool InvokePrimary()
    {
        if (!_isOpen)
        {
            return false;
        }

        return Close(DialogCloseReason.Primary);
    }

    /// <summary>
    /// Acción secundaria/cierre con Esc. Si hay cambios sin guardar, no
    /// cierra: muestra la confirmación de descarte (SC-002).
    /// </summary>
    public bool RequestClose()
    {
        if (!_isOpen)
        {
            return false;
        }

        if (_closeConfirmationVisible)
        {
            // Esc sobre la confirmación la descarta y devuelve al formulario.
            return KeepEditing();
        }

        if (_hasUnsavedChanges)
        {
            _closeConfirmationVisible = true;
            OnPropertyChanged(nameof(CloseConfirmationVisible));
            return true;
        }

        return Close(DialogCloseReason.Secondary);
    }

    /// <summary>Descarta los cambios y cierra la superficie.</summary>
    public bool DiscardChanges()
    {
        if (!_isOpen || !_closeConfirmationVisible)
        {
            return false;
        }

        return Close(DialogCloseReason.Discarded);
    }

    /// <summary>Cancela la confirmación y mantiene el formulario abierto.</summary>
    public bool KeepEditing()
    {
        if (!_closeConfirmationVisible)
        {
            return false;
        }

        _closeConfirmationVisible = false;
        OnPropertyChanged(nameof(CloseConfirmationVisible));
        return true;
    }

    /// <summary>Cierra la superficie forzando el motivo indicado.</summary>
    public bool Close(DialogCloseReason reason = DialogCloseReason.Secondary)
    {
        if (!_isOpen)
        {
            return false;
        }

        _isOpen = false;
        _closeConfirmationVisible = false;
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(CloseConfirmationVisible));
        Closed?.Invoke(this, new DialogClosedEventArgs(reason));
        return true;
    }

    /// <summary>
    /// Relabeliza los textos propios de la superficie al conmutar el idioma
    /// (COM-002). Los textos aportados por el diálogo concreto no se tocan.
    /// </summary>
    public void NotifyLanguageChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Message));
        OnPropertyChanged(nameof(PrimaryText));
        OnPropertyChanged(nameof(SecondaryText));
        OnPropertyChanged(nameof(UnsavedChangesMessage));
        OnPropertyChanged(nameof(DiscardText));
        OnPropertyChanged(nameof(KeepEditingText));
    }

    /// <summary>Acción primaria (aceptar).</summary>
    public ICommand PrimaryCommand =>
        _primaryCommand ??= new RelayCommand(() => InvokePrimary());

    /// <summary>Acción secundaria (cancelar/cerrar).</summary>
    public ICommand SecondaryCommand =>
        _secondaryCommand ??= new RelayCommand(() => RequestClose());

    /// <summary>Descarta los cambios sin guardar.</summary>
    public ICommand DiscardCommand =>
        _discardCommand ??= new RelayCommand(() => DiscardChanges());

    /// <summary>Mantiene el formulario abierto y cancela el descarte.</summary>
    public ICommand KeepEditingCommand =>
        _keepEditingCommand ??= new RelayCommand(() => KeepEditing());

    public event EventHandler<DialogClosedEventArgs>? Closed;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaiseAll()
    {
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Message));
        OnPropertyChanged(nameof(BelowMessage));
        OnPropertyChanged(nameof(HasBelowMessage));
        OnPropertyChanged(nameof(FormContent));
        OnPropertyChanged(nameof(PrimaryText));
        OnPropertyChanged(nameof(SecondaryText));
        OnPropertyChanged(nameof(ValidationMessage));
        OnPropertyChanged(nameof(HasValidationMessage));
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(HasUnsavedChanges));
        OnPropertyChanged(nameof(CloseConfirmationVisible));
        OnPropertyChanged(nameof(UnsavedChangesMessage));
        OnPropertyChanged(nameof(DiscardText));
        OnPropertyChanged(nameof(KeepEditingText));
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
