using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T069ListTests
{
    [Fact]
    public void Bullet_ConvertsLines()
    {
        var md = "uno\ndos\ntres";
        var engine = new FormatEngine(md);
        var outcome = engine.Bullet();
        var result = outcome.ToMarkdown();
        Assert.Contains("- uno", result);
        Assert.Contains("- dos", result);
        Assert.Contains("- tres", result);
    }

    [Fact]
    public void Numbered_RenumbersSequentially()
    {
        var md = "1. uno\n1. dos\n1. tres";
        var engine = new FormatEngine(md);
        var outcome = engine.Numbered();
        var result = outcome.ToMarkdown();
        Assert.Contains("1. uno", result);
        Assert.Contains("2. dos", result);
        Assert.Contains("3. tres", result);
    }

    [Fact]
    public void Task_CreatesCheckboxes()
    {
        var md = "uno\ndos";
        var engine = new FormatEngine(md);
        var outcome = engine.Task();
        var result = outcome.ToMarkdown();
        Assert.Contains("- [ ] uno", result);
        Assert.Contains("- [ ] dos", result);
    }

    [Fact]
    public void Toggle_FlipsCheckbox()
    {
        var md = "- [ ] a\n- [x] b";
        var engine = new FormatEngine(md);
        var outcome = engine.Toggle();
        var result = outcome.ToMarkdown();
        Assert.Contains("- [X] a", result);
        Assert.Contains("- [ ] b", result);
    }

    [Fact]
    public void Nest_Indents()
    {
        var md = "- a\n- b";
        var engine = new FormatEngine(md);
        var outcome = engine.Nest();
        var result = outcome.Source ?? outcome.ToMarkdown();
        Assert.Contains("  - a", result);
        Assert.Contains("  - b", result);
    }

    [Fact]
    public void Unnest_Outdents()
    {
        var md = "  - a\n  - b";
        var engine = new FormatEngine(md);
        var outcome = engine.Unnest();
        var result = outcome.Source ?? outcome.ToMarkdown();
        Assert.Contains("- a", result);
        Assert.Contains("- b", result);
    }

    [Fact]
    public void Renumber_FixesGaps()
    {
        var md = "1. a\n3. b\n5. c";
        var engine = new FormatEngine(md);
        var outcome = engine.Renumber();
        var result = outcome.ToMarkdown();
        Assert.Contains("1. a", result);
        Assert.Contains("2. b", result);
        Assert.Contains("3. c", result);
    }

    [Fact]
    public void GetLists_ReturnsItems()
    {
        var md = "- a\n- b\n1. x";
        var engine = new FormatEngine(md);
        var lists = engine.GetLists();
        Assert.True(lists.Count >= 1);
    }

    [Fact]
    public void StaticAndExtensions_Work()
    {
        var md = "a\nb";
        var r = FormatEngine.Bullet(md);
        Assert.Contains("- a", r.ToMarkdown());
        var r2 = md.Bullet();
        Assert.Contains("- a", r2.ToMarkdown());
    }

    [Fact]
    public void NestedList_IndentationPreserved()
    {
        var md = "- a\n  - b\n- c";
        var engine = new FormatEngine(md);
        Assert.Contains("  - b", engine.Content!);
    }

    [Fact]
    public void Task_CheckboxAlternable()
    {
        var md = "- [ ] task";
        var t = new FormatEngine(md).Toggle().ToMarkdown();
        Assert.Contains("- [X] task", t);
        var t2 = new FormatEngine(t).Toggle().ToMarkdown();
        Assert.Contains("- [ ] task", t2);
    }
}