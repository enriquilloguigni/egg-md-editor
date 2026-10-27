using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T028GuardarTests
{
    [Fact]
    public void Guardar_ExistingPath_WritesAndCleans()
    {
        var path = Path.Combine(Path.GetTempPath(), "t028_save.md");
        try
        {
            File.WriteAllText(path, "old", new UTF8Encoding(false));
            var doc = new OpenDocument("new content", path);
            doc.MarkChanged();
            var store = new RecentFiles();
            var recovery = new RecoveryCopy(doc);
            var saved = GuardarOperation.Execute(doc, store, recovery);
            Assert.Same(doc, saved);
            Assert.Equal("new content", File.ReadAllText(path));
            Assert.False(doc.HasChanges);
            Assert.Equal(path, doc.FilePath);
            Assert.Equal(DocumentLastOperation.Save, doc.LastOperation);
            Assert.True(store.Contains(path));
            Assert.Equal(path, store.Entries[0].Path);
            Assert.True(recovery.IsObsolete);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Guardar_NoPath_Throws()
    {
        var doc = new OpenDocument("x");
        Assert.Throws<ArgumentException>(() => GuardarOperation.Execute(doc));
    }

    [Fact]
    public void GuardarComo_NewPath_WritesCopyAndSwitches()
    {
        var original = Path.Combine(Path.GetTempPath(), "t028_orig.md");
        var copy = Path.Combine(Path.GetTempPath(), "t028_copy.md");
        try
        {
            File.WriteAllText(original, "original content", new UTF8Encoding(false));
            var doc = new OpenDocument("edited", original);
            doc.MarkChanged();
            var store = new RecentFiles();
            store.Add(original);
            var saved = GuardarComoOperation.Execute(doc, copy, store);
            Assert.Same(doc, saved);
            // original unchanged
            Assert.Equal("original content", File.ReadAllText(original));
            // copy written
            Assert.Equal("edited", File.ReadAllText(copy));
            Assert.Equal(copy, doc.FilePath);
            Assert.False(doc.HasChanges);
            Assert.Equal(DocumentLastOperation.SaveAs, doc.LastOperation);
            Assert.True(store.Contains(copy));
            Assert.Equal(copy, store.Entries[0].Path);
        }
        finally
        {
            if (File.Exists(original)) File.Delete(original);
            if (File.Exists(copy)) File.Delete(copy);
        }
    }

    [Fact]
    public void GuardarComo_AppendsMdExtension()
    {
        var path = Path.Combine(Path.GetTempPath(), "t028_noext");
        var expected = path + ".md";
        try
        {
            var doc = new OpenDocument("content");
            var saved = GuardarComoOperation.Execute(doc, path);
            Assert.Equal(expected, doc.FilePath);
            Assert.True(File.Exists(expected));
            Assert.Equal("content", File.ReadAllText(expected));
        }
        finally
        {
            if (File.Exists(expected)) File.Delete(expected);
        }
    }

    [Fact]
    public void OpenDocument_Save_StaticWorks()
    {
        var path = Path.Combine(Path.GetTempPath(), "t028_static.md");
        try
        {
            var doc = new OpenDocument("static content");
            var saved = OpenDocument.Save(doc, path);
            Assert.Equal("static content", File.ReadAllText(path));
            Assert.Equal(path, doc.FilePath);
            Assert.False(doc.HasChanges);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void OpenDocument_SaveAs_StaticWorks()
    {
        var path = Path.Combine(Path.GetTempPath(), "t028_staticas.md");
        try
        {
            var doc = new OpenDocument("static as");
            var saved = OpenDocument.SaveAs(doc, path);
            Assert.Equal("static as", File.ReadAllText(path));
            Assert.Equal(path, doc.FilePath);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Document_Instance_Save_Works()
    {
        var path = Path.Combine(Path.GetTempPath(), "t028_instance.md");
        try
        {
            var doc = new OpenDocument("instance");
            doc.Save(path);
            Assert.Equal("instance", File.ReadAllText(path));
            Assert.False(doc.HasChanges);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Guardar_Failure_LeavesDocumentChanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "t028_ro_dir");
        var path = Path.Combine(dir, "x.md");
        Directory.CreateDirectory(dir);
        try
        {
            // make dir read-only to force failure on write
            new DirectoryInfo(dir).Attributes |= FileAttributes.ReadOnly;
            var doc = new OpenDocument("content", path);
            doc.MarkChanged();
            Assert.ThrowsAny<Exception>(() => GuardarOperation.Execute(doc));
            Assert.True(doc.HasChanges);
        }
        finally
        {
            new DirectoryInfo(dir).Attributes &= ~FileAttributes.ReadOnly;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Guardar_RecentsToFront()
    {
        var p1 = Path.Combine(Path.GetTempPath(), "t028_r1.md");
        var p2 = Path.Combine(Path.GetTempPath(), "t028_r2.md");
        try
        {
            File.WriteAllText(p1, "a");
            File.WriteAllText(p2, "b");
            var store = new RecentFiles();
            store.Add(p1);
            store.Add(p2);
            // p2 is front initially? Add sorts by time; force p1 front by touching? just check after save p1 moves front
            var doc = new OpenDocument("content", p1);
            GuardarOperation.Execute(doc, store);
            Assert.Equal(p1, store.Entries[0].Path);
        }
        finally
        {
            if (File.Exists(p1)) File.Delete(p1);
            if (File.Exists(p2)) File.Delete(p2);
        }
    }
}