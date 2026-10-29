using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T044PositionMapTests
{
    [Fact]
    public void Calculate_HeadingsListsCode_Anchors()
    {
        var md = "# Titulo\n\n- item uno\n- item dos\n\n```\ncode here\n```";
        var ast = MarkdownAst.Parse(md);
        var map = PositionMap.Calculate(ast);
        Assert.NotNull(map);
        Assert.True(map.Anchors.Count >= 3);
        Assert.True(map.Coverage > 0);
        Assert.True(map.CalculatedAt != default);
    }

    [Fact]
    public void CorrespondenceEngine_StaticAndInstance()
    {
        var md = "# H\n\nParagraph\n";
        var ast = MarkdownAst.Parse(md);
        var map1 = CorrespondenceEngine.Calculate(ast);
        Assert.NotNull(map1);
        Assert.True(map1.Anchors.Count >= 1);

        var engine = new CorrespondenceEngine();
        var map2 = engine.Calculate(ast);
        Assert.NotNull(map2);
    }

    [Fact]
    public void GetAnchor_InsideHeading_ReturnsNotNull()
    {
        var md = "# Titulo\n\n- item\n\n```\ncode\n```";
        var ast = MarkdownAst.Parse(md);
        var map = CorrespondenceEngine.Calculate(ast);
        var firstAnchor = map.Anchors[0];
        var target = map.GetAnchor(firstAnchor.SourceStart);
        Assert.NotNull(target);
    }

    [Fact]
    public void GetAnchor_WithoutAnchor_ReturnsNull()
    {
        var md = "# Titulo\n\n- item\n\n```\ncode\n```";
        var ast = MarkdownAst.Parse(md);
        var map = CorrespondenceEngine.Calculate(ast);
        // offset far beyond source
        var target = map.GetAnchor(100000);
        Assert.Null(target);
    }

    [Fact]
    public void Constructor_WithAst_Works()
    {
        var md = "# H";
        var ast = MarkdownAst.Parse(md);
        var map = new PositionMap(ast);
        Assert.True(map.Anchors.Count >= 1);
        Assert.True(map.Coverage > 0);
    }

    [Fact]
    public void GetAnchor_StaticOverloads()
    {
        var md = "# H\n\npara";
        var ast = MarkdownAst.Parse(md);
        var t = PositionMap.GetAnchor(ast, 0);
        // may be null or not depending anchor; just ensure no throw
        _ = t;
    }
}