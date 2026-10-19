using EggMdEditor.App.Catalogos;
using Xunit;

namespace EggMdEditor.App.Tests;

// Estructura de los catálogos de sesión (DS-001): idioma, estado del menú y
// aviso flotante, con sus valores controlados y defectos de arranque.
public class CatalogosSesionTests
{
    [Fact]
    public void Catalogo_idiomas_tiene_exactamente_los_valores_del_DS001()
    {
        Assert.Equal(
            [IdiomaDeInterfaz.Español, IdiomaDeInterfaz.English],
            CatalogoIdiomas.Valores);
    }

    [Fact]
    public void Catalogo_idiomas_defecto_es_espanol()
    {
        Assert.Equal(IdiomaDeInterfaz.Español, CatalogoIdiomas.Defecto);
    }

    [Fact]
    public void Catalogo_estado_menu_tiene_exactamente_los_valores_del_DS001()
    {
        Assert.Equal(
            [EstadoMenuPrincipal.Cerrado, EstadoMenuPrincipal.DesplegadoConEntradaActiva],
            CatalogoEstadoMenuPrincipal.Valores);
    }

    [Fact]
    public void Catalogo_estado_menu_defecto_es_cerrado()
    {
        Assert.Equal(EstadoMenuPrincipal.Cerrado, CatalogoEstadoMenuPrincipal.Defecto);
    }

    [Fact]
    public void Sesion_arranca_con_los_defectos_de_arranque_del_DS001()
    {
        var sesion = new SesionDeAplicacion();

        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
        Assert.Equal(EstadoMenuPrincipal.Cerrado, sesion.EstadoMenuPrincipal);
        Assert.False(sesion.AvisoFlotanteVisible);
    }

    [Fact]
    public void Sesion_idiomas_son_mutuamente_excluyentes_valores_distintos()
    {
        Assert.NotEqual(IdiomaDeInterfaz.Español, IdiomaDeInterfaz.English);
    }
}
