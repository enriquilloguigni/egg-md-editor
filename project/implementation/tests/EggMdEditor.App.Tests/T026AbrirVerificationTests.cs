using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T026AbrirVerificationTests
{
    [Fact]
    public void Abrir_ValidMd_CreatesDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), "t026_valid.md");
        try
        {
            File.WriteAllText(path, "hola\nmundo", new UTF8Encoding(false));
            var doc = AbrirOperation.Execute(path);
            Assert.Equal("hola\nmundo", doc.Content);
            Assert.Equal(path, doc.FilePath);
            Assert.Equal("t026_valid.md", doc.DisplayName);
            Assert.False(doc.HasChanges);
            Assert.Equal(DocumentOrigin.Open, doc.Origin);
            Assert.Equal(DocumentLastOperation.Open, doc.LastOperation);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Abrir_InvalidExtension_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), "t026.txt");
        File.WriteAllText(path, "x");
        try { Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(path)); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Abrir_Missing_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => AbrirOperation.Execute(Path.Combine(Path.GetTempPath(), "nope_t026.md")));
    }

    [Fact]
    public void Abrir_InvalidUtf8_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), "t026_bad.md");
        File.WriteAllBytes(path, new byte[] { 0xFF, 0xFE, 0x41 });
        try { Assert.ThrowsAny<Exception>(() => AbrirOperation.Execute(path)); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Abrir_Drag_Multiple_Throws()
    {
        Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(new[] { "a.md", "b.md" }));
    }

    [Fact]
    public void Abrir_Drag_NonMd_Throws()
    {
        Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(new[] { "a.txt" }));
    }

    [Fact]
    public void Abrir_Drag_Valid_CreatesDragDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), "t026_drag.md");
        try
        {
            File.WriteAllText(path, "drag content");
            var doc = AbrirOperation.Execute(new[] { path });
            Assert.Equal("drag content", doc.Content);
            Assert.Equal(DocumentOrigin.Drag, doc.Origin);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Abrir_ValidateDrop_Works()
    {
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.md", "b.md" }));
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.txt" }));
        Assert.True(AbrirOperation.ValidateDrop(new[] { "a.md" }));
    }

    [Fact]
    public void OpenDocument_Open_StaticWorks()
    {
        var path = Path.Combine(Path.GetTempPath(), "t026_static.md");
        try
        {
            File.WriteAllText(path, "static");
            var doc = OpenDocument.Open(path);
            Assert.Equal("static", doc.Content);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}