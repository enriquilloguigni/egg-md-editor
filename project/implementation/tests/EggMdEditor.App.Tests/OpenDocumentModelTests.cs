using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class OpenDocumentModelTests
{
    [Fact]
    public void NewDocument_Defaults()
    {
        var doc = new OpenDocument();
        Assert.Equal("Sin título", doc.DisplayName);
        Assert.Equal("Sin título", doc.Title);
        Assert.Null(doc.FilePath);
        Assert.Equal(string.Empty, doc.Content);
        Assert.Equal(DocumentEncoding.Utf8, doc.Encoding);
        Assert.Equal(DocumentChangeState.Clean, doc.ChangeState);
        Assert.False(doc.HasChanges);
        Assert.Equal(DocumentOrigin.New, doc.Origin);
        Assert.Equal(DocumentLastOperation.None, doc.LastOperation);
        Assert.NotNull(doc.Metadata);
        Assert.False(doc.Metadata.IsPresent);
    }

    [Fact]
    public void NewDocument_ViaAlias_CreatesEmpty()
    {
        var doc = new Document();
        Assert.Equal("Sin título", doc.DisplayName);
        Assert.Equal(string.Empty, doc.Content);
    }

    [Fact]
    public void FilePath_UpdatesDisplayName()
    {
        var doc = new OpenDocument();
        doc.FilePath = "/tmp/foo/bar.md";
        Assert.Equal("bar.md", doc.DisplayName);
        Assert.Equal("bar.md", doc.Title);
        Assert.Equal("/tmp/foo/bar.md", doc.Path);
        doc.FilePath = null;
        Assert.Equal("Sin título", doc.DisplayName);
    }

    [Fact]
    public void Content_AliasesWork()
    {
        var doc = new OpenDocument();
        doc.Markdown = "hello";
        Assert.Equal("hello", doc.Content);
        Assert.Equal("hello", doc.Text);
        Assert.Equal("hello", doc.Body);
    }

    [Fact]
    public void ChangeState_HasChanges()
    {
        var doc = new OpenDocument();
        doc.MarkChanged();
        Assert.True(doc.HasChanges);
        Assert.True(doc.IsDirty);
        doc.MarkClean();
        Assert.False(doc.HasChanges);
    }

    [Fact]
    public void Metadata_OpaqueBlock()
    {
        var block = new MetadataBlock(true, "---\nfoo: bar\n---", "---");
        Assert.True(block.IsPresent);
        Assert.Equal("---\nfoo: bar\n---", block.Content);
        Assert.Equal("---", block.Delimiters);
        Assert.Equal(MetadataPosition.Start, block.Position);

        var doc = new OpenDocument();
        doc.Metadata = block;
        Assert.True(doc.Metadata.IsPresent);
        Assert.Same(block, doc.MetadataBlock);
    }

    [Fact]
    public void PropertyChanged_Fires()
    {
        var doc = new OpenDocument();
        var fired = false;
        doc.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(OpenDocument.Content)) fired = true; };
        doc.Content = "x";
        Assert.True(fired);
    }
}