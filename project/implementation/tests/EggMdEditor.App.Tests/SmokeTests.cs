using Xunit;

// Smoke test of the scaffolding (T-001): checks that the solution builds,
// that the test project references the application project and that `dotnet
// test` runs. It does not cover functional behavior (that belongs to
// T-002…T-013).
public class SmokeTests
{
    [Fact]
    public void Application_assembly_is_referenced_and_loadable()
    {
        var appType = typeof(global::EggMdEditor.App.App);

        Assert.True(typeof(Avalonia.Application).IsAssignableFrom(appType));
    }
}
