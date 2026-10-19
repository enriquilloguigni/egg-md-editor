using EggMdEditor.App.Catalogos;
using Xunit;

namespace EggMdEditor.App.Tests;

// Catálogo de cadenas i18n (DS-001): toda cadena visible de SC-001 se
// resuelve desde aquí en el idioma activo (BR-F001-007), con defecto español
// (BR-F001-004) y fallback español ante claves sin resolver en el idioma
// destino (ERR-004). La coherencia con las claves publicadas por los
// catálogos de menú (T-002) y de barra (T-002) se verifica frente a sus
// listas: no puede haber claves de más ni ausentes.
public class CatalogoCadenasI18nTests
{
    [Fact]
    public void El_idioma_defecto_es_espanol()
    {
        Assert.Equal("es", CatalogoCadenasI18n.CodigoDefecto);
        Assert.Equal("es", CatalogoCadenasI18n.CodigoDe(IdiomaDeInterfaz.Español));
        Assert.Equal("en", CatalogoCadenasI18n.CodigoDe(IdiomaDeInterfaz.English));
    }

    [Fact]
    public void Todo_valor_de_idioma_tiene_codigo_y_el_desconocido_cae_en_el_defecto()
    {
        Assert.All(
            CatalogoIdiomas.Valores,
            idioma => Assert.False(string.IsNullOrWhiteSpace(
                CatalogoCadenasI18n.CodigoDe(idioma))));

        Assert.Equal(
            CatalogoCadenasI18n.CodigoDefecto,
            CatalogoCadenasI18n.CodigoDe((IdiomaDeInterfaz)99));
    }

    [Fact]
    public void Cubre_el_100_por_ciento_de_las_claves_publicadas_por_los_catalogos()
    {
        var clavesPublicadas = ClavesPublicadasPorLosCatalogos()
            .OrderBy(clave => clave, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(6 + 48 + 12, clavesPublicadas.Count);
        Assert.Equal(
            clavesPublicadas,
            CatalogoCadenasI18n.TextosPorIdioma[CatalogoCadenasI18n.CodigoDefecto]
                .Keys.OrderBy(clave => clave, StringComparer.Ordinal));
        Assert.Equal(
            clavesPublicadas,
            CatalogoCadenasI18n.TextosPorIdioma["en"]
                .Keys.OrderBy(clave => clave, StringComparer.Ordinal));
        Assert.Equal(66, CatalogoCadenasI18n.TotalCadenas);
    }

    [Fact]
    public void Cada_clave_tiene_texto_en_es_y_en_y_ambos_idiomas_tienen_paridad()
    {
        var clavesEs = CatalogoCadenasI18n.TextosPorIdioma["es"].Keys
            .OrderBy(clave => clave, StringComparer.Ordinal);
        var clavesEn = CatalogoCadenasI18n.TextosPorIdioma["en"].Keys
            .OrderBy(clave => clave, StringComparer.Ordinal);

        Assert.Equal(clavesEs, clavesEn);

        Assert.All(
            CatalogoCadenasI18n.TextosPorIdioma,
            par => Assert.All(
                par.Value.Values,
                texto => Assert.False(string.IsNullOrWhiteSpace(texto))));
    }

    [Fact]
    public void Resuelve_en_el_idioma_activo_con_el_codigo_de_idioma()
    {
        Assert.Equal("Archivo", CatalogoCadenasI18n.Resolver("menu.archivo", "es"));
        Assert.Equal("File", CatalogoCadenasI18n.Resolver("menu.archivo", "en"));
        Assert.Equal("Guardar como…", CatalogoCadenasI18n.Resolver("menu.archivo.guardar-como", "es"));
        Assert.Equal("Save as…", CatalogoCadenasI18n.Resolver("menu.archivo.guardar-como", "en"));
        Assert.Equal("Nota al pie", CatalogoCadenasI18n.Resolver("barra.nota-al-pie", "es"));
        Assert.Equal("Footnote", CatalogoCadenasI18n.Resolver("barra.nota-al-pie", "en"));
    }

    [Fact]
    public void Resuelve_en_el_idioma_activo_con_el_catalogo_de_idiomas()
    {
        Assert.Equal("Edición", CatalogoCadenasI18n.Resolver("menu.edicion", IdiomaDeInterfaz.Español));
        Assert.Equal("Edit", CatalogoCadenasI18n.Resolver("menu.edicion", IdiomaDeInterfaz.English));
        Assert.Equal("Índice", CatalogoCadenasI18n.Resolver("barra.indice", IdiomaDeInterfaz.Español));
        Assert.Equal("Table of contents", CatalogoCadenasI18n.Resolver("barra.indice", IdiomaDeInterfaz.English));
    }

    [Fact]
    public void Clave_ausente_en_el_idioma_destino_se_resuelve_en_espanol_ERR004()
    {
        var textosEspanol = CatalogoCadenasI18n.TextosPorIdioma[CatalogoCadenasI18n.CodigoDefecto];

        // Un idioma no registrado deja todas las claves sin resolver: cada
        // una cae en el idioma defecto (ERR-004), nunca en nulo ni vacío.
        Assert.All(
            ClavesPublicadasPorLosCatalogos(),
            clave => Assert.Equal(textosEspanol[clave], CatalogoCadenasI18n.Resolver(clave, "fr")));
    }

    [Fact]
    public void El_codigo_de_idioma_se_normaliza_sin_distinguir_caja_ni_espacios()
    {
        Assert.Equal("Archivo", CatalogoCadenasI18n.Resolver("menu.archivo", "ES"));
        Assert.Equal("Archivo", CatalogoCadenasI18n.Resolver("menu.archivo", " es "));
        Assert.Equal("File", CatalogoCadenasI18n.Resolver("menu.archivo", " En "));
    }

    [Fact]
    public void Codigo_ausente_o_vacio_cae_en_el_idioma_defecto()
    {
        Assert.Equal("Ayuda", CatalogoCadenasI18n.Resolver("menu.ayuda", ""));
        Assert.Equal("Ayuda", CatalogoCadenasI18n.Resolver("menu.ayuda", "   "));
        Assert.Equal("Ayuda", CatalogoCadenasI18n.Resolver("menu.ayuda", null!));
    }

    [Fact]
    public void Clave_desconocida_nunca_deja_texto_sin_resolver()
    {
        var resuelta = CatalogoCadenasI18n.Resolver("clave.inexistente", "en");

        Assert.Equal("clave.inexistente", resuelta);
        Assert.False(string.IsNullOrWhiteSpace(resuelta));
    }

    [Fact]
    public void Clave_nula_es_un_error_de_programacion()
    {
        Assert.Throws<ArgumentNullException>(
            () => CatalogoCadenasI18n.Resolver(null!, "es"));
    }

    /// <summary>
    /// Unión de las claves publicadas por los catálogos de T-002: los 6 menús
    /// y sus 48 entradas del menú principal (FR-011) y los 12 controles de la
    /// barra de formato (FR-012), sin duplicados.
    /// </summary>
    private static IEnumerable<string> ClavesPublicadasPorLosCatalogos() =>
        CatalogoMenuPrincipal.Menus
            .Select(menu => menu.Clave)
            .Concat(CatalogoMenuPrincipal.Menus
                .SelectMany(menu => menu.Entradas.Select(entrada => entrada.Clave)))
            .Concat(CatalogoBarraFormato.Controles.Select(control => control.Clave));
}
