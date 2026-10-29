using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T041EditorPanelsTests
{
    [Fact]
    public void TwoPanels_ShareSameModel()
    {
        var doc = new OpenDocument("hola");
        var (left, right) = EditorPanel.CreatePair(doc);
        Assert.Same(doc, left.Document);
        Assert.Same(doc, right.Document);
        Assert.Same(left.Document, right.Document);
    }

    [Fact]
    public void LeftPanel_ModeIsWysiwyg()
    {
        var doc = new OpenDocument();
        var left = EditorPanel.CreateLeft(doc);
        Assert.True(left.IsWysiwyg);
        Assert.Equal(EditorMode.WYSIWYG, left.Mode);
    }

    [Fact]
    public void RightPanel_ModeIsMarkdown()
    {
        var doc = new OpenDocument();
        var right = EditorPanel.CreateRight(doc);
        Assert.True(right.IsMarkdown);
        Assert.Equal(EditorMode.Markdown, right.Mode);
    }

    [Fact]
    public void DefaultVisibility_IsTrue()
    {
        var panel = new EditorPanel();
        Assert.True(panel.Visible);
        Assert.True(panel.IsVisible);
    }

    [Fact]
    public void Vm_Panels_ShareDocument()
    {
        var vm = new MainWindowViewModel();
        var doc = new OpenDocument("x");
        vm.Document = doc;
        Assert.Same(doc, vm.LeftPanel.Document);
        Assert.Same(doc, vm.RightPanel.Document);
    }
}