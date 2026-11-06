using EggMdEditor.App;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T079SimTests
{
    [Fact]
    public void LinkDialog_OpensAndValidates()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        Assert.True(vm.ShowFormatDialog("link"));
        Assert.True(vm.Dialog.IsOpen);
        var form = (FormatDialogModel)vm.Dialog.FormContent!;
        Assert.Equal("link", form.FormatKey);
        form.Destination = "";
        Assert.False(vm.ApplyFormatDialog());
        Assert.True(vm.Dialog.HasValidationMessage);
        form.Destination = "https://example.com";
        Assert.True(vm.ApplyFormatDialog());
        Assert.False(vm.Dialog.IsOpen);
    }

    [Fact]
    public void TableDialog_RowsColumns()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        vm.ShowFormatDialog("table");
        var form = (TableDialogViewModel)vm.Dialog.FormContent!;
        form.Rows = 0;
        Assert.False(vm.ApplyFormatDialog());
        form.Rows = 2;
        form.Columns = 2;
        Assert.True(vm.ApplyFormatDialog());
    }

    [Fact]
    public void ImageDialog_AltRequired()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        vm.ShowFormatDialog("image");
        var form = vm.Dialog.FormContent as ImageDialog;
        Assert.NotNull(form);
        form.Alt = "";
        Assert.False(vm.ApplyFormatDialog());
        form.Src = "img.png";
        form.Alt = "alt";
        Assert.True(vm.ApplyFormatDialog());
    }

    [Fact]
    public void FormulaDialog_CodeRequired()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        vm.ShowFormatDialog("formula");
        var form = vm.Dialog.FormContent as FormulaDialogViewModel;
        Assert.NotNull(form);
        form.Code = "";
        Assert.False(vm.ApplyFormatDialog());
        form.Code = "x^2";
        Assert.True(vm.ApplyFormatDialog());
    }
}
