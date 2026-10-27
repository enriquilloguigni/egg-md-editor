using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T030AutoguardadoTests
{
    [Fact]
    public void Autoguardado_WithChanges_WritesRecoveryAndKeepsDirty()
    {
        var path = Path.Combine(Path.GetTempPath(), "t030_save.md");
        var recoveryPath = Path.Combine(Path.GetTempPath(), "t030_recovery.json");
        try
        {
            File.WriteAllText(path, "original", new UTF8Encoding(false));
            var doc = new OpenDocument("changed content", path);
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            var beforeContent = File.ReadAllText(path);

            var result = AutoguardadoOperation.Execute(doc, recovery, recoveryPath);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            // user file unchanged
            Assert.Equal(beforeContent, File.ReadAllText(path));
            // document still dirty
            Assert.True(doc.HasChanges);
            // recovery file written
            Assert.True(File.Exists(recoveryPath));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(recoveryPath)) File.Delete(recoveryPath);
        }
    }

    [Fact]
    public void Autoguardado_NoChanges_DoesNotWrite()
    {
        var recoveryPath = Path.Combine(Path.GetTempPath(), "t030_nochanges.json");
        try
        {
            if (File.Exists(recoveryPath)) File.Delete(recoveryPath);
            var doc = new OpenDocument("content");
            // not marked changed
            AutoguardadoOperation.Execute(doc, null, recoveryPath);
            Assert.False(File.Exists(recoveryPath));
        }
        finally
        {
            if (File.Exists(recoveryPath)) File.Delete(recoveryPath);
        }
    }

    [Fact]
    public void Autoguardado_Failure_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "t030_ro_dir");
        var recoveryPath = Path.Combine(dir, "recovery.json");
        Directory.CreateDirectory(dir);
        try
        {
            new DirectoryInfo(dir).Attributes |= FileAttributes.ReadOnly;
            var doc = new OpenDocument("content");
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            Assert.ThrowsAny<Exception>(() => AutoguardadoOperation.Execute(doc, recovery, recoveryPath));
            Assert.True(doc.HasChanges);
        }
        finally
        {
            new DirectoryInfo(dir).Attributes &= ~FileAttributes.ReadOnly;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void AutosaveTimer_Interval_Is30Seconds()
    {
        var timer = new AutosaveTimer();
        Assert.Equal(TimeSpan.FromSeconds(30), timer.Interval);
    }
}