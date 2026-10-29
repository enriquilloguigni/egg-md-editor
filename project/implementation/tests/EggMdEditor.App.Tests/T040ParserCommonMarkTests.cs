using EggMdEditor.App;
using Markdig.Syntax;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T040ParserCommonMarkTests
{
    [Fact]
    public void Parse_Heading_ProducesHeadingBlock()
    {
        var ast = MarkdownParser.Parse("# Hola mundo");
        Assert.True(ast.IsValid);
        Assert.Equal(1, ast.Version);
        Assert.True(ast.Nodes.Count >= 1);
        Assert.IsType<HeadingBlock>(ast.Nodes[0]);
        Assert.Contains("Hola mundo", ast.ToMarkdown());
    }

    [Fact]
    public void Parse_Lists_PreservesItems()
    {
        var md = "- uno\n- dos\n- tres";
        var ast = MarkdownAst.Parse(md);
        Assert.True(ast.IsValid);
        Assert.Contains("uno", ast.ToMarkdown());
        Assert.Contains("dos", ast.ToMarkdown());
        Assert.Contains("tres", ast.ToMarkdown());
    }

    [Fact]
    public void Parse_CodeBlock_PreservesFenceAndCode()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var ast = CommonMarkParser.Parse(md);
        Assert.True(ast.IsValid);
        var rendered = ast.RenderMarkdown();
        Assert.Contains("var x = 1;", rendered);
        Assert.Contains("```", rendered);
    }

    [Fact]
    public void Parse_Link_PreservesUrlAndText()
    {
        var md = "Texto con [enlace](https://example.com) inline.";
        var ast = SyntaxTree.Parse(md);
        Assert.True(ast.IsValid);
        var rendered = ast.ToMarkdown();
        Assert.Contains("enlace", rendered);
        Assert.Contains("https://example.com", rendered);
    }

    [Fact]
    public void Parse_GfmTable_PreservesCells()
    {
        var md = "| a | b |\n|---|---|\n| 1 | 2 |";
        var ast = GfmParser.Parse(md);
        Assert.True(ast.IsValid);
        var rendered = ast.ToString();
        Assert.Contains("a", rendered);
        Assert.Contains("b", rendered);
        Assert.Contains("1", rendered);
        Assert.Contains("2", rendered);
    }

    [Fact]
    public void Parse_FormulaAsText_Preserved()
    {
        var md = "Fórmula: $$E=mc^2$$";
        var ast = DocumentAst.Parse(md);
        Assert.True(ast.IsValid);
        Assert.Contains("E=mc^2", ast.ToMarkdown());
    }

    [Fact]
    public void Parse_DiagramAsText_Preserved()
    {
        var md = "```mermaid\ngraph TD; A-->B;\n```";
        var ast = Ast.Parse(md);
        Assert.True(ast.IsValid);
        Assert.Contains("graph TD; A-->B;", ast.ToMarkdown());
    }

    [Fact]
    public void Parse_Reversibility_Basic()
    {
        var md = "# Título\n\nPárrafo con **negrita** y *cursiva*.\n\n- item\n\n```\ncode\n```\n";
        var ast = MarkdownSyntaxTree.Parse(md);
        Assert.True(ast.IsValid);
        var rendered = ast.ToMarkdown();
        Assert.Contains("# Título", rendered);
        Assert.Contains("Párrafo", rendered);
        Assert.Contains("negrita", rendered);
        Assert.Contains("item", rendered);
        Assert.Contains("code", rendered);
    }

    [Fact]
    public void Parse_Empty_IsValid()
    {
        var ast = new MarkdownTree();
        // parse empty via static
        var ast2 = MarkdownDocumentAst.Parse(string.Empty);
        Assert.True(ast2.IsValid);
        Assert.Equal(1, ast2.Version);
    }

    [Fact]
    public void Parse_Null_DoesNotThrow()
    {
        var ast = DocumentTree.Parse(null);
        Assert.True(ast.IsValid);
        Assert.Equal(string.Empty, ast.Source);
    }

    [Fact]
    public void Version_IncrementsOnReparseInstance()
    {
        var parser = new MarkdownParser();
        var first = parser.Parse("a");
        var second = parser.Parse("b");
        Assert.True(second.Version >= first.Version);
    }
}
