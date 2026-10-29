using EggMdEditor.App;
using Xunit;

public class T042RenderTests
{
    [Fact]
    public void RenderMarkdown_ReproducesContent()
    {
        var md = "# Hola\n\nPárrafo con **negrita**.";
        var ast = MarkdownAst.Parse(md);
        var result = Renderer.RenderMarkdown(ast);
        Assert.Contains("Hola", result);
        Assert.Contains("negrita", result);
    }

    [Fact]
    public void RenderWysiwyg_NoSyntax()
    {
        var md = "# Hola\n\nPárrafo con **negrita**.";
        var ast = MarkdownAst.Parse(md);
        var result = Renderer.RenderWysiwyg(ast);
        Assert.Contains("Hola", result);
        Assert.Contains("negrita", result);
        Assert.DoesNotContain("#", result);
        Assert.DoesNotContain("**", result);
    }

    [Fact]
    public void RenderWysiwyg_InvalidNode_Marker()
    {
        var md = "```mermaid\ngraph TD; A-->B;\n```";
        var ast = MarkdownAst.Parse(md);
        var result = Renderer.RenderWysiwyg(ast);
        Assert.Contains("No se pudo renderizar este bloque en la vista WYSIWYG.", result);
    }

    [Fact]
    public void RenderWysiwyg_Latex_Marker()
    {
        var md = "Fórmula: $$E=mc^2$$";
        var ast = MarkdownAst.Parse(md);
        var result = Renderer.RenderWysiwyg(ast);
        Assert.Contains("No se pudo renderizar este bloque en la vista WYSIWYG.", result);
    }

    [Fact]
    public void RenderMarkdown_Instance()
    {
        var md = "texto";
        var ast = new MarkdownAst(md);
        Assert.Equal(md, ast.RenderMarkdown());
    }
}