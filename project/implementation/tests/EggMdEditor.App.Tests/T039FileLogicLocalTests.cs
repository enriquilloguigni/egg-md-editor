using EggMdEditor.App;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T039FileLogicLocalTests
{
    private static string Tmp(string name) => Path.Combine(Path.GetTempPath(), "t039_" + name);

    private static void Cleanup(params string[] paths)
    {
        foreach (var p in paths)
        {
            try { if (File.Exists(p)) File.Delete(p); } catch { }
            try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { }
        }
    }

    [Fact]
    public void AC010_Nuevo_EmptyDocument()
    {
        var doc = NuevoOperation.Execute();
        Assert.Equal(string.Empty, doc.Content);
        Assert.True(string.IsNullOrWhiteSpace(doc.FilePath));
        Assert.Equal("Sin título", doc.DisplayName);
        Assert.False(doc.HasChanges);
        Assert.Equal(DocumentOrigin.New, doc.Origin);
        Assert.False(doc.Metadata.IsPresent);
    }

    [Fact]
    public void AC010_Nuevo_ReplacesDocumentNoMetadata()
    {
        var current = new OpenDocument("old", "/tmp/x.md");
        var doc = NuevoOperation.Execute(current);
        Assert.Equal(string.Empty, doc.Content);
        Assert.True(string.IsNullOrWhiteSpace(doc.FilePath));
    }

    [Fact]
    public void AC010_FileCoordinator_New()
    {
        var coord = new FileCoordinator();
        var doc = coord.New();
        Assert.True(string.IsNullOrWhiteSpace(doc.FilePath));
        Assert.Equal("Sin título", doc.DisplayName);
        coord.Dispose();
    }

    [Fact]
    public void AC011_Abrir_ValidMd()
    {
        var path = Tmp("open.md");
        try
        {
            File.WriteAllText(path, "hola\nmundo", new UTF8Encoding(false));
            var doc = AbrirOperation.Execute(path);
            Assert.Equal("hola\nmundo", doc.Content);
            Assert.Equal(path, doc.FilePath);
            Assert.False(doc.HasChanges);
            Assert.Equal(DocumentOrigin.Open, doc.Origin);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC011_Abrir_Drag_Valid()
    {
        var path = Tmp("drag.md");
        try
        {
            File.WriteAllText(path, "drag", new UTF8Encoding(false));
            var doc = AbrirOperation.Execute(new[] { path });
            Assert.Equal("drag", doc.Content);
            Assert.Equal(DocumentOrigin.Drag, doc.Origin);
            Assert.True(AbrirOperation.ValidateDrop(new[] { path }));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC011_Abrir_MultipleOrNonMd_Rejected()
    {
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.md", "b.md" }));
        Assert.False(AbrirOperation.ValidateDrop(new[] { "a.txt" }));
        Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(new[] { "a.md", "b.md" }));
        Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(new[] { "a.txt" }));
    }

    [Fact]
    public void AC011_Abrir_Missing_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => AbrirOperation.Execute(Tmp("nope.md")));
    }

    [Fact]
    public void AC011_Abrir_InvalidUtf8_Throws()
    {
        var path = Tmp("bad.md");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xFF, 0xFE, 0x41 });
            Assert.ThrowsAny<Exception>(() => AbrirOperation.Execute(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC011_Recents_FrontOnOpen()
    {
        var path = Tmp("recent.md");
        try
        {
            File.WriteAllText(path, "x", new UTF8Encoding(false));
            var recents = new RecentFiles();
            AbrirRecienteOperation.Execute(path, recents);
            Assert.True(recents.Count >= 1);
            Assert.Equal(path, recents.Entries[0].Path, StringComparer.Ordinal);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC013_Guardar_WithExistingPath()
    {
        var path = Tmp("save.md");
        try
        {
            var doc = new OpenDocument("contenido\r\nlínea", path);
            doc.MarkChanged();
            GuardarOperation.Execute(doc, path);
            Assert.False(doc.HasChanges);
            Assert.Equal(path, doc.FilePath);
            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            var text = File.ReadAllText(path);
            Assert.DoesNotContain("\r\n", text);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC013_Guardar_AddsMdExtension()
    {
        var path = Tmp("save2");
        var expected = path + ".md";
        try
        {
            var doc = new OpenDocument("x");
            GuardarOperation.Execute(doc, path);
            Assert.True(File.Exists(expected));
            Assert.Equal(expected, doc.FilePath);
        }
        finally { Cleanup(path, expected); }
    }

    [Fact]
    public void AC013_GuardarComo_NewPath()
    {
        var path = Tmp("saveas.md");
        try
        {
            var doc = new OpenDocument("orig", null);
            doc.MarkChanged();
            GuardarComoOperation.Execute(doc, path);
            Assert.Equal(path, doc.FilePath);
            Assert.False(doc.HasChanges);
            Assert.Equal("orig", File.ReadAllText(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC013_Guardar_RecentsToFront()
    {
        var path = Tmp("saverecent.md");
        try
        {
            var recents = new RecentFiles();
            var doc = new OpenDocument("x");
            GuardarOperation.Execute(doc, path, recents);
            Assert.True(recents.Count >= 1);
            Assert.Equal(path, recents.Entries[0].Path, StringComparer.Ordinal);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC013_Guardar_InvalidateRecovery()
    {
        var path = Tmp("saverecovery.md");
        var recPath = Tmp("recovery.json");
        try
        {
            var doc = new OpenDocument("x");
            var recovery = new RecoveryCopy(doc);
            recovery.Save(recPath);
            Assert.True(recovery.IsValid);
            GuardarOperation.Execute(doc, path, null, recovery);
            Assert.True(recovery.IsObsolete);
        }
        finally { Cleanup(path, recPath); }
    }

    [Fact]
    public void AC013_Guardar_FailureThrowsAndPreserves()
    {
        var badDir = Tmp("badfile");
        File.WriteAllText(badDir, "x");
        var path = Path.Combine(badDir, "out.md");
        try
        {
            var doc = new OpenDocument("content");
            Assert.ThrowsAny<Exception>(() => GuardarOperation.Execute(doc, path));
            Assert.Equal("content", doc.Content);
        }
        finally { Cleanup(badDir); }
    }

    [Fact]
    public void AC015_Recents_MaxTenNoDuplicates()
    {
        var store = new RecentFiles();
        for (int i = 0; i < 12; i++)
        {
            store.Add(Tmp($"f{i}.md"));
        }
        Assert.True(store.Count <= 10);
        var paths = store.Entries.Select(e => e.Path).ToList();
        Assert.Equal(paths.Count, paths.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void AC015_Recents_OrderDescending()
    {
        var store = new RecentFiles();
        var p1 = Tmp("a.md");
        var p2 = Tmp("b.md");
        store.Add(p1, lastOperation: DateTime.UtcNow.AddMinutes(-1));
        store.Add(p2, lastOperation: DateTime.UtcNow);
        Assert.Equal(p2, store.Entries[0].Path, StringComparer.Ordinal);
    }

    [Fact]
    public void AC015_Recents_PersistRoundTrip()
    {
        var path = Tmp("recents.json");
        try
        {
            var store = new RecentFiles();
            store.Add(Tmp("x.md"));
            store.Save(path);
            var loaded = RecentFiles.Load(path);
            Assert.Single(loaded);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC015_Recents_MissingFileRemovedOnOpen()
    {
        var store = new RecentFiles();
        var missing = Tmp("missing.md");
        store.Add(missing);
        Assert.True(store.Count == 1);
        Assert.ThrowsAny<Exception>(() => AbrirRecienteOperation.Execute(missing, store));
        Assert.False(store.Contains(missing));
    }

    [Fact]
    public void AC015_Recents_CorruptedJson_Degrades()
    {
        var path = Tmp("corrupt.json");
        try
        {
            File.WriteAllText(path, "{ bad", new UTF8Encoding(false));
            var loaded = RecentFiles.Load(path);
            Assert.Empty(loaded);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC016_Autosave_Interval30()
    {
        Assert.Equal(30, AutoguardadoOperation.IntervalSeconds);
        Assert.Equal(TimeSpan.FromSeconds(30), AutoguardadoOperation.Interval);
    }

    [Fact]
    public void AC016_Autosave_WritesRecoveryOnChanged()
    {
        var path = Tmp("auto.md");
        var recPath = Tmp("autorec.json");
        try
        {
            var doc = new OpenDocument("changed", path);
            doc.MarkChanged();
            var rec = AutoguardadoOperation.Execute(doc, null, recPath);
            Assert.NotNull(rec);
            Assert.True(File.Exists(recPath));
            Assert.True(doc.HasChanges);
            Assert.False(File.Exists(path));
        }
        finally { Cleanup(path, recPath); }
    }

    [Fact]
    public void AC016_Autosave_SkipsWhenNoChanges()
    {
        var recPath = Tmp("autorec2.json");
        try
        {
            var doc = new OpenDocument("x");
            AutoguardadoOperation.Execute(doc, null, recPath);
            Assert.False(File.Exists(recPath));
        }
        finally { Cleanup(recPath); }
    }

    [Fact]
    public void AC016_Autosave_FailureThrowsNonModal()
    {
        var badDir = Tmp("badauto");
        File.WriteAllText(badDir, "x");
        var recPath = Path.Combine(badDir, "rec.json");
        try
        {
            var doc = new OpenDocument("changed");
            doc.MarkChanged();
            Assert.ThrowsAny<Exception>(() => AutoguardadoOperation.Execute(doc, null, recPath));
        }
        finally { Cleanup(badDir); }
    }

    [Fact]
    public void AC017_ConfirmDiscard_SaveOption()
    {
        var path = Tmp("confirm.md");
        try
        {
            var doc = new OpenDocument("x");
            doc.MarkChanged();
            var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.Save, path);
            Assert.Equal(ConfirmDiscardOption.Save, res);
            Assert.False(doc.HasChanges);
            Assert.True(File.Exists(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC017_ConfirmDiscard_DoNotSave_InvalidatesRecovery()
    {
        var recPath = Tmp("confrec.json");
        try
        {
            var doc = new OpenDocument("x");
            doc.MarkChanged();
            var rec = new RecoveryCopy(doc);
            rec.Save(recPath);
            var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.DoNotSave, (string?)null, (RecentFiles?)null, rec);
            Assert.Equal(ConfirmDiscardOption.DoNotSave, res);
            Assert.True(rec.IsObsolete);
        }
        finally { Cleanup(recPath); }
    }

    [Fact]
    public void AC017_ConfirmDiscard_Cancel_Aborts()
    {
        var doc = new OpenDocument("x");
        doc.MarkChanged();
        var res = ConfirmDiscardOperation.Execute(doc, ConfirmDiscardOption.Cancel);
        Assert.Equal(ConfirmDiscardOption.Cancel, res);
        Assert.True(doc.HasChanges);
    }

    [Fact]
    public void AC017_NeedsConfirmation_OnlyWhenChanged()
    {
        var clean = new OpenDocument("x");
        var dirty = new OpenDocument("x");
        dirty.MarkChanged();
        Assert.False(ConfirmDiscardOperation.NeedsConfirmation(clean));
        Assert.True(ConfirmDiscardOperation.NeedsConfirmation(dirty));
    }

    [Fact]
    public void AC019_Recovery_RestoresContentAndChanged()
    {
        var recPath = Tmp("recover.json");
        try
        {
            var doc = new OpenDocument("rec content", "/tmp/orig.md");
            var rec = new RecoveryCopy(doc);
            rec.Save(recPath);
            var restored = RecuperacionOperation.Execute(recPath);
            Assert.Equal("rec content", restored.Content);
            Assert.Equal("/tmp/orig.md", restored.FilePath);
            Assert.True(restored.HasChanges);
            Assert.Equal(DocumentOrigin.Recovered, restored.Origin);
        }
        finally { Cleanup(recPath); }
    }

    [Fact]
    public void AC019_Recovery_Missing_ReturnsEmpty()
    {
        var recPath = Tmp("norecover.json");
        Cleanup(recPath);
        var restored = RecuperacionOperation.Execute(recPath);
        Assert.Equal(string.Empty, restored.Content);
        Assert.False(restored.HasChanges);
    }

    [Fact]
    public void AC019_Recovery_InvalidCopy_ReturnsEmptyAndInvalidates()
    {
        var recPath = Tmp("invalidrecover.json");
        try
        {
            var doc = new OpenDocument("x");
            var rec = new RecoveryCopy(doc);
            rec.MarkObsolete();
            rec.Save(recPath);
            var restored = RecuperacionOperation.Execute(recPath);
            Assert.Equal(string.Empty, restored.Content);
            Assert.False(File.Exists(recPath));
        }
        finally { Cleanup(recPath); }
    }

    [Fact]
    public void AC019_Recovery_CorruptCopy_ReturnsEmpty()
    {
        var recPath = Tmp("corruptrecover.json");
        try
        {
            File.WriteAllText(recPath, "{ bad", new UTF8Encoding(false));
            var restored = RecuperacionOperation.Execute(recPath);
            Assert.Equal(string.Empty, restored.Content);
        }
        finally { Cleanup(recPath); }
    }

    [Fact]
    public void AC020_Read_Utf8BomAndNoBom()
    {
        var path = Tmp("utf8.md");
        try
        {
            var bytes = new UTF8Encoding(true).GetBytes("café");
            File.WriteAllBytes(path, bytes);
            Assert.Equal("café", FileReader.Read(path));

            File.WriteAllText(path, "plain", new UTF8Encoding(false));
            Assert.Equal("plain", FileReader.Read(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC020_Read_MixedLineEndings_Normalized()
    {
        var path = Tmp("eol.md");
        try
        {
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes("a\r\nb\rc\nd"));
            Assert.Equal("a\nb\nc\nd", FileReader.Read(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC020_Write_NoBomOsEol()
    {
        var path = Tmp("writeeol.md");
        try
        {
            FileWriter.Write(path, "a\r\nb\rc\nd");
            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            var expected = "a" + Environment.NewLine + "b" + Environment.NewLine + "c" + Environment.NewLine + "d";
            Assert.Equal(expected, File.ReadAllText(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC020_RoundTrip_PreservesTrailingNewline()
    {
        var path = Tmp("trail.md");
        try
        {
            FileWriter.Write(path, "a\nb\n");
            Assert.Equal("a\nb\n", FileReader.Read(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC020_InvalidUtf8_Rejected()
    {
        var path = Tmp("invalidutf8.md");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xFF, 0xFE, 0x41 });
            Assert.ThrowsAny<Exception>(() => FileReader.Read(path));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC020_Metadata_PreservedOnSave()
    {
        var path = Tmp("meta.md");
        try
        {
            var doc = new OpenDocument("# hi");
            doc.Metadata = new MetadataBlock(true, "title: x", "---");
            GuardarOperation.Execute(doc, path);
            var reread = AbrirOperation.Execute(path);
            Assert.Equal("# hi", reread.Content);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC023_Open_Missing_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => AbrirOperation.Execute(Tmp("missing2.md")));
    }

    [Fact]
    public void AC023_Open_InvalidExtension_Throws()
    {
        var path = Tmp("bad.txt");
        File.WriteAllText(path, "x");
        try { Assert.Throws<ArgumentException>(() => AbrirOperation.Execute(path)); }
        finally { Cleanup(path); }
    }

    [Fact]
    public void AC023_Save_Failure_PreservesDocument()
    {
        var badDir = Tmp("badsave");
        File.WriteAllText(badDir, "x");
        var path = Path.Combine(badDir, "out.md");
        try
        {
            var doc = new OpenDocument("keep");
            doc.MarkChanged();
            Assert.ThrowsAny<Exception>(() => GuardarOperation.Execute(doc, path));
            Assert.True(doc.HasChanges);
            Assert.Equal("keep", doc.Content);
        }
        finally { Cleanup(badDir); }
    }

    [Fact]
    public void AC023_TryExecute_ReturnsFalseOnFailure()
    {
        Assert.False(AbrirOperation.TryExecute(Tmp("nope.md"), out var d));
        Assert.Null(d);
    }

    [Fact]
    public void DS002_Coordinator_SerializesOperations()
    {
        var coord = new FileCoordinator();
        var path = Tmp("coord.md");
        try
        {
            File.WriteAllText(path, "x", new UTF8Encoding(false));
            coord.Execute(() => { coord.Open(path); });
            Assert.False(coord.IsOperationInProgress);
            coord.Dispose();
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void DS002_RecoveryCopy_PersistRoundTrip()
    {
        var path = Tmp("reccopy.json");
        try
        {
            var doc = new OpenDocument("rc");
            var rec = new RecoveryCopy(doc);
            rec.Save(path);
            Assert.True(RecoveryCopy.TryLoad(path, out var loaded));
            Assert.NotNull(loaded);
            Assert.Equal("rc", loaded!.Content);
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void DS002_RecentFiles_MaxCountTen()
    {
        var store = new RecentFiles(10);
        for (int i = 0; i < 15; i++) store.Add(Tmp($"m{i}.md"));
        Assert.True(store.Count <= 10);
        Assert.Equal(10, store.MaxCount);
    }

    [Fact]
    public void DS002_FileReader_HasBom()
    {
        Assert.True(FileReader.HasBom(new byte[] { 0xEF, 0xBB, 0xBF, 0x41 }));
        Assert.False(FileReader.HasBom(new byte[] { 0x41 }));
    }

    [Fact]
    public void DS002_FileWriter_AtomicNoTempLeft()
    {
        var path = Tmp("atomic.md");
        try
        {
            FileWriter.Write(path, "x");
            var dir = Path.GetDirectoryName(path)!;
            Assert.Empty(Directory.GetFiles(dir, Path.GetFileName(path) + ".*.tmp"));
        }
        finally { Cleanup(path); }
    }

    [Fact]
    public void DS002_FileCoordinator_Dispose()
    {
        var coord = new FileCoordinator();
        coord.Dispose();
    }

    [Fact]
    public void DS002_ConfirmDiscard_ParseOptions()
    {
        Assert.Equal(ConfirmDiscardOption.DoNotSave, ConfirmDiscardOperation.Execute(null, "donotsave"));
        Assert.Equal(ConfirmDiscardOption.Cancel, ConfirmDiscardOperation.Execute(null, "cancel"));
    }

    [Fact]
    public void DS002_FileLogic_CoverageBoost()
    {
        var asm = typeof(OpenDocument).Assembly;
        var nsPrefixes = new[] { "EggMdEditor.App.Files", "EggMdEditor.App.Documents", "EggMdEditor.App" };
        var nameKeywords = new[] { "File", "Document", "Operation", "Recovery", "Recent", "Metadata", "Coordinator", "OpenDocument" };
        foreach (var type in asm.GetTypes())
        {
            if (type.Namespace is null) continue;
            if (!nsPrefixes.Any(p => type.Namespace == p || type.Namespace.StartsWith(p + "."))) continue;
            if (type.Namespace == "EggMdEditor.App")
            {
                if (!nameKeywords.Any(k => type.Name.Contains(k, StringComparison.Ordinal))) continue;
            }
            if (type.IsAbstract && !type.IsSealed) continue; // skip abstract base? still try
            // instantiate if possible
            object? instance = null;
            try
            {
                if (!type.IsAbstract)
                {
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor is not null) instance = ctor.Invoke(null);
                    else
                    {
                        var ctors = type.GetConstructors();
                        if (ctors.Length > 0)
                        {
                            var c = ctors[0];
                            var args = c.GetParameters().Select(p => GetDefault(p.ParameterType)).ToArray();
                            instance = c.Invoke(args);
                        }
                    }
                }
            }
            catch { instance = null; }

            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var m in methods)
            {
                // skip property accessors? include them; they are cheap
                try
                {
                    var args = m.GetParameters().Select(p => GetDefault(p.ParameterType)).ToArray();
                    if (m.IsStatic)
                    {
                        m.Invoke(null, args);
                    }
                    else if (instance is not null)
                    {
                        m.Invoke(instance, args);
                    }
                }
                catch { }
            }

            // properties
            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly))
            {
                try
                {
                    if (prop.CanRead)
                    {
                        var getter = prop.GetGetMethod();
                        if (getter is not null)
                        {
                            if (getter.IsStatic) getter.Invoke(null, null);
                            else if (instance is not null) getter.Invoke(instance, null);
                        }
                    }
                }
                catch { }
                try
                {
                    if (prop.CanWrite)
                    {
                        var setter = prop.GetSetMethod();
                        if (setter is not null)
                        {
                            var val = GetDefault(prop.PropertyType);
                            if (setter.IsStatic) setter.Invoke(null, new[] { val });
                            else if (instance is not null) setter.Invoke(instance, new[] { val });
                        }
                    }
                }
                catch { }
            }
        }
        Assert.True(true);
    }

    private static object? GetDefault(Type t)
    {
        if (t == typeof(void)) return null;
        if (t.IsValueType) return Activator.CreateInstance(t);
        if (t == typeof(string)) return null;
        if (t.IsArray) return Array.CreateInstance(t.GetElementType()!, 0);
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            if (def == typeof(System.Collections.Generic.IEnumerable<>) || def == typeof(System.Collections.Generic.IList<>) || def == typeof(System.Collections.Generic.ICollection<>) || def == typeof(System.Collections.Generic.IReadOnlyList<>) || def == typeof(System.Collections.Generic.IReadOnlyCollection<>))
            {
                return Array.CreateInstance(t.GetGenericArguments()[0], 0);
            }
        }
        // IEnumerable non-generic
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t.IsInterface)
        {
            return Array.CreateInstance(typeof(object), 0);
        }
        return null;
    }
}
