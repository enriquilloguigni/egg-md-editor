using Avalonia.Controls;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Relabelizado en caliente de SC-001 (COM-001, T-011): re-resuelve toda
/// cadena visible contra el idioma activo ya aplicado a la sesión, sin
/// reinicio (DS-003, NFR-006, BR-F001-005).
///
/// <para>
/// La fuente de cada cadena es la clave de catálogo que el control expone en
/// <see cref="Control.Tag"/> —puesta ahí por los constructores del menú
/// (T-008, T-010) y de la barra de formato (T-009)— y la resolución es la del
/// catálogo de cadenas i18n (T-003) en el idioma dado: ningún texto se
/// reconstruye a mano ni se guarda copia, de modo que el relabelizado no
/// puede desviarse del catálogo (DS-003). Los rótulos de menú —menús,
/// entradas y los valores de la selección de idioma— se re-resuelven en su
/// <see cref="MenuItem.Header"/>; los avisos flotantes de la barra de formato
/// (VR-002), en su <see cref="Avalonia.Controls.ToolTip"/>; y la marca de la
/// selección de idioma se re-pinta desde el estado de la sesión, de modo que
/// la marca acompañe al idioma activo sea cual sea el origen de la
/// conmutación (menú o atajo de teclado). Claves ausentes del idioma activo
/// caen en el idioma defecto español dentro de la propia resolución (ERR-004).
/// </para>
///
/// <para>
/// El estado de la sesión —menú desplegado y aviso flotante visible— queda
/// intacto: el relabelizado solo toca textos y marcas (BR-F001-005). Con una
/// conmutación abortada (ERR-003) el servicio no notifica y el cableado no
/// invoca el relabelizado: los rótulos permanecen en el idioma anterior.
/// </para>
/// </summary>
public static class RelabelizadorDeInterfaz
{
    /// <summary>
    /// Re-resuelve los rótulos de la barra de menú y los avisos de la barra
    /// de formato en el idioma activo dado, y re-marca la selección de idioma
    /// del menú Herramientas según ese idioma. Idempotente: puede invocarse
    /// tras cada conmutación sin efecto adicional.
    /// </summary>
    public static void Relabelizar(Menu barraMenu, StackPanel barraFormato, IdiomaDeInterfaz idiomaActivo)
    {
        ArgumentNullException.ThrowIfNull(barraMenu);
        ArgumentNullException.ThrowIfNull(barraFormato);

        foreach (var item in barraMenu.Items.OfType<MenuItem>())
        {
            RelabelizarElemento(item, idiomaActivo);
        }

        foreach (var boton in barraFormato.Children.OfType<Button>())
        {
            if (boton.Tag is string clave)
            {
                ToolTip.SetTip(boton, CatalogoCadenasI18n.Resolver(clave, idiomaActivo));
            }
        }
    }

    /// <summary>
    /// Re-resuelve un elemento del menú —rótulo y, si es un valor de la
    /// selección de idioma, su marca de radio— y repite el tratamiento en sus
    /// subelementos, en profundidad.
    /// </summary>
    private static void RelabelizarElemento(MenuItem item, IdiomaDeInterfaz idiomaActivo)
    {
        if (item.Tag is string clave)
        {
            item.Header = CatalogoCadenasI18n.Resolver(clave, idiomaActivo);

            if (item.ToggleType == MenuItemToggleType.Radio)
            {
                item.IsChecked = clave == ConstructorDeMenuPrincipal.ClaveDeValorDeIdioma(idiomaActivo);
            }
        }

        foreach (var hijo in item.Items.OfType<MenuItem>())
        {
            RelabelizarElemento(hijo, idiomaActivo);
        }
    }
}
