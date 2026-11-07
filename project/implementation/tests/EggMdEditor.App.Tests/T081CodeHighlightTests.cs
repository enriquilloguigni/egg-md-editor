using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T081CodeHighlightTests
{
    [Fact]
    public void Highlight_ContainsLanguageAndCopyButton()
    {
        var html = CodeHighlighter.Highlight("var x = 1;", "csharp");
        Assert.Contains("language-csharp", html);
        Assert.Contains("copy-code", html);
        Assert.Contains("Copiar", html);
        Assert.Contains("var x = 1;", html);
        Assert.Contains("<span", html);
    }

    [Fact]
    public void Highlight_InlineCode()
    {
        var html = CodeHighlighter.HighlightInline("x+1");
        Assert.Contains("<code class=\"inline-code\">", html);
        Assert.Contains("x+1", html);
    }

    [Fact]
    public void Renderer_RenderCode_Works()
    {
        var html = Renderer.RenderCode("print('hi')", "python");
        Assert.Contains("language-python", html);
        Assert.Contains("copy-code", html);
    }

    [Fact]
    public void Renderer_RenderWysiwyg_CodeBlock_HasCopyText()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var result = Renderer.RenderWysiwyg(md);
        Assert.Contains("Copiar", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AliasClasses_Work()
    {
        var h = new SyntaxHighlighter("code", "js");
        Assert.Contains("language-js", h.Highlight());
        var b = new CopyCodeButton();
        Assert.Contains("copy-code", b.Render());
    }

    [Fact]
    public void StaticHelpers_Work()
    {
        var html = CodeHighlighters.Highlight("a", "go");
        Assert.Contains("language-go", html);
    }

    [Fact]
    public void EmptyLanguage_DefaultsPlaintext()
    {
        var html = CodeHighlighter.Highlight("hello");
        Assert.Contains("language-plaintext", html);
    }
}