using Avalonia.Headless.XUnit;
using EggMdEditor.App.I18n;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T033TitleChangeIndicatorTests
{
    [Fact]
    public void WindowTitle_NewDocument_ShowsSinTitulo()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        Assert.Equal("Egg MD Editor — Sin título", vm.WindowTitle);
        Assert.Equal(string.Empty, vm.ChangeIndicator);
        Assert.False(vm.HasChangeIndicator);
    }

    [Fact]
    public void WindowTitle_DocumentWithName_Updates()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        var doc = new OpenDocument("x", "/tmp/foo.md");
        vm.Document = doc;
        Assert.Equal("Egg MD Editor — foo.md", vm.WindowTitle);
    }

    [Fact]
    public void WindowTitle_WhenDirty_ShowsIndicator()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        var doc = new OpenDocument("x", "/tmp/foo.md");
        vm.Document = doc;
        doc.MarkChanged();
        Assert.Equal("Egg MD Editor — foo.md •", vm.WindowTitle);
        Assert.Equal("•", vm.ChangeIndicator);
        Assert.True(vm.HasChangeIndicator);
    }

    [Fact]
    public void WindowTitle_WhenClean_HidesIndicator()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        var doc = new OpenDocument("x", "/tmp/foo.md");
        doc.MarkChanged();
        vm.Document = doc;
        doc.MarkClean();
        Assert.Equal("Egg MD Editor — foo.md", vm.WindowTitle);
        Assert.Equal(string.Empty, vm.ChangeIndicator);
    }

    [AvaloniaFact]
    public void Window_Title_BindsToViewModel()
    {
        var vm = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(vm);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Assert.Equal(vm.WindowTitle, window.Title);
    }
}
