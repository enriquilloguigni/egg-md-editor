using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Constructor de la barra de formato de SC-001 (T-009): llena un panel
/// horizontal con el 100 % de los controles de <see cref="CatalogoBarraFormato"/>
/// —fuente única de recuento y de orden (FR-012, DS-001)— en el orden de
/// escritura declarado, sin controles de más ni ausentes. Cada control muestra
/// su icono (VR-001: un glyph por control, según el patrón ventana-principal
/// del design-system) y su aviso flotante (VR-002): el nombre del control
/// resuelto desde el catálogo de cadenas i18n (T-003) en el idioma activo de
/// la sesión (arranque español, BR-007), que se muestra al puntero mientras
/// permanece sobre el control y se oculta al retirarlo, sin interacción de
/// clic. Cada botón expone su clave de catálogo en <see cref="Control.Tag"/>,
/// con la que el relabelizado en caliente de los avisos (T-011, COM-001)
/// re-resuelve los textos. Ningún control queda conectado a acciones
/// (ADR-001): sin comando y sin marcas de deshabilitado, con la misma
/// apariencia que un control conectado; pulsarlo no altera el documento.
/// </summary>
public static class ConstructorDeBarraFormato
{
    /// <summary>
    /// Glyph (VR-001) de cada control de la barra, por clave de catálogo: un
    /// glifo por control en el orden de escritura. F-001 no incorpora
    /// recursos binarios de iconografía: el glifo textual implementa el
    /// patrón del design-system (mockup Z3, [ic]) a la espera del juego de
    /// iconos definitivo (TK-004), con tamaño y espaciado del control.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Glifos { get; } =
        new Dictionary<string, string>
        {
            ["barra.enfasis"] = "B",
            ["barra.encabezados"] = "H▾",
            ["barra.listas"] = "≡",
            ["barra.cita"] = "“",
            ["barra.codigo"] = "</>",
            ["barra.enlace"] = "↗",
            ["barra.imagen"] = "▣",
            ["barra.tabla"] = "▦",
            ["barra.nota-al-pie"] = "†",
            ["barra.formula"] = "∑",
            ["barra.diagrama"] = "◇",
            ["barra.indice"] = "⋮",
        };

    /// <summary>
    /// Llena la barra dada con los 12 controles de FR-012 en orden de
    /// escritura (DS-001), construidos desde el catálogo de la barra de
    /// formato. La barra queda limpia antes de rellenar, de modo que la
    /// construcción puede repetirse sobre la misma barra.
    /// </summary>
    public static void Construir(StackPanel barra, SesionDeAplicacion sesion)
    {
        ArgumentNullException.ThrowIfNull(barra);
        ArgumentNullException.ThrowIfNull(sesion);

        barra.Children.Clear();

        foreach (var control in CatalogoBarraFormato.Controles)
        {
            barra.Children.Add(ConstruirControl(control, sesion));
        }
    }

    /// <summary>
    /// Construye el botón de un control del catálogo: el glyph (VR-001) como
    /// contenido, la clave de catálogo en <see cref="Control.Tag"/> para el
    /// relabelizado (T-011, COM-001) y el aviso flotante (VR-002) resuelto en
    /// el idioma activo. Sin comando: control visible sin acción conectada
    /// (ADR-001).
    /// </summary>
    private static Button ConstruirControl(ControlDeBarra control, SesionDeAplicacion sesion)
    {
        var boton = new Button
        {
            Content = new TextBlock
            {
                Text = Glifos[control.Clave],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Tag = control.Clave,
            Padding = new Thickness(10, 4),
        };

        // VR-002: aviso flotante con el nombre del control en el idioma
        // activo; visible al puntero y oculto al retirarlo, sin clic.
        ToolTip.SetTip(boton, CatalogoCadenasI18n.Resolver(control.Clave, sesion.IdiomaActivo));

        return boton;
    }
}
