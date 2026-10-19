namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Catálogo de idiomas de la interfaz (DS-001): valores controlados
/// <c>Español</c> y <c>English</c>, mutuamente excluyentes (BR-007).
/// El valor de arranque de la sesión es siempre Español.
/// </summary>
public enum IdiomaDeInterfaz
{
    Español = 1,
    English = 2,
}

/// <summary>
/// Catálogo en memoria del catálogo <c>Idioma</c> (DS-001): lista ordenada de
/// valores y defecto de sesión. Sin persistencia (FR-013 → F-030).
/// </summary>
public static class CatalogoIdiomas
{
    /// <summary>Valores controlados del catálogo, en orden canónico.</summary>
    public static IReadOnlyList<IdiomaDeInterfaz> Valores { get; } =
        [IdiomaDeInterfaz.Español, IdiomaDeInterfaz.English];

    /// <summary>Defecto de sesión: exactamente un idioma activo, siempre Español (BR-007).</summary>
    public static IdiomaDeInterfaz Defecto { get; } = IdiomaDeInterfaz.Español;
}
