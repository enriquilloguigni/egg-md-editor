using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using EggMdEditor.App;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T035DialogsSc002LocalTests
{
    [Fact]
    public void FileSelectorDialog_OpensWithFormContentAndInlineValidation()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        var field = new TextBox { Text = "nombre.md" };

        Assert.True(dialog.Open(
            title: "Abrir",
            message: "Selecciona un archivo Markdown.",
            formContent: field,
            primaryText: "Abrir",
            secondaryText: "Cancelar"));

        Assert.True(dialog.IsOpen);
        Assert.Same(field, dialog.FormContent);
        Assert.True(dialog.IsValid);

        dialog.SetValidationMessage("Escribe un nombre válido con extensión .md.");
        Assert.False(dialog.IsValid);
        Assert.True(dialog.HasValidationMessage);
        Assert.Equal("Escribe un nombre válido con extensión .md.", dialog.ValidationMessage);

        dialog.SetValidationMessage(null);
        Assert.True(dialog.IsValid);
    }

    [Fact]
    public void ConfirmationDialog_DefaultsToSaveAndRequiresExplicitDiscard()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        DialogCloseReason? reason = null;
        dialog.Closed += (_, args) => reason = args.Reason;

        Assert.True(dialog.Open(
            title: "Confirmar",
            message: "El documento tiene cambios sin guardar.",
            primaryText: "Guardar",
            secondaryText: "Cancelar",
            hasUnsavedChanges: true));

        Assert.True(dialog.HasUnsavedChanges);
        Assert.False(dialog.CloseConfirmationVisible);

        // Esc/secondary pide confirmación, no cierra.
        Assert.True(dialog.RequestClose());
        Assert.True(dialog.IsOpen);
        Assert.True(dialog.CloseConfirmationVisible);
        Assert.Null(reason);

        // Keep editing cancels.
        Assert.True(dialog.KeepEditing());
        Assert.False(dialog.CloseConfirmationVisible);

        // Discard closes with Discarded.
        Assert.True(dialog.RequestClose());
        Assert.True(dialog.DiscardChanges());
        Assert.False(dialog.IsOpen);
        Assert.Equal(DialogCloseReason.Discarded, reason);
    }

    [Fact]
    public void ErrorDialog_OpensWithMessageAndAcceptAction()
    {
        var dialog = new DialogViewModel(() => new I18nCatalog());
        DialogCloseReason? reason = null;
        dialog.Closed += (_, args) => reason = args.Reason;

        Assert.True(dialog.Open(
            title: "Error",
            message: "No se pudo abrir «x.md»: el archivo no existe.",
            primaryText: "Aceptar",
            secondaryText: "Cancelar"));

        Assert.True(dialog.IsOpen);
        Assert.Equal("Error", dialog.Title);
        Assert.Contains("No se pudo abrir", dialog.Message);

        Assert.True(dialog.InvokePrimary());
        Assert.False(dialog.IsOpen);
        Assert.Equal(DialogCloseReason.Primary, reason);
    }

    [AvaloniaFact]
    public void DialogSurface_FocusesPrimaryAndSupportsInlineValidation()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            viewModel.Dialog.Open(
                title: "Abrir",
                message: "Selecciona un archivo.",
                primaryText: "Abrir",
                secondaryText: "Cancelar");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var surface = window.FindControl<DialogSurface>("DialogSurface");
            Assert.NotNull(surface);
            var primary = surface.FindControl<Button>("PrimaryButton");
            var validation = surface.FindControl<TextBlock>("DialogValidation");
            Assert.NotNull(primary);
            Assert.NotNull(validation);

            Assert.True(primary.IsFocused);
            Assert.False(validation.IsVisible);

            viewModel.Dialog.SetValidationMessage("Campo obligatorio");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(validation.IsVisible);
            Assert.Equal("Campo obligatorio", validation.Text);

            window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.False(viewModel.Dialog.IsOpen);
        }
        finally
        {
            window.Close();
        }
    }
}
