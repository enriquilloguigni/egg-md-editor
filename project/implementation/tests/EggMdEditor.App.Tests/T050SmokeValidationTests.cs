using EggMdEditor.App;
using System.IO;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T050SmokeValidationTests
{
    [Fact]
    public void RoundTrip_And_Defaults()
    {
        var path = Path.Combine(Path.GetTempPath(), "t050_prefs.json");
        try
        {
            if (File.Exists(path)) File.Delete(path);
            var loadedDefault = UiPreferences.Load(path);
            Assert.True(loadedDefault.LeftPanelVisible);
            Assert.True(loadedDefault.RightPanelVisible);
            Assert.True(loadedDefault.FormatBarVisible);

            var prefs = new UiPreferences();
            prefs.LeftPanelVisible = false;
            prefs.RightPanelVisible = true;
            prefs.FormatBarVisible = false;
            prefs.SplitterWidth = 7.5;
            UiPreferences.Save(prefs, path);

            var loaded = UiPreferences.Load(path);
            Assert.False(loaded.LeftPanelVisible);
            Assert.True(loaded.RightPanelVisible);
            Assert.False(loaded.FormatBarVisible);
            Assert.Equal(7.5, loaded.SplitterWidth);

            // service
            var svc = new UiPreferencesStore(path);
            Assert.True(svc.Save(prefs));
            var loaded2 = svc.Load();
            Assert.False(loaded2.LeftPanelVisible);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
