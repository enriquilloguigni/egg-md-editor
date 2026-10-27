using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class FileReaderTests
{
    [Fact]
    public void Read_WithoutBom_ReturnsContent()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_nobom.md");
        try
        {
            File.WriteAllText(path, "hola\nmundo", new UTF8Encoding(false));
            Assert.Equal("hola\nmundo", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_WithBom_StripsBom()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_bom.md");
        try
        {
            var bytes = new UTF8Encoding(true).GetBytes("hola");
            File.WriteAllBytes(path, bytes);
            Assert.Equal("hola", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_Crlf_NormalizedToLf()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_crlf.md");
        try
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes("a\r\nb\r\nc"));
            Assert.Equal("a\nb\nc", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_Cr_NormalizedToLf()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_cr.md");
        try
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes("a\rb\rc"));
            Assert.Equal("a\nb\nc", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_PreservesTrailingNewline()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_trail.md");
        try
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes("a\nb\n"));
            Assert.Equal("a\nb\n", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_NoTrailingNewline_NoAdded()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_notrail.md");
        try
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes("a\nb"));
            Assert.Equal("a\nb", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Read_InvalidUtf8_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), "t023_bad.md");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xFF, 0xFE, 0x41 }); // invalid UTF-8-ish
            Assert.ThrowsAny<Exception>(() => FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void TryRead_Missing_ReturnsFalse()
    {
        Assert.False(FileReader.TryRead("/tmp/does-not-exist-t023.md", out var content));
        Assert.Null(content);
    }

    [Fact]
    public void NormalizeLineEndings_Mixed()
    {
        Assert.Equal("a\nb\nc\nd", FileReader.NormalizeLineEndings("a\r\nb\rc\nd"));
    }

    [Fact]
    public void HasBom_Detects()
    {
        Assert.True(FileReader.HasBom(new byte[] { 0xEF, 0xBB, 0xBF, 0x41 }));
        Assert.False(FileReader.HasBom(new byte[] { 0x41 }));
    }
}