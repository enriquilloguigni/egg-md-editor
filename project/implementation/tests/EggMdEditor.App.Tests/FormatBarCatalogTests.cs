using EggMdEditor.App.Catalogs;
using Xunit;

namespace EggMdEditor.App.Tests;

// Exact, ordered structure of the format bar catalog (FR-012, DS-001), per
// SC-001's canonical enumeration: the controls follow the writing flow
// order; 100 % of this list feeds the count of T-009/T-013: there can be no
// extra or missing controls.
public class FormatBarCatalogTests
{
    [Fact]
    public void Has_the_12_controls_of_FR012_in_writing_order()
    {
        Assert.Equal(
            [
                "barra.enfasis",
                "barra.encabezados",
                "barra.listas",
                "barra.cita",
                "barra.codigo",
                "barra.enlace",
                "barra.imagen",
                "barra.tabla",
                "barra.nota-al-pie",
                "barra.formula",
                "barra.diagrama",
                "barra.indice",
            ],
            FormatBarCatalog.Controls.Select(control => control.Key));
    }

    [Fact]
    public void Total_controls_is_100_percent_of_FR012()
    {
        Assert.Equal(12, FormatBarCatalog.TotalControls);
    }

    [Fact]
    public void The_position_in_writing_order_identifies_each_control()
    {
        Assert.Equal("barra.enfasis", FormatBarCatalog.Controls[0].Key);
        Assert.Equal("barra.encabezados", FormatBarCatalog.Controls[1].Key);
        Assert.Equal("barra.indice", FormatBarCatalog.Controls[^1].Key);
    }

    [Fact]
    public void Control_keys_are_unique()
    {
        var keys = FormatBarCatalog.Controls.Select(control => control.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }
}
