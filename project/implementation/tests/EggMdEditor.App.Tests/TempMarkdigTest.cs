using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Xunit;

namespace EggMdEditor.App.Tests;

public class TempMarkdigTest
{
    [Fact]
    public void Print()
    {
        var md = "# Café\n# naïve\n# 日本語\n# 123 test\n# ---\n# a.b.c\n";
        var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers().Build();
        var html = Markdown.ToHtml(md, pipeline);
        System.IO.File.WriteAllText("/tmp/opencode/markdig2.html", html);
        Assert.True(true);
    }
}