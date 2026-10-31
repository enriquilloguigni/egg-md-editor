using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

[Collection("SerialSync")]
public class T063OppositePanelSyncTests
{
    [Fact]
    public void TypeLeft_RightShowsSameContent()
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
    public void TypeRight_LeftShowsSameContent()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        bool ok = right.HandleKeyboardInput("right content");
        Assert.True(ok);
        Assert.Equal("right content", doc.Content);
        Assert.Equal("right content", left.RenderedContent);
        Assert.Equal("right content", right.RenderedContent);
    }
}
