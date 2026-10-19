using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using EggMdEditor.App.Catalogos;
using Xunit;

namespace EggMdEditor.App.Tests;

// Área de documento de SC-001 vacía en F-001 (T-010, AC-015): la superficie
// solo muestra la barra de menú y la barra de formato; el área de documento
// queda sin vistas, paneles ni contenido (FR-001 → F-003, FR-004 → F-002);
// sobre ella se pintan los avisos flotantes.
public class AreaDeDocumentoTests
{
    [Fact]
    public void El_area_de_documento_esta_vacia_sin_vistas_ni_contenido()
    {
        var ventana = new MainWindow(new SesionDeAplicacion());

        var area = ventana.FindControl<Border>("AreaDeDocumento");

        Assert.NotNull(area);
        Assert.Null(area!.Child);
        Assert.Empty(((ILogical)area).LogicalChildren);
        Assert.Empty(area.GetVisualChildren());
    }

    [Fact]
    public void La_ventana_solo_muestra_menu_barra_formato_y_area_de_documento()
    {
        var ventana = new MainWindow(new SesionDeAplicacion());

        var raiz = Assert.IsType<DockPanel>(ventana.Content);

        // Z2 barra de menú, Z3 barra de formato y Z4 área de documento: sin
        // paneles ni vistas de más (AC-015, BR-F001-008).
        Assert.Equal(3, raiz.Children.Count);
        Assert.IsType<Menu>(raiz.Children[0]);
        Assert.IsType<StackPanel>(raiz.Children[1]);
        Assert.IsType<Border>(raiz.Children[2]);
    }
}
