using EggMdEditor.App;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T029ConfirmacionDescarteTests
{
    [Fact]
    public void ConfirmDiscard_Cancel_KeepsChangesAndRecovery()
    {
        var path = Path.Combine(Path.GetTempPath(), "t029_cancel.md");
        try
        {
            File.WriteAllText(path, "content", new UTF8Encoding(false));
            var doc = new OpenDocument("changed", path);
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.Cancel, recovery);
            Assert.Equal(ConfirmDiscardOption.Cancel, res);
            Assert.True(doc.HasChanges);
            Assert.False(recovery.IsObsolete);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ConfirmDiscard_DoNotSave_InvalidatesRecovery()
    {
        var path = Path.Combine(Path.GetTempPath(), "t029_discard.md");
        try
        {
            File.WriteAllText(path, "content", new UTF8Encoding(false));
            var doc = new OpenDocument("changed", path);
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.DoNotSave, recovery);
            Assert.Equal(ConfirmDiscardOption.DoNotSave, res);
            Assert.True(recovery.IsObsolete);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ConfirmDiscard_Save_WritesAndCleans()
    {
        var path = Path.Combine(Path.GetTempPath(), "t029_save.md");
        try
        {
            File.WriteAllText(path, "old", new UTF8Encoding(false));
            var doc = new OpenDocument("new", path);
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.Save, recovery);
            Assert.Equal(ConfirmDiscardOption.Save, res);
            Assert.Equal("new", File.ReadAllText(path));
            Assert.False(doc.HasChanges);
            Assert.True(recovery.IsObsolete);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ConfirmDiscard_Save_Failure_LeavesChanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "t029_ro_dir");
        var path = Path.Combine(dir, "x.md");
        Directory.CreateDirectory(dir);
        try
        {
            new DirectoryInfo(dir).Attributes |= FileAttributes.ReadOnly;
            var doc = new OpenDocument("content", path);
            doc.MarkChanged();
            var recovery = new RecoveryCopy(doc);
            Assert.ThrowsAny<Exception>(() => ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.Save, recovery));
            Assert.True(doc.HasChanges);
            Assert.False(recovery.IsObsolete);
        }
        finally
        {
            new DirectoryInfo(dir).Attributes &= ~FileAttributes.ReadOnly;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void NeedsConfirmation_Works()
    {
        var doc = new OpenDocument("x");
        Assert.False(ConfirmDiscardOperation.NeedsConfirmation(doc));
        doc.MarkChanged();
        Assert.True(ConfirmDiscardOperation.NeedsConfirmation(doc));
    }
}
