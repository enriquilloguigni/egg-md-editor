using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T049FormatBarVisibilityTests
{
    [Fact]
    public void Default_FormatBar_Visible()
    {
        var vm = new MainWindowViewModel();
        Assert.True(vm.FormatBarVisible);
        Assert.True(vm.IsFormatBarVisible);
        Assert.Equal(FormatBarVisibilityState.Visible, vm.FormatBarState);
    }

    [Fact]
    public void Toggle_FormatBar_ChangesVisibility()
    {
        var vm = new MainWindowViewModel();
        Assert.True(vm.ToggleFormatBar());
        Assert.False(vm.FormatBarVisible);
        Assert.True(vm.ToggleFormatBar());
        Assert.True(vm.FormatBarVisible);
    }

    [Fact]
    public void Hide_Show_FormatBar()
    {
        var vm = new MainWindowViewModel();
        Assert.True(vm.HideFormatBar());
        Assert.False(vm.FormatBarVisible);
        Assert.True(vm.ShowFormatBar());
        Assert.True(vm.FormatBarVisible);
    }

    [Fact]
    public void FormatBar_ActsOnActivePanel()
    {
        var vm = new MainWindowViewModel();
        vm.SetActivePanel(vm.RightPanel);
        Assert.Same(vm.RightPanel, vm.ActivePanel);
        Assert.Same(vm.RightPanel, vm.FormatBarTarget);
        Assert.Same(vm.RightPanel, vm.FormatTarget);
        Assert.True(vm.IsRightPanelActive);
    }

    [Fact]
    public void Toggle_DoesNotChangeDocument()
    {
        var vm = new MainWindowViewModel();
        var doc = new OpenDocument("hello");
        vm.Document = doc;
        vm.ToggleFormatBar();
        Assert.Same(doc, vm.Document);
        Assert.Equal("hello", vm.Document!.Content);
    }
}