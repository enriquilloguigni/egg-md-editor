using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

[Collection("SerialSync")]
public class T061KeyboardSyncTests
{
    [Fact]
    public void TypeLeft_UpdatesModelAndRight()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        bool ok = left.HandleKeyboardInput("new content");
        Assert.True(ok);
        Assert.Equal("new content", doc.Content);
        Assert.Equal("new content", right.RenderedContent);
        Assert.Equal("new content", left.RenderedContent);
    }

    [Fact]
    public void TypeRight_UpdatesModelAndLeft()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        bool ok = right.HandleKeyboardInput("right content");
        Assert.True(ok);
        Assert.Equal("right content", doc.Content);
        Assert.Equal("right content", left.RenderedContent);
        Assert.Equal("right content", right.RenderedContent);
    }

    [Fact]
    public void TypeLeft_ViaContainer_UpdatesRight()
    {
        var doc = new OpenDocument("old");
        var panels = new EditorPanels(doc);
        bool ok = panels.HandleKeyboardInput("container content");
        Assert.True(ok);
        Assert.Equal("container content", doc.Content);
        Assert.Equal("container content", panels.Right.RenderedContent);
    }

    [Fact]
    public void TypeRight_ViaContainer_UpdatesLeft()
    {
        var doc = new OpenDocument("old");
        var panels = new EditorPanels(doc);
        bool ok = panels.HandleRightKeyboardInput("right container");
        Assert.True(ok);
        Assert.Equal("right container", doc.Content);
        Assert.Equal("right container", panels.Left.RenderedContent);
    }

    [Fact]
    public void TypeChar_Left_Syncs()
    {
        var doc = new OpenDocument("");
        var (left, right) = EditorPanel.CreatePair(doc);
        bool ok = left.HandleKeyboardInput('a');
        Assert.True(ok);
        Assert.Equal("a", doc.Content);
        Assert.Equal("a", right.RenderedContent);
    }

    [Fact]
    public void Type_DoesNotInfiniteLoop()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        int leftRenders = 0, rightRenders = 0;
        left.RenderedEvent += (_, _) => leftRenders++;
        right.RenderedEvent += (_, _) => rightRenders++;
        left.HandleKeyboardInput("x");
        Assert.True(leftRenders >= 1);
        Assert.True(rightRenders >= 1);
    }
}
