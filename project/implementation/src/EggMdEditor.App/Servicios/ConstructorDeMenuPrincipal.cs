using Avalonia.Controls;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Constructor de la barra de menú persistente de SC-001 (T-008): llena un
/// control <see cref="Menu"/> con el 100 % de los menús y entradas de
/// <see cref="CatalogoMenuPrincipal"/> —fuente única de recuento y de orden
/// (FR-011, DS-001)— en el orden declarado del catálogo, sin entradas de más
/// ni ausentes. Los rótulos se resuelven desde el catálogo de cadenas i18n
/// (T-003) en el idioma activo de la sesión (arranque español, BR-007) y cada
/// elemento expone su clave de catálogo en <see cref="Control.Tag"/>, con la
/// que el relabelizado en caliente (T-011, COM-001) re-resuelve los rótulos.
/// Toda entrada queda visible sin acción conectada (ADR-001): sin comando y
/// sin marcas de deshabilitado, con la misma apariencia que una entrada
/// conectada; abrir y cerrar el menú no altera el documento.
/// </summary>
public static class ConstructorDeMenuPrincipal
{
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
            item.Items.Add(new MenuItem
            {
                Header = CatalogoCadenasI18n.Resolver(entrada.Clave, sesion.IdiomaActivo),
                Tag = entrada.Clave,
            });
        }

        return item;
    }
}
