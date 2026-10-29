using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T045PassengerHighlightTests
{
    [Fact]
    public void Apply_ActivatesHighlight()
    {
        var doc = new OpenDocument("hola");
        var (left, right) = EditorPanel.CreatePair(doc);
        var h = new PassengerHighlight();
        bool ok = h.Apply(right, "range", 100);
        Assert.True(ok);
        Assert.Equal(HighlightState.Active, h.State);
        Assert.True(h.IsActive);
        Assert.Same(right, h.Panel);
    }

    [Fact]
    public void Expires_AfterDuration()
    {
        var doc = new OpenDocument("hola");
        var (left, right) = EditorPanel.CreatePair(doc);
        var h = new PassengerHighlight();
        h.Apply(right, "range", 50);
        Assert.Equal(HighlightState.Active, h.State);
        System.Threading.Thread.Sleep(150);
        Assert.Equal(HighlightState.Expired, h.State);
    }

    [Fact]
    public void Expire_Manual()
    {
        var h = new PassengerHighlight();
        h.Apply(new EditorPanel(), "r", 1000);
        h.Expire();
        Assert.True(h.IsExpired);
    }

    [Fact]
    public void Apply_DoesNotModifyContent()
    {
        var doc = new OpenDocument("original content");
        var (left, right) = EditorPanel.CreatePair(doc);
        var before = doc.Content;
        var h = new PassengerHighlight();
        h.Apply(right, "range", 10);
        Assert.Equal(before, doc.Content);
        Assert.Equal("original content", doc.Content);
    }

    [Fact]
    public void AnchorNotFound_DoesNotHighlight()
    {
        var md = "# Titulo\n\npara";
        var ast = MarkdownAst.Parse(md);
        var map = PositionMap.Calculate(ast);
        var notFoundOffset = 100000;
        var anchor = map.GetAnchor(notFoundOffset);
        Assert.Null(anchor);

        var h = new PassengerHighlight();
        bool ok = h.Apply(null, null, 100);
        Assert.False(ok);
        Assert.Equal(HighlightState.Expired, h.State);
    }

    [Fact]
    public void ReducedMotion_DurationZero()
    {
        PassengerHighlight.PrefersReducedMotion = true;
        try
        {
            var h = new PassengerHighlight();
            bool ok = h.Apply(new EditorPanel(), "r", 1000);
            Assert.False(ok);
            Assert.Equal(0, h.DurationMs);
            Assert.Equal(HighlightState.Expired, h.State);
        }
        finally
        {
            PassengerHighlight.PrefersReducedMotion = false;
        }
    }

    [Fact]
    public void HighlightService_AliasWorks()
    {
        var svc = new HighlightService();
        bool ok = svc.Apply(new EditorPanel(), "r", 10);
        Assert.True(ok);
        svc.Expire();
        Assert.True(svc.IsExpired);
    }
}