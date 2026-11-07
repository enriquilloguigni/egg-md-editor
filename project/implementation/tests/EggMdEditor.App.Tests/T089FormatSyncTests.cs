using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T089FormatSyncTests
{
    [Fact]
    public void FormatEngine_Apply_UpdatesBothPanels()
    {
        var doc = new OpenDocument("hello");
        var (left, right) = EditorPanel.CreatePair(doc);
        var engine = new FormatEngine(left, right, left);
        var outcome = engine.Apply("bold");
        Assert.True(outcome);
        Assert.Equal(Renderer.RenderWysiwyg(doc), left.RenderedContent);
        Assert.Equal(doc.Content, right.RenderedContent);
        Assert.Contains("****", doc.Content);
    }

    [Fact]
    public void FormatSync_ApplyFormat_UpdatesBothPanels()
    {
        var doc = new OpenDocument("hello");
        var (left, right) = EditorPanel.CreatePair(doc);
        var sync = new FormatSync(left, right);
        bool ok = sync.ApplyFormat("bold");
        Assert.True(ok);
        Assert.Equal(Renderer.RenderWysiwyg(doc), left.RenderedContent);
        Assert.Equal(doc.Content, right.RenderedContent);
    }

    [Fact]
    public void FormatBarAction_ApplyFormat_UpdatesBothPanels()
    {
        var doc = new OpenDocument("hello");
        var (left, right) = EditorPanel.CreatePair(doc);
        var bar = new FormatBarAction(left, right, left);
        bool ok = bar.ApplyFormat("bold");
        Assert.True(ok);
        Assert.Equal(Renderer.RenderWysiwyg(doc), left.RenderedContent);
        Assert.Equal(doc.Content, right.RenderedContent);
    }

    [Fact]
    public void MainWindowViewModel_ApplyFormat_UpdatesBothPanels()
    {
        var doc = new OpenDocument("hello");
        var (left, right) = EditorPanel.CreatePair(doc);
        var vm = new MainWindowViewModel();
        vm.Document = doc;
        vm.LeftPanel = left;
        vm.RightPanel = right;
        vm.ActivePanel = left;
        bool ok = vm.ApplyFormat("bold");
        Assert.True(ok);
        Assert.Equal(Renderer.RenderWysiwyg(doc), left.RenderedContent);
        Assert.Equal(doc.Content, right.RenderedContent);
    }

    [Fact]
    public void FormatEngine_AllFormats_DoNotBreakSync()
    {
        var doc = new OpenDocument("texto");
        var (left, right) = EditorPanel.CreatePair(doc);
        var engine = new FormatEngine(left, right, left);
        foreach (var key in new[] { "bold", "italic", "heading", "list", "quote", "code", "link", "table", "escape" })
        {
            engine.Apply(key);
            Assert.Equal(doc.Content, right.RenderedContent);
            Assert.Equal(Renderer.RenderWysiwyg(doc), left.RenderedContent);
        }
    }
}
