using Markdig;
using Xunit;

namespace EggMdEditor.App.Tests;

public class TmpScriptTest
{
    [Fact]
    public void Print()
    {
        var md = "Hello <script>alert(1)</script>\n\n[x](javascript:alert(1))";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(md, pipeline);
        System.IO.File.WriteAllText("/tmp/opencode/markdig_script.html", html);
        Assert.True(true);
    }
}