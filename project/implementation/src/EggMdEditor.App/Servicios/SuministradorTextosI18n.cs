using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Fuente de los textos i18n (clave→texto) por código de idioma, tras la que
/// trabaja el servicio de conmutación (COM-001). «Cargar el catálogo del
/// idioma destino» es la operación que puede fallar y provocar el abortado
/// sin cambio parcial (ERR-003); al estar tras una interfaz del núcleo, el
/// servicio no depende de dónde viven los textos y las pruebas pueden
/// simular el fallo de carga.
/// </summary>
public interface ISuministradorTextosI18n
{
    /// <summary>
    /// Intenta cargar los textos del código de idioma dado. Devuelve
    /// <c>false</c> si el catálogo del idioma no carga (ERR-003); un código
    /// vacío o nulo conserva la semántica del catálogo de cadenas: cae en el
    /// idioma defecto (BR-F001-004).
    /// </summary>
    bool TryCargar(string codigoIdioma, out IReadOnlyDictionary<string, string> textos);
}

/// <summary>
/// Suministrador por defecto: carga los textos desde el catálogo de cadenas
/// i18n en memoria (T-003). Un idioma registrado y con texto carga; un código
/// no registrado no carga y provoca el abortado de la conmutación (ERR-003).
/// </summary>
public sealed class SuministradorCatalogoCadenasI18n : ISuministradorTextosI18n
{
    private static readonly IReadOnlyDictionary<string, string> TextosVacios =
        new Dictionary<string, string>();

    /// <summary>Instancia única sin estado (el catálogo es estático).</summary>
    public static SuministradorCatalogoCadenasI18n Instancia { get; } = new();

    private SuministradorCatalogoCadenasI18n()
    {
    }

    /// <summary>
    /// Carga los textos del catálogo de cadenas i18n para el código dado,
    /// normalizado como en la resolución de claves (caja y espacios no
    /// cuentan; vacío o nulo cae en el idioma defecto).
    /// </summary>
    public bool TryCargar(string codigoIdioma, out IReadOnlyDictionary<string, string> textos)
    {
        var codigo = string.IsNullOrWhiteSpace(codigoIdioma)
            ? CatalogoCadenasI18n.CodigoDefecto
            : codigoIdioma.Trim().ToLowerInvariant();

        if (CatalogoCadenasI18n.TextosPorIdioma.TryGetValue(codigo, out var textosEncontrados)
            && textosEncontrados.Count > 0)
        {
            textos = textosEncontrados;
            return true;
        }

        textos = TextosVacios;
        return false;
    }
}
