using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T034DragHighlightLocalTests
{
    [Fact]
    public void IsDragOver_DefaultsFalse()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        Assert.False(vm.IsDragOver);
        Assert.False(vm.DragHighlightVisible);
    }

    [Fact]
    public void SetDragOver_TogglesHighlight()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        vm.SetDragOver(true);
        Assert.True(vm.IsDragOver);
        Assert.True(vm.DragHighlightVisible);
        vm.SetDragOver(false);
        Assert.False(vm.IsDragOver);
    }

    [Fact]
    public void ValidateDrop_SingleMdOnly()
    {
        Assert.True(AbrirOperation.ValidateDrop(new[] { "a.md" }));
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.md", "b.md" }));
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.txt" }));
    }

    [AvaloniaFact]
    public void Window_DragHighlight_ControlExists()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(vm);
        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var border = window.FindControl<Avalonia.Controls.Border>("DragHighlight");
            Assert.NotNull(border);
        }
        finally
        {
            window.Close();
        }
    }
}