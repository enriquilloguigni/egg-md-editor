using Avalonia.Controls;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Constructor de la barra de menú persistente de SC-001 (T-008, T-010):
/// llena un control <see cref="Menu"/> con el 100 % de los menús y entradas
/// de <see cref="CatalogoMenuPrincipal"/> —fuente única de recuento y de
/// orden (FR-011, DS-001)— en el orden declarado del catálogo, sin entradas
/// de más ni ausentes. Los rótulos se resuelven desde el catálogo de cadenas
/// i18n (T-003) en el idioma activo de la sesión (arranque español, BR-007) y
/// cada elemento expone su clave de catálogo en <see cref="Control.Tag"/>,
/// con la que el relabelizado en caliente (T-011, COM-001) re-resuelve los
/// rótulos. La entrada Idioma de la interfaz de Herramientas (T-010)
/// despliega los dos valores del catálogo <see cref="IdiomaDeInterfaz"/> como
/// elementos de radio del mismo grupo —selección mutuamente excluyente,
/// siempre una marcada (SC-001)— con la marca tomada del idioma activo de la
/// sesión. Toda entrada queda visible sin acción de negocio conectada
/// (ADR-001): sin comando y sin marcas de deshabilitado, con la misma
/// apariencia que una entrada conectada; abrir y cerrar el menú no altera el
/// documento.
/// </summary>
public static class ConstructorDeMenuPrincipal
{
    /// <summary>Clave de la entrada Herramientas → Idioma de la interfaz (FR-011).</summary>
    public const string ClaveEntradaIdioma = "menu.herramientas.idioma-interfaz";

    /// <summary>
    /// Grupo de radio de los valores de idioma: dentro del grupo solo queda
    /// marcado un valor a la vez (selección mutuamente excluyente, SC-001).
    /// </summary>
    private const string GrupoDeIdioma = "idioma-de-la-interfaz";

    /// <summary>
    /// Llena la barra de menú dada con los 6 menús de FR-011 en orden y el
    /// 100 % de sus entradas en orden (DS-001), construidas desde el catálogo
    /// del menú principal. La barra queda limpia antes de rellenar, de modo
    /// que la construcción puede repetirse sobre la misma barra.
    /// </summary>
    public static void Construir(Menu barra, SesionDeAplicacion sesion)
    {
        ArgumentNullException.ThrowIfNull(barra);
        ArgumentNullException.ThrowIfNull(sesion);

        barra.Items.Clear();

        foreach (var menu in CatalogoMenuPrincipal.Menus)
        {
            barra.Items.Add(ConstruirElementoDeMenu(menu, sesion));
        }
    }

    /// <summary>
    /// Clave de catálogo de un valor de idioma de la entrada Idioma de la
    /// interfaz (T-010): deriva del código de idioma del catálogo de cadenas
    /// i18n, única fuente de identidad del valor. Sirve para buscar el valor
    /// en el árbol y para re-marlo en el cableado de T-011 (COM-001).
    /// </summary>
    public static string ClaveDeValorDeIdioma(IdiomaDeInterfaz idioma) =>
        $"{ClaveEntradaIdioma}.{CatalogoCadenasI18n.CodigoDe(idioma)}";

    /// <summary>
    /// Construye el elemento de primer nivel de un menú del catálogo, con sus
    /// entradas en orden declarado. La clave del catálogo queda en
    /// <see cref="Control.Tag"/> de cada elemento.
    /// </summary>
    private static MenuItem ConstruirElementoDeMenu(MenuPrincipal menu, SesionDeAplicacion sesion)
    {
        var item = new MenuItem
        {
            Header = CatalogoCadenasI18n.Resolver(menu.Clave, sesion.IdiomaActivo),
            Tag = menu.Clave,
        };

        foreach (var entrada in menu.Entradas)
        {
            item.Items.Add(entrada.Clave == ClaveEntradaIdioma
                ? ConstruirEntradaDeIdioma(entrada, sesion)
                : ConstruirEntradaDeMenu(entrada, sesion));
        }

        return item;
    }

    /// <summary>
    /// Construye una entrada corriente del menú: rótulo resuelto en el idioma
    /// activo y clave de catálogo en <see cref="Control.Tag"/>, sin acción
    /// conectada (ADR-001).
    /// </summary>
    private static MenuItem ConstruirEntradaDeMenu(EntradaDeMenu entrada, SesionDeAplicacion sesion) => new()
    {
        Header = CatalogoCadenasI18n.Resolver(entrada.Clave, sesion.IdiomaActivo),
        Tag = entrada.Clave,
    };

    /// <summary>
    /// Construye la entrada Idioma de la interfaz (T-010) con los valores de
    /// <see cref="CatalogoIdiomas.Valores"/> —en orden canónico, sin valores
    /// de más ni ausentes— como elementos de radio del mismo grupo, de modo
    /// que la selección queda mutuamente excluyente: siempre hay una marcada
    /// y es la del idioma activo de la sesión (arranque español, BR-F001-004).
    /// Sin acción de negocio conectada (ADR-001): conmutar el idioma y
    /// re-marcar es tarea del cableado de T-011 (COM-001).
    /// </summary>
    private static MenuItem ConstruirEntradaDeIdioma(EntradaDeMenu entrada, SesionDeAplicacion sesion)
    {
        var item = ConstruirEntradaDeMenu(entrada, sesion);

        foreach (var idioma in CatalogoIdiomas.Valores)
        {
            var clave = ClaveDeValorDeIdioma(idioma);

            item.Items.Add(new MenuItem
            {
                Header = CatalogoCadenasI18n.Resolver(clave, sesion.IdiomaActivo),
                Tag = clave,
                ToggleType = MenuItemToggleType.Radio,
                GroupName = GrupoDeIdioma,
                IsChecked = idioma == sesion.IdiomaActivo,
            });
        }

        return item;
    }
}
