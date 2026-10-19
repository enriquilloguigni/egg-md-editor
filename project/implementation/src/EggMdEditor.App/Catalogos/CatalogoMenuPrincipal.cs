namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Entrada de un menú del catálogo del menú principal (DS-001). Se identifica
/// por una <see cref="Clave"/> única y estable, cuya posición dentro del menú
/// la da el orden de la lista; el rótulo visible es texto i18n resuelto desde
/// el catálogo de cadenas (T-003) en el idioma activo.
/// </summary>
/// <param name="Clave">Identificador único y estable de la entrada.</param>
public sealed record EntradaDeMenu(string Clave);

/// <summary>
/// Menú de primer nivel de la barra de menú persistente (FR-011). Sus
/// entradas van en orden declarado y pertenecen exactamente a este menú
/// (DS-001). La posición de cada entrada es su índice en <see cref="Entradas"/>.
/// </summary>
/// <param name="Clave">Identificador único y estable del menú.</param>
/// <param name="Entradas">Entradas del menú, en orden declarado.</param>
public sealed record MenuPrincipal(string Clave, IReadOnlyList<EntradaDeMenu> Entradas);

/// <summary>
/// Catálogo del menú principal (DS-001): lista ordenada de los 6 menús de
/// FR-011 con el 100 % de sus entradas en orden, como fuente única de
/// recuento y de construcción de la UI (T-008) sin conexión a acciones
/// (ADR-001). En memoria, sin persistencia. El menú contextual de Edición no
/// forma parte de este catálogo (SC-001).
/// </summary>
public static class CatalogoMenuPrincipal
{
    /// <summary>Menús de FR-011 en orden, cada uno con sus entradas en orden.</summary>
    public static IReadOnlyList<MenuPrincipal> Menus { get; } =
    [
        new("menu.archivo",
        [
            new EntradaDeMenu("menu.archivo.nuevo"),
            new EntradaDeMenu("menu.archivo.abrir"),
            new EntradaDeMenu("menu.archivo.guardar"),
            new EntradaDeMenu("menu.archivo.guardar-como"),
            new EntradaDeMenu("menu.archivo.recientes"),
            new EntradaDeMenu("menu.archivo.autoguardado"),
            new EntradaDeMenu("menu.archivo.exportar-pdf"),
        ]),
        new("menu.edicion",
        [
            new EntradaDeMenu("menu.edicion.deshacer"),
            new EntradaDeMenu("menu.edicion.rehacer"),
            new EntradaDeMenu("menu.edicion.cortar"),
            new EntradaDeMenu("menu.edicion.copiar"),
            new EntradaDeMenu("menu.edicion.pegar"),
            new EntradaDeMenu("menu.edicion.seleccionar-todo"),
            new EntradaDeMenu("menu.edicion.buscar"),
            new EntradaDeMenu("menu.edicion.reemplazar"),
            new EntradaDeMenu("menu.edicion.ir-a-linea"),
        ]),
        new("menu.ver",
        [
            new EntradaDeMenu("menu.ver.mostrar-ocultar-vistas"),
            new EntradaDeMenu("menu.ver.mostrar-ocultar-barra-formato"),
            new EntradaDeMenu("menu.ver.plegado-secciones"),
            new EntradaDeMenu("menu.ver.zoom"),
            new EntradaDeMenu("menu.ver.ajuste-linea"),
            new EntradaDeMenu("menu.ver.desplazamiento-sincronizado"),
            new EntradaDeMenu("menu.ver.pantalla-completa"),
            new EntradaDeMenu("menu.ver.modo-foco"),
            new EntradaDeMenu("menu.ver.tema"),
            new EntradaDeMenu("menu.ver.fuente-markdown"),
        ]),
        new("menu.formato",
        [
            new EntradaDeMenu("menu.formato.enfasis"),
            new EntradaDeMenu("menu.formato.encabezados"),
            new EntradaDeMenu("menu.formato.listas"),
            new EntradaDeMenu("menu.formato.citas"),
            new EntradaDeMenu("menu.formato.codigo"),
            new EntradaDeMenu("menu.formato.enlaces"),
            new EntradaDeMenu("menu.formato.imagenes"),
            new EntradaDeMenu("menu.formato.tablas"),
            new EntradaDeMenu("menu.formato.notas-al-pie"),
            new EntradaDeMenu("menu.formato.formulas"),
            new EntradaDeMenu("menu.formato.diagramas"),
            new EntradaDeMenu("menu.formato.escape-sintaxis"),
            new EntradaDeMenu("menu.formato.abrazo-delimitadores"),
            new EntradaDeMenu("menu.formato.alineacion-columnas"),
            new EntradaDeMenu("menu.formato.tab-en-tablas"),
            new EntradaDeMenu("menu.formato.anclas-internas"),
        ]),
        new("menu.herramientas",
        [
            new EntradaDeMenu("menu.herramientas.idioma-interfaz"),
            new EntradaDeMenu("menu.herramientas.contador-palabras"),
            new EntradaDeMenu("menu.herramientas.revision-ortografica"),
        ]),
        new("menu.ayuda",
        [
            new EntradaDeMenu("menu.ayuda.guia-markdown"),
            new EntradaDeMenu("menu.ayuda.atajos"),
            new EntradaDeMenu("menu.ayuda.acercade"),
        ]),
    ];

    /// <summary>Total de menús (6, FR-011). Derivado de la lista: nunca un número fijo.</summary>
    public static int TotalMenus => Menus.Count;

    /// <summary>
    /// Total de entradas del menú principal. Derivado del catálogo: es la
    /// fuente única de recuento para la UI (T-008) y las pruebas (T-013).
    /// </summary>
    public static int TotalEntradas => Menus.Sum(menu => menu.Entradas.Count);
}
