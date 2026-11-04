using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T070QuoteTests
{
    [Fact]
    public void Quote_ConvertsLines()
    {
        var md = "uno\ndos\ntres";
        var engine = new FormatEngine(md);
        var outcome = engine.Quote();
        var result = outcome.ToMarkdown();
        Assert.Contains("> uno", result);
        Assert.Contains("> dos", result);
        Assert.Contains("> tres", result);
    }

    [Fact]
    public void Quote_Level2_Nests()
    {
        var md = "uno\ndos";
        var engine = new FormatEngine(md);
        var outcome = engine.Quote(2);
        var result = outcome.ToMarkdown();
        Assert.Contains(">> uno", result);
        Assert.Contains(">> dos", result);
    }

    [Fact]
    public void Quote_TogglesOffWhenAlreadyQuoted()
    {
        var md = "> uno\n> dos";
        var engine = new FormatEngine(md);
        var outcome = engine.Quote();
        var result = outcome.ToMarkdown();
        Assert.DoesNotContain("> uno", result);
        Assert.Contains("uno", result);
    }

    [Fact]
    public void Quote_StringLevel()
    {
        var md = "uno\ndos";
        var engine = new FormatEngine(md);
        var outcome = engine.Quote("nivel 2");
        var result = outcome.ToMarkdown();
        Assert.Contains(">> uno", result);
    }

    [Fact]
    public void Cita_AliasWorks()
    {
        var md = "uno\ndos";
        var engine = new FormatEngine(md);
        var outcome = engine.Cita();
        var result = outcome.ToMarkdown();
        Assert.Contains("> uno", result);
    }

    [Fact]
    public void StaticAndExtensions_Work()
    {
        var md = "a\nb";
        var r = FormatEngine.Quote(md);
        Assert.Contains("> a", r.ToMarkdown());
        var r2 = md.Quote();
        Assert.Contains("> a", r2.ToMarkdown());
    }

    [Fact]
    public void ApplyQuote_Works()
    {
        var md = "uno\ndos";
        var engine = new FormatEngine(md);
        engine.Apply("quote");
        Assert.Contains("> uno", engine.Content!);
    }

    [Fact]
    public void PromoteQuote_IncreasesLevel()
    {
        var md = "> uno";
        var engine = new FormatEngine(md);
        var outcome = engine.PromoteQuote();
        var result = outcome.ToMarkdown();
        Assert.Contains(">> uno", result);
    }

    [Fact]
    public void DemoteQuote_DecreasesLevel()
    {
        var md = ">> uno";
        var engine = new FormatEngine(md);
        var outcome = engine.DemoteQuote();
        var result = outcome.ToMarkdown();
        Assert.Contains("> uno", result);
    }

    [Fact]
    public void GetQuotes_ReturnsLevelAndContent()
    {
        var md = "> a\n>> b";
        var engine = new FormatEngine(md);
        var quotes = engine.GetQuotes();
        Assert.True(quotes.Count >= 2);
        Assert.Equal(1, quotes[0].Level);
        Assert.Contains("a", quotes[0].Content);
        Assert.Equal(2, quotes[1].Level);
        Assert.Contains("b", quotes[1].Content);
    }

    [Fact]
    public void Blockquote_AliasWorks()
    {
        var md = "uno";
        var engine = new FormatEngine(md);
        var outcome = engine.Blockquote();
        Assert.Contains("> uno", outcome.ToMarkdown());
    }

    [Fact]
    public void Unquote_RemovesMarker()
    {
        var md = "> uno";
        var engine = new FormatEngine(md);
        var outcome = engine.Unquote();
        var result = outcome.ToMarkdown();
        Assert.DoesNotContain("> uno", result);
        Assert.Contains("uno", result);
    }

    [Fact]
    public void NestedQuote_Preserved()
    {
        var md = "> a\n>> b\n> c";
        var engine = new FormatEngine(md);
        Assert.Contains(">> b", engine.Content!);
    }
}