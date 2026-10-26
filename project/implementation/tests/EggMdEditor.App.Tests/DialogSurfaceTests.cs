using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using EggMdEditor.App.I18n;
using Xunit;
using AvaloniaThemeVariant = Avalonia.Styling.ThemeVariant;

namespace EggMdEditor.App.Tests;

// SC-002 (T-014): superficie de diálogo genérica sobre SC-001. El estado,
// las acciones primaria/secundaria, la validación inline y la confirmación
// de cierre con cambios sin guardar son genéricos; en F-001 no hay ningún
// diálogo concreto conectado (screens.md, SC-002).
public class DialogSurfaceTests
{
    [Fact]
    public void DialogStartsClosedAndResolvesItsGenericTextsInTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        var viewModel = new MainWindowViewModel(catalog);
        var dialog = viewModel.Dialog;

        Assert.False(dialog.IsOpen);
        Assert.False(dialog.HasValidationMessage);
        Assert.False(dialog.CloseConfirmationVisible);
        Assert.Equal("Diálogo", dialog.Title);
        Assert.Equal("Formulario o mensaje", dialog.Message);
        Assert.Equal("Aceptar", dialog.PrimaryText);
        Assert.Equal("Cancelar", dialog.SecondaryText);

        // BR-F001-005/COM-002: los textos propios de la superficie se
        // relabelizan en caliente.
        Assert.True(viewModel.TryChangeLanguage("en"));

        Assert.Equal("Dialog", dialog.Title);
        Assert.Equal("Form or message", dialog.Message);
        Assert.Equal("Accept", dialog.PrimaryText);
        Assert.Equal("Cancel", dialog.SecondaryText);
    }

    [Fact]
    public void PrimaryActionClosesTheDialogAndRaisesThePrimaryReason()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        DialogCloseReason? reason = null;
        dialog.Closed += (_, args) => reason = args.Reason;

        Assert.True(dialog.Open(
            title: "Propiedades",
            message: "Describe el documento.",
            primaryText: "Guardar",
            secondaryText: "Cancelar"));
        Assert.True(dialog.IsOpen);
        Assert.Equal("Propiedades", dialog.Title);
        Assert.Equal("Describe el documento.", dialog.Message);
        Assert.Equal("Guardar", dialog.PrimaryText);

        Assert.True(dialog.InvokePrimary());
        Assert.False(dialog.IsOpen);
        Assert.Equal(DialogCloseReason.Primary, reason);
    }

    [Fact]
    public void SecondaryCloseRequiresConfirmationWhenThereAreUnsavedChanges()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        DialogCloseReason? reason = null;
        dialog.Closed += (_, args) => reason = args.Reason;

        Assert.True(dialog.Open(hasUnsavedChanges: true));
        Assert.True(dialog.RequestClose());

        // SC-002: con cambios sin guardar no se cierra; se confirma.
        Assert.True(dialog.IsOpen);
        Assert.True(dialog.CloseConfirmationVisible);
        Assert.Equal("Hay cambios sin guardar. ¿Quieres descartarlos?", dialog.UnsavedChangesMessage);
        Assert.Null(reason);

        // Seguir editando cancela el descarte.
        Assert.True(dialog.KeepEditing());
        Assert.False(dialog.CloseConfirmationVisible);
        Assert.True(dialog.IsOpen);

        // Segundo intento: descartar cierra la superficie.
        Assert.True(dialog.RequestClose());
        Assert.True(dialog.DiscardChanges());
        Assert.False(dialog.IsOpen);
        Assert.False(dialog.CloseConfirmationVisible);
        Assert.Equal(DialogCloseReason.Discarded, reason);
    }

    [Fact]
    public void DialogWithoutUnsavedChangesClosesOnSecondaryRequest()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        DialogCloseReason? reason = null;
        dialog.Closed += (_, args) => reason = args.Reason;

        Assert.True(dialog.Open());
        Assert.True(dialog.RequestClose());

        Assert.False(dialog.IsOpen);
        Assert.Equal(DialogCloseReason.Secondary, reason);
    }

    [Fact]
    public void UnsavedChangesCanBeMarkedAfterTheDialogOpens()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());

        Assert.True(dialog.Open());
        Assert.False(dialog.HasUnsavedChanges);

        dialog.SetHasUnsavedChanges(true);
        Assert.True(dialog.HasUnsavedChanges);

        // La marca activa la confirmación de descarte al solicitar el cierre.
        Assert.True(dialog.RequestClose());
        Assert.True(dialog.IsOpen);
        Assert.True(dialog.CloseConfirmationVisible);
    }

    [AvaloniaFact]
    public void WindowShowsTheGenericDialogOverlayWithInitialFocusAndEscClose()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var surface = window.FindControl<DialogSurface>("DialogSurface");
            Assert.NotNull(surface);
            Assert.False(surface.IsVisible);
            Assert.False(viewModel.Dialog.IsOpen);

            viewModel.Dialog.Open();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var card = surface.FindControl<Border>("DialogCard");
            var overlay = surface.FindControl<Border>("DialogOverlay");
            var primary = surface.FindControl<Button>("PrimaryButton");
            var secondary = surface.FindControl<Button>("SecondaryButton");
            var validation = surface.FindControl<TextBlock>("DialogValidation");
            Assert.NotNull(card);
            Assert.NotNull(overlay);
            Assert.NotNull(primary);
            Assert.NotNull(secondary);
            Assert.NotNull(validation);

            // SC-002: overlay visible, acción primaria y secundaria, foco
            // inicial en la acción primaria.
            Assert.True(surface.IsVisible);
            Assert.True(card.IsVisible);
            Assert.Equal("Aceptar", primary.Content);
            Assert.Equal("Cancelar", secondary.Content);
            Assert.True(primary.IsFocused);

            // TK-001/TK-006: overlay y tarjeta con brushes resueltos por el
            // tema activo.
            Assert.NotNull(overlay.Background);
            Assert.NotNull(card.Background);
            Assert.False(window.FindControl<Grid>("MainContent")!.IsEnabled);

            // Validación inline: oculta sin error y visible con el mensaje
            // asociado al formulario.
            Assert.False(validation.IsVisible);
            Assert.True(viewModel.Dialog.IsValid);
            viewModel.Dialog.SetValidationMessage("El campo es obligatorio");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(validation.IsVisible);
            Assert.Equal("El campo es obligatorio", validation.Text);
            Assert.False(viewModel.Dialog.IsValid);

            // SC-002: Esc cierra la superficie.
            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.False(viewModel.Dialog.IsOpen);
            Assert.False(surface.IsVisible);
            Assert.True(window.FindControl<Grid>("MainContent")!.IsEnabled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WindowShowsTheUnsavedChangesConfirmationAndEscKeepsTheDialogOpen()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            viewModel.Dialog.Open(hasUnsavedChanges: true);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var surface = Assert.IsType<DialogSurface>(window.FindControl<DialogSurface>("DialogSurface"));
            var confirmation = surface.FindControl<Border>("UnsavedChangesPanel");
            Assert.NotNull(confirmation);
            Assert.False(confirmation.IsVisible);

            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            // SC-002: con cambios sin guardar, Esc pide confirmación y no
            // cierra; un segundo Esc cancela la confirmación.
            Assert.True(viewModel.Dialog.IsOpen);
            Assert.True(confirmation.IsVisible);

            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.True(viewModel.Dialog.IsOpen);
            Assert.False(confirmation.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void OpenDialogCanHostTheConcreteFormContent()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        var field = new TextBox { Text = "valor" };

        Assert.True(dialog.Open(formContent: field));

        Assert.Same(field, dialog.FormContent);
    }

    [AvaloniaFact]
    public void DialogSurfaceFollowsTheActiveTheme()
    {
        var application = Assert.IsType<App>(Application.Current);
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            viewModel.Dialog.Open();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var surface = Assert.IsType<DialogSurface>(window.FindControl<DialogSurface>("DialogSurface"));
            var card = Assert.IsType<Border>(surface.FindControl<Border>("DialogCard"));
            var light = Assert.IsType<SolidColorBrush>(card.Background).Color;

            // OP-002/COM-003: la superficie repinta con el tema de la
            // aplicación (TK-001, TK-006).
            application.RequestedThemeVariant = AvaloniaThemeVariant.Dark;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var dark = Assert.IsType<SolidColorBrush>(card.Background).Color;
            Assert.NotEqual(light, dark);
        }
        finally
        {
            application.RequestedThemeVariant = AvaloniaThemeVariant.Default;
            window.Close();
        }
    }
}
