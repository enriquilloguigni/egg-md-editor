using Avalonia.Headless.XUnit;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

// T-012 (OP-006, TR-001): el cierre de SC-001 finaliza la sesión (ST-001 →
// ST-002) por cualquier vía —atajo o barra de título— y sin estado
// transitorio pendiente.
public class WindowLifecycleTests
{
    [Fact]
    public void ClosingTheWindowEndsTheSessionAndClearsTransientState()
    {
        var session = new Session(new I18nCatalog());
        session.OpenMenu();
        session.ShowNotice();

        session.CloseWindow();

        Assert.False(session.WindowOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.False(session.NoticeVisible);
    }

    [Fact]
    public void ClosingTheSessionTwiceIsIdempotent()
    {
        var session = new Session(new I18nCatalog());
        session.CloseWindow();
        session.CloseWindow();

        Assert.False(session.WindowOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.False(session.NoticeVisible);
    }

    [AvaloniaFact]
    public void ClosingTheWindowFromTheTitleBarEndsTheSession()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);
        window.Show();
        session.OpenMenu();
        session.ShowNotice();

        window.Close();

        Assert.False(session.WindowOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.False(session.NoticeVisible);
    }

    [AvaloniaFact]
    public void TheCloseShortcutClosesTheWindowAndEndsTheSession()
    {
        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel);
        window.Show();
        session.OpenMenu();
        session.ShowNotice();

        Assert.True(viewModel.DispatchShortcut(ShortcutNames.Close));

        Assert.False(window.IsVisible);
        Assert.False(session.WindowOpen);
        Assert.Equal(MenuState.Closed, session.MenuState);
        Assert.False(session.NoticeVisible);
    }
}
