using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T043SyncSmokeTests
{
    [Fact]
    public void EditLeft_UpdatesModelAndRight()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        left.SyncEdit("new content");
        Assert.Equal("new content", doc.Content);
        Assert.Equal("new content", right.RenderedContent);
    }

    [Fact]
    public void EditRight_UpdatesModelAndLeft()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        right.SyncEdit("right content");
        Assert.Equal("right content", doc.Content);
        Assert.Equal("right content", left.RenderedContent);
    }

    [Fact]
    public void SyncEdit_Static_NoInfiniteLoop()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        int leftRenders = 0, rightRenders = 0;
        left.RenderedEvent += (_, _) => leftRenders++;
        right.RenderedEvent += (_, _) => rightRenders++;
        Sync.SyncEdit(left, "x");
        Assert.True(leftRenders >= 1);
        Assert.True(rightRenders >= 1);
    }

    [Fact]
    public void ContentChanged_Event_Fires()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        bool fired = false;
        doc.ContentChanged += (_, _) => fired = true;
        left.SyncEdit("y");
        Assert.True(fired);
    }

    [Fact]
    public void OppositePanel_Renders()
    {
        var doc = new OpenDocument("old");
        var panels = new EditorPanels(doc);
        panels.SyncEdit(panels.Left, "z");
        Assert.Equal("z", panels.Right.RenderedContent);
        Assert.Equal("z", panels.Left.RenderedContent);
    }

    [Fact]
    public void Reentrancy_Guard()
    {
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        // Simulate nested sync during event
        int count = 0;
        doc.ContentChanged += (_, _) =>
        {
            // nested attempt should be ignored (returns false) not loop
            var r = Sync.SyncEdit(left, "nested");
            if (r) count++;
        };
        left.SyncEdit("first");
        // count may be 0 due to guard? Actually guard _syncing false during event firing? Content setter sets _isChangingContent but Sync._syncing false after SyncEdit's try? Wait SyncEdit sets _syncing true then doc.Content set triggers event while _syncing true, so nested SyncEdit returns false. Good.
        Assert.Equal(0, count);
    }
}
