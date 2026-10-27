using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T027AbrirRecienteLocalTests
{
    [Fact]
    public void AbrirReciente_ValidMd_CreatesDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_valid.md");
        try
        {
            File.WriteAllText(path, "hola\nmundo", new UTF8Encoding(false));
            var store = new RecentFiles();
            store.Add(path);
            var doc = AbrirRecienteOperation.Execute(path, store);
            Assert.Equal("hola\nmundo", doc.Content);
            Assert.Equal(path, doc.FilePath);
            Assert.Equal("t027_valid.md", doc.DisplayName);
            Assert.False(doc.HasChanges);
            Assert.Equal(DocumentOrigin.Recent, doc.Origin);
            Assert.Equal(DocumentLastOperation.Open, doc.LastOperation);
            Assert.True(store.Contains(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void AbrirReciente_EntryOverload_Works()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_entry.md");
        try
        {
            File.WriteAllText(path, "entry content");
            var store = new RecentFiles();
            store.Add(path);
            var entry = store.Entries[0];
            var doc = AbrirRecienteOperation.Execute(entry, store);
            Assert.Equal("entry content", doc.Content);
            Assert.Equal(DocumentOrigin.Recent, doc.Origin);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void AbrirReciente_Missing_ThrowsAndRemoves()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_missing.md");
        var store = new RecentFiles();
        store.Add(path);
        try { AbrirRecienteOperation.Execute(path, store); }
        catch (FileNotFoundException) { }
        Assert.False(store.Contains(path));
    }

    [Fact]
    public void AbrirReciente_Missing_DocumentUnchangedNotApplicable()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_missing2.md");
        var store = new RecentFiles();
        store.Add(path);
        Assert.Throws<FileNotFoundException>(() => AbrirRecienteOperation.Execute(new RecentFile(path), store));
        Assert.False(store.Contains(path));
    }

    [Fact]
    public void AbrirReciente_InvalidExtension_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027.txt");
        File.WriteAllText(path, "x");
        try { Assert.Throws<ArgumentException>(() => AbrirRecienteOperation.Execute(path)); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void RecentFiles_OpenRecent_Works()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_store.md");
        try
        {
            File.WriteAllText(path, "store");
            var store = new RecentFiles();
            store.Add(path);
            var doc = store.OpenRecent(path);
            Assert.Equal("store", doc.Content);
            Assert.Equal(DocumentOrigin.Recent, doc.Origin);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void OpenDocument_OpenRecent_StaticWorks()
    {
        var path = Path.Combine(Path.GetTempPath(), "t027_static.md");
        try
        {
            File.WriteAllText(path, "static");
            var doc = OpenDocument.OpenRecent(path);
            Assert.Equal("static", doc.Content);
            Assert.Equal(DocumentOrigin.Recent, doc.Origin);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}