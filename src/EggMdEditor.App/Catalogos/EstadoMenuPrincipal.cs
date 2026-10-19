namespace EggMdEditor.App.Catalogos;

/// <summary>
/// Catálogo de estados del menú principal (DS-001): <c>Cerrado</c> o
/// <c>Desplegado con entrada activa</c>. Defecto de sesión: Cerrado.
/// </summary>
public enum EstadoMenuPrincipal
{
    Cerrado = 1,
    DesplegadoConEntradaActiva = 2,
}

/// <summary>
/// Catálogo en memoria del estado del menú principal (DS-001): lista ordenada
/// de valores y defecto de sesión.
/// </summary>
public static class CatalogoEstadoMenuPrincipal
{
    /// <summary>Valores controlados del catálogo, en orden canónico.</summary>
    public static IReadOnlyList<EstadoMenuPrincipal> Valores { get; } =
        [EstadoMenuPrincipal.Cerrado, EstadoMenuPrincipal.DesplegadoConEntradaActiva];

    /// <summary>Defecto de sesión: menú cerrado al arrancar.</summary>
    public static EstadoMenuPrincipal Defecto { get; } = EstadoMenuPrincipal.Cerrado;
}
