using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Building of SC-001's format bar (T-009) from the bar catalog: exact count
// and writing order of FR-012 (DS-001), one icon (VR-001) and one tooltip
// (VR-002) per control, tooltips i18n resolved in the session's active
// language (Spanish at startup, BR-007) and every control visible with no
// connected action (ADR-001).
public class FormatBarBuilderTests
{
    [Fact]
    public void Builds_the_12_controls_of_FR012_in_writing_order()
    {
        var bar = new StackPanel();

        FormatBarBuilder.Build(bar, SpanishSession());

        Assert.Equal(FormatBarCatalog.TotalControls, bar.Children.Count);
        Assert.Equal(12, bar.Children.Count);
        Assert.Equal(
            FormatBarCatalog.Controls.Select(control => control.Key),
            ControlsOf(bar).Select(control => control.Tag as string));
    }

    [Fact]
    public void Every_catalog_key_has_a_defined_glyph()
    {
        // VR-001 at 100 %: no FR-012 control without an icon.
        Assert.Equal(
            FormatBarCatalog.Controls.Select(control => control.Key).OrderBy(key => key),
            FormatBarBuilder.Glyphs.Keys.OrderBy(key => key));
    }

    [Fact]
    public void Puts_one_glyph_icon_per_control()
    {
        var bar = new StackPanel();

        FormatBarBuilder.Build(bar, SpanishSession());

        var controls = ControlsOf(bar).ToList();
        Assert.All(controls, control =>
        {
            var glyph = Assert.IsType<TextBlock>(control.Content);
            Assert.False(string.IsNullOrEmpty(glyph.Text));
        });
        Assert.Equal(
            FormatBarCatalog.Controls.Select(control => FormatBarBuilder.Glyphs[control.Key]),
            controls.Select(control => ((TextBlock)control.Content!).Text));
    }

    [Fact]
    public void Resolves_every_tooltip_in_the_session_active_language_at_startup()
    {
        // VR-002: each control's name, resolved in the active language.
        var bar = new StackPanel();

        FormatBarBuilder.Build(bar, SpanishSession());

        var controls = ControlsOf(bar).ToList();
        Assert.All(controls, control =>
            Assert.Equal(
                I18nStringCatalog.Resolve((string)control.Tag!, InterfaceLanguage.Spanish),
                ToolTip.GetTip(control) as string));

        // Spot sample of the Spanish startup (BR-007): first and last control.
        Assert.Equal("Énfasis", ToolTip.GetTip(controls[0]) as string);
        Assert.Equal("Índice", ToolTip.GetTip(controls[^1]) as string);
    }

    [Fact]
    public void Resolves_the_tooltips_in_english_when_the_session_starts_in_english()
    {
        var session = SpanishSession();
        session.ActiveLanguage = InterfaceLanguage.English;

        var bar = new StackPanel();
        FormatBarBuilder.Build(bar, session);

        var controls = ControlsOf(bar).ToList();
        Assert.All(controls, control =>
            Assert.Equal(
                I18nStringCatalog.Resolve((string)control.Tag!, InterfaceLanguage.English),
                ToolTip.GetTip(control) as string));
        Assert.Equal("Emphasis", ToolTip.GetTip(controls[0]) as string);
        Assert.Equal("Table of contents", ToolTip.GetTip(controls[^1]) as string);
    }

    [Fact]
    public void Leaves_every_control_visible_with_no_connected_action()
    {
        // ADR-001: appearance of a connected control, with no disabled marks
        // and no command: pressing it does not alter the document.
        var bar = new StackPanel();

        FormatBarBuilder.Build(bar, SpanishSession());

        var controls = ControlsOf(bar).ToList();
        Assert.Equal(FormatBarCatalog.TotalControls, controls.Count);
        Assert.All(controls, control =>
        {
            Assert.True(control.IsEnabled);
            Assert.True(control.IsVisible);
            Assert.Null(control.Command);
        });
    }

    [Fact]
    public void Building_over_an_existing_bar_does_not_duplicate_controls()
    {
        var bar = new StackPanel();

        FormatBarBuilder.Build(bar, SpanishSession());
        FormatBarBuilder.Build(bar, SpanishSession());

        Assert.Equal(FormatBarCatalog.TotalControls, bar.Children.Count);
        Assert.Equal(
            FormatBarCatalog.Controls.Select(control => control.Key),
            ControlsOf(bar).Select(control => control.Tag as string));
    }

    private static ApplicationSession SpanishSession() => new();

    private static IEnumerable<Button> ControlsOf(StackPanel bar) =>
        bar.Children.OfType<Button>();
}
