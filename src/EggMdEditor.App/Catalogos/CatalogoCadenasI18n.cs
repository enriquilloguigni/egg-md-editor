namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Catálogo de cadenas i18n (DS-001): pares clave→texto por código de idioma
/// que alimentan los rótulos de menú y los avisos flotantes de SC-001.
/// Requiere texto para <c>es</c> y <c>en</c>, con defecto de interfaz español
/// (BR-F001-004); es extensible a más idiomas añadiendo un nuevo código de
/// idioma con el mismo conjunto de claves, sin cambiar la UI ni la resolución
/// (NFR-006, BR-F001-007). En memoria, sin persistencia.
/// </summary>
public static class CatalogoCadenasI18n
{
    /// <summary>Código del idioma defecto de la interfaz: español (BR-F001-004).</summary>
    public const string CodigoDefecto = "es";

    /// <summary>
    /// Textos por código de idioma: cada código registrado expone exactamente
    /// el mismo conjunto de claves. El código de idioma (no el catálogo
    /// <see cref="IdiomaDeInterfaz"/>) gobierna la resolución, de modo que
    /// añadir un idioma no exige cambios de código (NFR-006).
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> TextosPorIdioma { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["es"] = new Dictionary<string, string>
            {
                // Menús de FR-011 (orden del catálogo del menú principal).
                ["menu.archivo"] = "Archivo",
                ["menu.edicion"] = "Edición",
                ["menu.ver"] = "Ver",
                ["menu.formato"] = "Formato",
                ["menu.herramientas"] = "Herramientas",
                ["menu.ayuda"] = "Ayuda",

                // Archivo (FR-011).
                ["menu.archivo.nuevo"] = "Nuevo",
                ["menu.archivo.abrir"] = "Abrir…",
                ["menu.archivo.guardar"] = "Guardar",
                ["menu.archivo.guardar-como"] = "Guardar como…",
                ["menu.archivo.recientes"] = "Recientes",
                ["menu.archivo.autoguardado"] = "Autoguardado",
                ["menu.archivo.exportar-pdf"] = "Exportar a PDF…",

                // Edición (FR-007 → FR-011).
                ["menu.edicion.deshacer"] = "Deshacer",
                ["menu.edicion.rehacer"] = "Rehacer",
                ["menu.edicion.cortar"] = "Cortar",
                ["menu.edicion.copiar"] = "Copiar",
                ["menu.edicion.pegar"] = "Pegar",
                ["menu.edicion.seleccionar-todo"] = "Seleccionar todo",
                ["menu.edicion.buscar"] = "Buscar…",
                ["menu.edicion.reemplazar"] = "Reemplazar…",
                ["menu.edicion.ir-a-linea"] = "Ir a la línea…",

                // Ver (FR-009 → FR-011).
                ["menu.ver.mostrar-ocultar-vistas"] = "Mostrar/ocultar vistas",
                ["menu.ver.mostrar-ocultar-barra-formato"] = "Mostrar/ocultar barra de formato",
                ["menu.ver.plegado-secciones"] = "Plegado de secciones",
                ["menu.ver.zoom"] = "Zoom",
                ["menu.ver.ajuste-linea"] = "Ajuste de línea",
                ["menu.ver.desplazamiento-sincronizado"] = "Desplazamiento sincronizado",
                ["menu.ver.pantalla-completa"] = "Pantalla completa",
                ["menu.ver.modo-foco"] = "Modo foco",
                ["menu.ver.tema"] = "Tema",
                ["menu.ver.fuente-markdown"] = "Fuente del Markdown",

                // Formato (FR-002 → FR-011).
                ["menu.formato.enfasis"] = "Énfasis",
                ["menu.formato.encabezados"] = "Encabezados",
                ["menu.formato.listas"] = "Listas",
                ["menu.formato.citas"] = "Citas",
                ["menu.formato.codigo"] = "Código",
                ["menu.formato.enlaces"] = "Enlaces",
                ["menu.formato.imagenes"] = "Imágenes",
                ["menu.formato.tablas"] = "Tablas",
                ["menu.formato.notas-al-pie"] = "Notas al pie",
                ["menu.formato.formulas"] = "Fórmulas",
                ["menu.formato.diagramas"] = "Diagramas",
                ["menu.formato.escape-sintaxis"] = "Escape de sintaxis",
                ["menu.formato.abrazo-delimitadores"] = "Abrazo de delimitadores",
                ["menu.formato.alineacion-columnas"] = "Alineación de columnas",
                ["menu.formato.tab-en-tablas"] = "Tab en tablas",
                ["menu.formato.anclas-internas"] = "Anclas internas",

                // Herramientas (FR-011).
                ["menu.herramientas.idioma-interfaz"] = "Idioma de la interfaz",
                ["menu.herramientas.contador-palabras"] = "Contador de palabras y caracteres",
                ["menu.herramientas.revision-ortografica"] = "Revisión ortográfica",

                // Ayuda (FR-010 → FR-011).
                ["menu.ayuda.guia-markdown"] = "Guía de Markdown…",
                ["menu.ayuda.atajos"] = "Atajos de teclado…",
                ["menu.ayuda.acercade"] = "Acerca de…",

                // Barra de formato (FR-012): avisos flotantes (VR-002),
                // en orden de escritura del catálogo de la barra.
                ["barra.enfasis"] = "Énfasis",
                ["barra.encabezados"] = "Encabezados",
                ["barra.listas"] = "Listas",
                ["barra.cita"] = "Cita",
                ["barra.codigo"] = "Código",
                ["barra.enlace"] = "Enlace",
                ["barra.imagen"] = "Imagen",
                ["barra.tabla"] = "Tabla",
                ["barra.nota-al-pie"] = "Nota al pie",
                ["barra.formula"] = "Fórmula",
                ["barra.diagrama"] = "Diagrama",
                ["barra.indice"] = "Índice",
            },
            ["en"] = new Dictionary<string, string>
            {
                // Menus of FR-011 (order of the main menu catalog).
                ["menu.archivo"] = "File",
                ["menu.edicion"] = "Edit",
                ["menu.ver"] = "View",
                ["menu.formato"] = "Format",
                ["menu.herramientas"] = "Tools",
                ["menu.ayuda"] = "Help",

                // File (FR-011).
                ["menu.archivo.nuevo"] = "New",
                ["menu.archivo.abrir"] = "Open…",
                ["menu.archivo.guardar"] = "Save",
                ["menu.archivo.guardar-como"] = "Save as…",
                ["menu.archivo.recientes"] = "Recent",
                ["menu.archivo.autoguardado"] = "Autosave",
                ["menu.archivo.exportar-pdf"] = "Export to PDF…",

                // Edit (FR-007 → FR-011).
                ["menu.edicion.deshacer"] = "Undo",
                ["menu.edicion.rehacer"] = "Redo",
                ["menu.edicion.cortar"] = "Cut",
                ["menu.edicion.copiar"] = "Copy",
                ["menu.edicion.pegar"] = "Paste",
                ["menu.edicion.seleccionar-todo"] = "Select all",
                ["menu.edicion.buscar"] = "Find…",
                ["menu.edicion.reemplazar"] = "Replace…",
                ["menu.edicion.ir-a-linea"] = "Go to line…",

                // View (FR-009 → FR-011).
                ["menu.ver.mostrar-ocultar-vistas"] = "Show/hide views",
                ["menu.ver.mostrar-ocultar-barra-formato"] = "Show/hide format bar",
                ["menu.ver.plegado-secciones"] = "Section folding",
                ["menu.ver.zoom"] = "Zoom",
                ["menu.ver.ajuste-linea"] = "Word wrap",
                ["menu.ver.desplazamiento-sincronizado"] = "Synchronized scrolling",
                ["menu.ver.pantalla-completa"] = "Full screen",
                ["menu.ver.modo-foco"] = "Focus mode",
                ["menu.ver.tema"] = "Theme",
                ["menu.ver.fuente-markdown"] = "Markdown font",

                // Format (FR-002 → FR-011).
                ["menu.formato.enfasis"] = "Emphasis",
                ["menu.formato.encabezados"] = "Headings",
                ["menu.formato.listas"] = "Lists",
                ["menu.formato.citas"] = "Blockquotes",
                ["menu.formato.codigo"] = "Code",
                ["menu.formato.enlaces"] = "Links",
                ["menu.formato.imagenes"] = "Images",
                ["menu.formato.tablas"] = "Tables",
                ["menu.formato.notas-al-pie"] = "Footnotes",
                ["menu.formato.formulas"] = "Formulas",
                ["menu.formato.diagramas"] = "Diagrams",
                ["menu.formato.escape-sintaxis"] = "Escape syntax",
                ["menu.formato.abrazo-delimitadores"] = "Wrap with delimiters",
                ["menu.formato.alineacion-columnas"] = "Column alignment",
                ["menu.formato.tab-en-tablas"] = "Tab in tables",
                ["menu.formato.anclas-internas"] = "Internal anchors",

                // Tools (FR-011).
                ["menu.herramientas.idioma-interfaz"] = "Interface language",
                ["menu.herramientas.contador-palabras"] = "Word and character count",
                ["menu.herramientas.revision-ortografica"] = "Spell checking",

                // Help (FR-010 → FR-011).
                ["menu.ayuda.guia-markdown"] = "Markdown guide…",
                ["menu.ayuda.atajos"] = "Keyboard shortcuts…",
                ["menu.ayuda.acercade"] = "About…",

                // Format bar (FR-012): floating tooltips (VR-002), in the
                // writing order of the format bar catalog.
                ["barra.enfasis"] = "Emphasis",
                ["barra.encabezados"] = "Headings",
                ["barra.listas"] = "Lists",
                ["barra.cita"] = "Blockquote",
                ["barra.codigo"] = "Code",
                ["barra.enlace"] = "Link",
                ["barra.imagen"] = "Image",
                ["barra.tabla"] = "Table",
                ["barra.nota-al-pie"] = "Footnote",
                ["barra.formula"] = "Formula",
                ["barra.diagrama"] = "Diagram",
                ["barra.indice"] = "Table of contents",
            },
        };

    /// <summary>
    /// Total de cadenas del catálogo. Derivado del idioma defecto: es la
    /// fuente única de recuento para las pruebas (T-013).
    /// </summary>
    public static int TotalCadenas => TextosPorIdioma[CodigoDefecto].Count;

    /// <summary>
    /// Código de idioma de un valor del catálogo <see cref="IdiomaDeInterfaz"/>.
    /// Un valor sin mapear cae en el código defecto: nunca hay idioma sin
    /// resolución (BR-F001-004).
    /// </summary>
    public static string CodigoDe(IdiomaDeInterfaz idioma) => idioma switch
    {
        IdiomaDeInterfaz.Español => "es",
        IdiomaDeInterfaz.English => "en",
        _ => CodigoDefecto,
    };

    /// <summary>
    /// Resuelve la clave en el idioma activo del catálogo
    /// <see cref="IdiomaDeInterfaz"/>. La resolución es la de
    /// <see cref="Resolver(string, string)"/>.
    /// </summary>
    public static string Resolver(string clave, IdiomaDeInterfaz idioma) =>
        Resolver(clave, CodigoDe(idioma));

    /// <summary>
    /// Resuelve la clave en el código de idioma dado. Toda cadena visible de
    /// SC-001 se resuelve aquí (BR-F001-007): si la clave no está resuelta en
    /// el idioma destino, se resuelve en español, el idioma defecto (ERR-004);
    /// una clave ausente incluso del defecto se resuelve en la propia clave,
    /// de modo que ningún rótulo queda nulo ni vacío. Un código de idioma
    /// vacío, nulo o no registrado cae en el idioma defecto (BR-F001-004).
    /// </summary>
    public static string Resolver(string clave, string codigoIdioma)
    {
        ArgumentNullException.ThrowIfNull(clave);

        var codigo = string.IsNullOrWhiteSpace(codigoIdioma)
            ? CodigoDefecto
            : codigoIdioma.Trim().ToLowerInvariant();

        if (TextosPorIdioma.TryGetValue(codigo, out var textos)
            && textos.TryGetValue(clave, out var texto))
        {
            return texto;
        }

        return TextosPorIdioma.TryGetValue(CodigoDefecto, out var defecto)
            && defecto.TryGetValue(clave, out var textoDefecto)
                ? textoDefecto
                : clave;
    }
}
