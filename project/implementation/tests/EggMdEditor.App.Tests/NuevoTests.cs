using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class NuevoTests
{
    [Fact]
    public void Nuevo_CreatesEmptyDocument()
    {
        var doc = NuevoOperation.Execute();
        Assert.Equal("Sin título", doc.DisplayName);
        Assert.Null(doc.FilePath);
        Assert.Equal(string.Empty, doc.Content);
        Assert.False(doc.HasChanges);
        Assert.Equal(DocumentOrigin.New, doc.Origin);
    }

    [Fact]
    public void FileReader_New_ReturnsEmpty()
    {
        var doc = FileReader.New();
        Assert.Equal("Sin título", doc.DisplayName);
        Assert.Null(doc.FilePath);
        Assert.Equal(string.Empty, doc.Content);
    }

    [Fact]
    public void Nuevo_ReplacesCurrent()
    {
        var current = new OpenDocument("changed", "/tmp/x.md");
        current.MarkChanged();
        var op = new NuevoOperation(current);
        var replaced = NuevoOperation.Execute();
        Assert.NotSame(current, replaced);
        Assert.Equal("Sin título", replaced.DisplayName);
        Assert.Null(replaced.FilePath);
    }

    [Fact]
    public void OpenDocument_NewFactory()
    {
        var doc = OpenDocument.New();
        Assert.Equal("Sin título", doc.Title);
    }
}
