using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T068HeadingAnchorTests
{
    [Fact]
    public void Slugify_Basic()
    {
        Assert.Equal("hello-world", FormatEngine.Slugify("Hello World!"));
        Assert.Equal("foo-bar-baz", FormatEngine.Slugify("Foo   Bar   Baz"));
        Assert.Equal("c-c", FormatEngine.Slugify("C++ & C#"));
    }

    [Fact]
    public void Slugify_DeduplicatesPerDocument()
    {
        var md = "# Foo\n# Foo\n# Foo\n";
        var engine = new FormatEngine(md);
        var anchors = engine.GetHeadingAnchors();
        Assert.Equal("foo", anchors[0]);
        Assert.Equal("foo-1", anchors[1]);
        Assert.Equal("foo-2", anchors[2]);
    }

    [Fact]
    public void GetHeadings_ReturnsLevelAndText()
    {
        var md = "# Title\n## Sub\n";
        var engine = new FormatEngine(md);
        var hs = engine.GetHeadings();
        Assert.Equal(2, hs.Count);
        Assert.Equal(1, hs[0].Level);
        Assert.Equal("Title", hs[0].Text);
        Assert.Equal(2, hs[1].Level);
        Assert.Equal("Sub", hs[1].Text);
    }

    [Fact]
    public void ResolveInternalLink_ByAnchor()
    {
        var md = "# Hello\n## World\n";
        var engine = new FormatEngine(md);
        var h = engine.ResolveHeading("#world");
        Assert.NotNull(h);
        Assert.Equal("World", h!.Text);
    }

    [Fact]
    public void ResolveInternalLink_ByText()
    {
        var md = "# Getting Started\n";
        var engine = new FormatEngine(md);
        var h = engine.ResolveInternalLink("Getting Started");
        Assert.NotNull(h);
        Assert.Equal("getting-started", h!.Anchor);
    }

    [Fact]
    public void IsInternalLink()
    {
        var engine = new FormatEngine("# x");
        Assert.True(engine.IsInternalLink("#foo"));
        Assert.False(engine.IsInternalLink("https://example.com"));
    }
}