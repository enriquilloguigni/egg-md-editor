using Xunit;

namespace EggMdEditor.App.Tests;

// Prueba de humo del andamiaje (T-001): verifica que la solución compila, que el
// proyecto de pruebas referencia al proyecto de aplicación y que `dotnet test`
// se ejecuta. No cubre comportamiento funcional (eso corresponde a T-002…T-013).
public class SmokeTests
{
    [Fact]
    public void Application_assembly_is_referenced_and_loadable()
    {
        var appType = typeof(global::EggMdEditor.App.App);

        Assert.True(typeof(Avalonia.Application).IsAssignableFrom(appType));
    }
}
