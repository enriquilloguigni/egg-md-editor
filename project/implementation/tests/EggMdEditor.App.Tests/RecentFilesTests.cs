using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class RecentFilesTests
{
    [Fact]
    public void Default_MaxCount_IsTen()
    {
        var store = new RecentFiles();
        Assert.Equal(10, store.MaxCount);
        Assert.Empty(store);
    }

    [Fact]
    public void Add_MovesToFront_AndDeduplicates()
    {
        var store = new RecentFiles();
        store.Add("/tmp/a.md");
        store.Add("/tmp/b.md");
        store.Add("/tmp/a.md");

        Assert.Equal(2, store.Count);
        Assert.Equal("/tmp/a.md", store.Entries[0].Path);
        Assert.Equal("/tmp/b.md", store.Entries[1].Path);
    }

    [Fact]
    public void Add_ExceedsMax_TrimsToTen()
    {
        var store = new RecentFiles();
        for (var i = 0; i < 15; i++)
        {
            store.Add($"/tmp/f{i}.md");
        }

        Assert.Equal(10, store.Count);
        Assert.Equal("/tmp/f14.md", store.Entries[0].Path);
        Assert.Equal("/tmp/f5.md", store.Entries[9].Path);
    }

    [Fact]
    public void Remove_RemovesEntry()
    {
        var store = new RecentFiles();
        store.Add("/tmp/a.md");
        Assert.True(store.Remove("/tmp/a.md"));
        Assert.False(store.Contains("/tmp/a.md"));
        Assert.Empty(store);
    }

    [Fact]
    public void Clear_Empties()
    {
        var store = new RecentFiles();
        store.Add("/tmp/a.md");
        store.Clear();
        Assert.Empty(store);
    }

    [Fact]
    public void Entry_DerivesNameAndFolder()
    {
        var entry = new RecentFile("/tmp/foo/bar.md");
        Assert.Equal("bar.md", entry.DisplayName);
        Assert.Equal("/tmp/foo", entry.ParentFolder);
    }

    [Fact]
    public void Order_IsDescendingByLastOperation()
    {
        var store = new RecentFiles();
        var old = DateTime.UtcNow.AddMinutes(-5);
        var newer = DateTime.UtcNow;
        store.Add("/tmp/old.md", lastOperation: old);
        store.Add("/tmp/new.md", lastOperation: newer);
        store.Touch("/tmp/old.md", lastOperation: DateTime.UtcNow.AddMinutes(1));
        Assert.Equal("/tmp/old.md", store.Entries[0].Path);
    }

    [Fact]
    public void Unavailable_CanBeRemoved()
    {
        var store = new RecentFiles();
        store.Add("/tmp/a.md");
        store.MarkUnavailable("/tmp/a.md");
        Assert.True(store.RemoveUnavailable());
        Assert.Empty(store);
    }

    [Fact]
    public void Persistence_RoundTrip()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "recents-test.json");
        try
        {
            var store = new RecentFiles();
            store.Add("/tmp/a.md");
            store.Add("/tmp/b.md");
            store.Save(path);
            var loaded = RecentFiles.Load(path);
            Assert.Equal(2, loaded.Count);
            Assert.Equal("/tmp/b.md", loaded.Entries[0].Path);
        }
        finally
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}