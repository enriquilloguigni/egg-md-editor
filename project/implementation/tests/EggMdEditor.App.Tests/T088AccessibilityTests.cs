using EggMdEditor.App;
using Xunit;
using System;
using System.Linq;

namespace EggMdEditor.App.Tests;

public class T088AccessibilityTests
{
    [Fact]
    public void Accessibility_Defaults()
    {
        var a = new Accessibility();
        Assert.True(a.FocusVisible);
        Assert.True(a.KeyboardNavigable);
        Assert.True(a.ScreenReaderCompatible);
        Assert.True(a.WCAGCompliant);
        Assert.True(a.AACompliant);
        Assert.Equal(4.5, a.MinContrast);
        Assert.True(a.HasLandmarks);
        Assert.True(a.HasRoles);
        Assert.True(a.MeetsContrast("#000000", "#FFFFFF"));
    }

    [Fact]
    public void Accessibility_Contrast()
    {
        Assert.True(Accessibility.MeetsWCAG("#000000", "#FFFFFF"));
        Assert.True(Accessibility.ContrastRatio("#000000", "#FFFFFF") > 20);
        Assert.True(new Accessibility().ContrastConforms("#000000", "#FFFFFF"));
    }

    [Fact]
    public void Accessibility_Landmarks()
    {
        var a = new Accessibility();
        Assert.Contains("main", a.Landmarks);
        Assert.True(a.HasLandmark("main"));
        a.AddLandmark("region");
        Assert.True(a.HasLandmark("region"));
    }

    [Fact]
    public void DialogViewModel_Accessibility_Defaults()
    {
        var d = new DialogViewModel();
        Assert.True(d.FocusVisible);
        Assert.True(d.KeyboardNavigable);
        Assert.True(d.ScreenReaderCompatible);
        Assert.Equal("dialog", d.Role);
        Assert.True(d.IsModal);
        Assert.True(d.WCAGCompliant);
    }

    [Fact]
    public void TableRenderer_Accessibility_Defaults()
    {
        var t = new TableRenderer();
        Assert.True(t.FocusVisible);
        Assert.True(t.KeyboardNavigable);
        Assert.True(t.ScreenReaderCompatible);
        Assert.Equal("grid", t.Role);
        Assert.True(t.WCAGCompliant);
    }

    [Fact]
    public void FormulaRenderer_Accessibility_Defaults()
    {
        var f = new FormulaRenderer();
        Assert.True(f.FocusVisible);
        Assert.True(f.KeyboardNavigable);
        Assert.True(f.ScreenReaderCompatible);
        Assert.Equal("math", f.Role);
        Assert.True(f.WCAGCompliant);
    }

    [Fact]
    public void DiagramRenderer_Accessibility_Defaults()
    {
        var d = new DiagramRenderer();
        Assert.True(d.FocusVisible);
        Assert.True(d.KeyboardNavigable);
        Assert.True(d.ScreenReaderCompatible);
        Assert.Equal("img", d.Role);
        Assert.True(d.WCAGCompliant);
    }

    [Fact]
    public void CodeHighlighter_Accessibility_Defaults()
    {
        var c = new CodeHighlighter();
        Assert.True(c.FocusVisible);
        Assert.True(c.KeyboardNavigable);
        Assert.True(c.ScreenReaderCompatible);
        Assert.Equal("code", c.Role);
        Assert.True(c.WCAGCompliant);
    }

    [Fact]
    public void AliasClasses_Work()
    {
        Assert.NotNull(new DialogAccessibility());
        Assert.NotNull(new ControlAccessibility());
        Assert.NotNull(new PreviewAccessibility());
        Assert.NotNull(new Landmark());
        Assert.NotNull(new Landmarks());
    }

    [Fact]
    public void ReducedMotion()
    {
        var a = new Accessibility();
        a.PrefersReducedMotion = true;
        Assert.True(a.PrefersReducedMotion);
        Assert.Equal(0, a.ReducedMotionDuration(100));
        a.PrefersReducedMotion = false;
    }
}