using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T065FormatEngineTests
{
    [Fact]
    public void Constructor_WithAst_StoresAst()
    {
        var md = "# Hola\n\nPárrafo.";
        var ast = MarkdownAst.Parse(md);
        var engine = new FormatEngine(ast);
        Assert.Same(ast, engine.Ast);
        Assert.True(engine.IsValid);
    }

    [Fact]
    public void StaticApply_ReturnsAst()
    {
        var md = "texto";
        var ast = MarkdownAst.Parse(md);
        var result = FormatEngine.Apply(ast, "bold");
        Assert.True(result);
        Assert.NotNull(result.Ast);
        Assert.Contains("texto", result.ToMarkdown());
    }

    [Fact]
    public void InstanceApply_ReturnsOutcome()
    {
        var engine = new FormatEngine("# h");
        var outcome = engine.Apply("heading");
        Assert.True(outcome);
        Assert.NotNull(outcome.Ast);
    }

    [Fact]
    public void AliasClasses_Work()
    {
        var ast = MarkdownAst.Parse("x");
        var e = new MotorFormato(ast);
        Assert.Same(ast, e.Ast);
        var e2 = new MarkdownFormatEngine("y");
        Assert.NotNull(e2.Ast);
    }

    [Fact]
    public void Constructor_WithDocument_Works()
    {
        var doc = new OpenDocument("hola");
        var engine = new FormatEngine(doc);
        Assert.Same(doc, engine.Document);
        Assert.NotNull(engine.Ast);
    }
}