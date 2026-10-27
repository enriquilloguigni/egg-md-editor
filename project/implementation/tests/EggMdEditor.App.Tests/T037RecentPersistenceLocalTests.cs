using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T037RecentPersistenceLocalTests
{
    [Fact]
    public void Load_CorruptedJson_ReturnsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), "t037_corrupt.json");
        try
        {
            File.WriteAllText(path, "{ not json", new UTF8Encoding(false));
            var loaded = RecentFiles.Load(path);
            Assert.NotNull(loaded);
            Assert.Empty(loaded);
            Assert.False(RecentFiles.TryLoad(path, out var store));
            Assert.Null(store);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Save_Atomic_NoTempLeft()
    {
        var path = Path.Combine(Path.GetTempPath(), "t037_atomic.json");
        try
        {
            var store = new RecentFiles();
            store.Add("/tmp/a.md");
            store.Save(path);
            Assert.True(File.Exists(path));
            Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + ".*.tmp"));
            var loaded = RecentFiles.Load(path);
            Assert.True(loaded.Count == 1);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Save_Failure_OnReadOnlyDir_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "t037_ro_dir");
        var path = Path.Combine(dir, "recents.json");
        Directory.CreateDirectory(dir);
        try
        {
            new DirectoryInfo(dir).Attributes |= FileAttributes.ReadOnly;
            var store = new RecentFiles();
            store.Add("/tmp/a.md");
            Assert.ThrowsAny<Exception>(() => store.Save(path));
            // original file not created
            Assert.False(File.Exists(path));
        }
        finally
        {
            new DirectoryInfo(dir).Attributes &= ~FileAttributes.ReadOnly;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TrySave_Failure_ReturnsFalse()
    {
        var dir = Path.Combine(Path.GetTempPath(), "t037_ro_dir2");
        var path = Path.Combine(dir, "recents.json");
        Directory.CreateDirectory(dir);
        try
        {
            new DirectoryInfo(dir).Attributes |= FileAttributes.ReadOnly;
            var store = new RecentFiles();
            store.Add("/tmp/a.md");
            Assert.False(store.TrySave(path));
        }
        finally
        {
            new DirectoryInfo(dir).Attributes &= ~FileAttributes.ReadOnly;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Load_Missing_ReturnsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), "t037_missing.json");
        if (File.Exists(path)) File.Delete(path);
        var loaded = RecentFiles.Load(path);
        Assert.Empty(loaded);
    }

    [Fact]
    public void DefaultPath_RoundTrip()
    {
        // avoid polluting default path; use explicit
        var path = Path.Combine(Path.GetTempPath(), "t037_default.json");
        try
        {
            var store = new RecentFiles(path);
            // empty file created? Load missing => empty
            Assert.Empty(store);
            store.Add("/tmp/x.md");
            store.Save(path);
            var loaded = new RecentFiles(path);
            Assert.True(loaded.Count == 1);
            Assert.Equal("/tmp/x.md", loaded.Entries[0].Path);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}