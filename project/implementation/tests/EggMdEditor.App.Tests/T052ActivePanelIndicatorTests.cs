using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T052ActivePanelIndicatorTests
{
    [Fact]
    public void Default_ActivePanel_IsLeft()
    {
        var vm = new MainWindowViewModel();
        var indicator = new ActivePanelIndicator(vm);
        Assert.Same(vm.LeftPanel, indicator.ActivePanel);
        Assert.True(indicator.IsLeft);
        Assert.False(indicator.IsRight);
        Assert.Equal(ActivePanelIndicatorState.Left, indicator.State);
    }

    [Fact]
    public void Indicator_ReflectsActivePanel()
    {
        var vm = new MainWindowViewModel();
        vm.SetActivePanel(vm.RightPanel);
        var indicator = new ActivePanelIndicator(vm);
        Assert.Same(vm.RightPanel, indicator.ActivePanel);
        Assert.True(indicator.IsRight);
        Assert.False(indicator.IsLeft);
        Assert.Equal(ActivePanelIndicatorState.Right, indicator.State);
    }

    [Fact]
    public void ChangePanel_UpdatesIndicator()
    {
        var vm = new MainWindowViewModel();
        var indicator = vm.ActivePanelIndicator;
        Assert.Same(vm.LeftPanel, indicator.ActivePanel);
        vm.SetActivePanel(vm.RightPanel);
        Assert.Same(vm.RightPanel, indicator.ActivePanel);
        Assert.True(indicator.IsRight);
        vm.SetActivePanel(vm.LeftPanel);
        Assert.Same(vm.LeftPanel, indicator.ActivePanel);
        Assert.True(indicator.IsLeft);
    }

    [Fact]
    public void AliasClasses_Work()
    {
        var vm = new MainWindowViewModel();
        var i = new IndicadorPanelActivo(vm);
        Assert.Same(vm.LeftPanel, i.ActivePanel);
        vm.SetActivePanel(vm.RightPanel);
        Assert.Same(vm.RightPanel, i.ActivePanel);
    }

    [Fact]
    public void DirectPanels_ReflectActive()
    {
        var doc = new OpenDocument("hola");
        var (left, right) = EditorPanel.CreatePair(doc);
        var indicator = new ActivePanelIndicator(left, right, right);
        Assert.Same(right, indicator.ActivePanel);
        Assert.True(indicator.IsRight);
        indicator.ActivePanel = left;
        Assert.Same(left, indicator.ActivePanel);
        Assert.True(indicator.IsLeft);
    }

    [Fact]
    public void VisualProperties_Defaults()
    {
        var indicator = new ActivePanelIndicator();
        Assert.Equal(2.0, indicator.BorderThickness);
        Assert.True(indicator.FocusVisible);
        Assert.True(indicator.Visible);
    }
}