using EggMdEditor.App.Catalogos;
using Xunit;

namespace EggMdEditor.App.Tests;

// Estructura exacta y ordenada del catálogo de la barra de formato (FR-012,
// DS-001), según la enumeración canónica de SC-001: los controles siguen el
// orden del flujo de escritura; el 100 % de esta lista alimenta el recuento
// de T-009/T-013: no puede haber controles de más ni ausentes.
public class CatalogoBarraFormatoTests
{
    [Fact]
    public void Tiene_los_12_controles_de_FR012_en_orden_de_escritura()
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
            CatalogoBarraFormato.Controles.Select(control => control.Clave));
    }

    [Fact]
    public void Total_controles_es_el_100_por_ciento_de_FR012()
    {
        Assert.Equal(12, CatalogoBarraFormato.TotalControles);
    }

    [Fact]
    public void La_posicion_en_el_orden_de_escritura_identifica_cada_control()
    {
        Assert.Equal("barra.enfasis", CatalogoBarraFormato.Controles[0].Clave);
        Assert.Equal("barra.encabezados", CatalogoBarraFormato.Controles[1].Clave);
        Assert.Equal("barra.indice", CatalogoBarraFormato.Controles[^1].Clave);
    }

    [Fact]
    public void Las_claves_de_los_controles_son_unicas()
    {
        var claves = CatalogoBarraFormato.Controles.Select(control => control.Clave).ToList();

        Assert.Equal(claves.Count, claves.Distinct().Count());
    }
}
