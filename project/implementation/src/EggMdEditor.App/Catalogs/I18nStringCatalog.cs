namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Catalog of i18n strings (DS-001): key→text pairs per language code that
/// feed the menu labels and the tooltips of SC-001. Requires text for
/// <c>es</c> and <c>en</c>, with Spanish as the interface default
/// (BR-F001-004); it is extensible to more languages by adding a new language
/// code with the same set of keys, without changing the UI or the resolution
/// (NFR-006, BR-F001-007). In memory, no persistence.
/// </summary>
public static class I18nStringCatalog
{
    /// <summary>Code of the interface's default language: Spanish (BR-F001-004).</summary>
    public const string DefaultCode = "es";

    /// <summary>
    /// Texts by language code: each registered code exposes exactly the same
    /// set of keys. The language code (not the <see cref="InterfaceLanguage"/>
    /// catalog) governs the resolution, so adding a language requires no code
    /// changes (NFR-006).
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> TextsByLanguage { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["es"] = new Dictionary<string, string>
            {
                // Menus of FR-011 (order of the main menu catalog).
                ["menu.archivo"] = "Archivo",
                ["menu.edicion"] = "Edición",
                ["menu.ver"] = "Ver",
                ["menu.formato"] = "Formato",
                ["menu.herramientas"] = "Herramientas",
                ["menu.ayuda"] = "Ayuda",

                // File (FR-011).
                ["menu.archivo.nuevo"] = "Nuevo",
                ["menu.archivo.abrir"] = "Abrir…",
                ["menu.archivo.guardar"] = "Guardar",
                ["menu.archivo.guardar-como"] = "Guardar como…",
                ["menu.archivo.recientes"] = "Recientes",
                ["menu.archivo.autoguardado"] = "Autoguardado",
                ["menu.archivo.exportar-pdf"] = "Exportar a PDF…",

                // Edit (FR-007 → FR-011).
                ["menu.edicion.deshacer"] = "Deshacer",
                ["menu.edicion.rehacer"] = "Rehacer",
                ["menu.edicion.cortar"] = "Cortar",
                ["menu.edicion.copiar"] = "Copiar",
                ["menu.edicion.pegar"] = "Pegar",
                ["menu.edicion.seleccionar-todo"] = "Seleccionar todo",
                ["menu.edicion.buscar"] = "Buscar…",
                ["menu.edicion.reemplazar"] = "Reemplazar…",
                ["menu.edicion.ir-a-linea"] = "Ir a la línea…",

                // View (FR-009 → FR-011).
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

                // Format (FR-002 → FR-011).
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

                // Tools (FR-011).
                ["menu.herramientas.idioma-interfaz"] = "Idioma de la interfaz",
                // Values of the language selection of the previous entry
                // (SC-001, T-010): proper names of each language, the same
                // in Spanish and English.
                ["menu.herramientas.idioma-interfaz.es"] = "Español",
                ["menu.herramientas.idioma-interfaz.en"] = "English",
                ["menu.herramientas.contador-palabras"] = "Contador de palabras y caracteres",
                ["menu.herramientas.revision-ortografica"] = "Revisión ortográfica",

                // Help (FR-010 → FR-011).
                ["menu.ayuda.guia-markdown"] = "Guía de Markdown…",
                ["menu.ayuda.atajos"] = "Atajos de teclado…",
                ["menu.ayuda.acercade"] = "Acerca de…",

                // Format bar (FR-012): floating tooltips (VR-002), in the
                // writing order of the bar catalog.
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
                // Values of the language selection of the previous entry
                // (SC-001, T-010): proper names of each language, the same
                // in Spanish and English.
                ["menu.herramientas.idioma-interfaz.es"] = "Español",
                ["menu.herramientas.idioma-interfaz.en"] = "English",
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
    /// Total strings of the catalog. Derived from the default language: it is
    /// the single source of count for the tests (T-013).
    /// </summary>
    public static int TotalStrings => TextsByLanguage[DefaultCode].Count;

    /// <summary>
    /// Language code of a value of the <see cref="InterfaceLanguage"/> catalog.
    /// An unmapped value falls back to the default code: there is never a
    /// language without resolution (BR-F001-004).
    /// </summary>
    public static string CodeOf(InterfaceLanguage language) => language switch
    {
        InterfaceLanguage.Spanish => "es",
        InterfaceLanguage.English => "en",
        _ => DefaultCode,
    };

    /// <summary>
    /// Resolves the key in the active language of the
    /// <see cref="InterfaceLanguage"/> catalog. The resolution is that of
    /// <see cref="Resolve(string, string)"/>.
    /// </summary>
    public static string Resolve(string key, InterfaceLanguage language) =>
        Resolve(key, CodeOf(language));

    /// <summary>
    /// Resolves the key in the given language code. Every visible string of
    /// SC-001 is resolved here (BR-F001-007): if the key is not resolved in
    /// the target language, it is resolved in Spanish, the default language
    /// (ERR-004); a key absent even from the default is resolved to the key
    /// itself, so no label ends up null or empty. An empty, null or
    /// unregistered language code falls back to the default language
    /// (BR-F001-004).
    /// </summary>
    public static string Resolve(string key, string languageCode)
    {
        ArgumentNullException.ThrowIfNull(key);

        var code = string.IsNullOrWhiteSpace(languageCode)
            ? DefaultCode
            : languageCode.Trim().ToLowerInvariant();

        if (TextsByLanguage.TryGetValue(code, out var texts)
            && texts.TryGetValue(key, out var text))
        {
            return text;
        }

        return TextsByLanguage.TryGetValue(DefaultCode, out var defaultTexts)
            && defaultTexts.TryGetValue(key, out var defaultText)
                ? defaultText
                : key;
    }
}
