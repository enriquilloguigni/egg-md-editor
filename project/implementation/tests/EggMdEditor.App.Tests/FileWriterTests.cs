using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class FileWriterTests
{
    [Fact]
    public void Write_CreatesUtf8WithoutBom()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_nobom.md");
        try
        {
            FileWriter.Write(path, "hola");
            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Equal("hola", File.ReadAllText(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Write_NormalizesLineEndingsToOs()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_eol.md");
        try
        {
            FileWriter.Write(path, "a\r\nb\rc\nd");
            var expected = "a" + Environment.NewLine + "b" + Environment.NewLine + "c" + Environment.NewLine + "d";
            Assert.Equal(expected, File.ReadAllText(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Write_PreservesTrailingNewline()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_trail.md");
        try
        {
            FileWriter.Write(path, "a\nb\n");
            Assert.Equal("a" + Environment.NewLine + "b" + Environment.NewLine, File.ReadAllText(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Write_NoTrailingNewline_NoAdded()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_notrail.md");
        try
        {
            FileWriter.Write(path, "a\nb");
            Assert.Equal("a" + Environment.NewLine + "b", File.ReadAllText(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Write_OverwritesExisting()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_over.md");
        try
        {
            File.WriteAllText(path, "old", new UTF8Encoding(false));
            FileWriter.Write(path, "new");
            Assert.Equal("new", File.ReadAllText(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void TryWrite_MissingParentCreates()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_sub", "deep", "f.md");
        try
        {
            Assert.True(FileWriter.TryWrite(path, "ok"));
            Assert.Equal("ok", File.ReadAllText(path));
        }
        finally
        {
            var root = Path.Combine(Path.GetTempPath(), "t024_sub");
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void RoundTrip_FileReader()
    {
        var path = Path.Combine(Path.GetTempPath(), "t024_rt.md");
        try
        {
            FileWriter.Write(path, "hola\r\nmundo");
            Assert.Equal("hola\nmundo", FileReader.Read(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}