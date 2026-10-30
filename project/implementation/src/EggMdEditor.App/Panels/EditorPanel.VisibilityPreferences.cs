using System;

namespace EggMdEditor.App;

public partial class EditorPanel
{
    public void LoadPreferences(UiPreferences? prefs)
    {
        if (prefs is null) return;
        if (IsLeft) { Visible = prefs.LeftPanelVisible; IsVisible = prefs.LeftPanelVisible; }
        else { Visible = prefs.RightPanelVisible; IsVisible = prefs.RightPanelVisible; }
    }

    public void ApplyPreferences(UiPreferences? prefs) => LoadPreferences(prefs);

    public UiPreferences ToPreferences() => new UiPreferences { LeftPanelVisible = Visible, RightPanelVisible = Visible };
}

public partial class EditorPanels
{
    public void LoadPreferences(UiPreferences? prefs)
    {
        Left?.LoadPreferences(prefs);
        Right?.LoadPreferences(prefs);
    }
    public void ApplyPreferences(UiPreferences? prefs) => LoadPreferences(prefs);
}