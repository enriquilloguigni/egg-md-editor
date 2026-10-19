namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Catalog of main menu states (DS-001): <c>Closed</c> or <c>Expanded with
/// active entry</c>. Session default: Closed.
/// </summary>
public enum MainMenuState
{
    Closed = 1,
    ExpandedWithActiveEntry = 2,
}

/// <summary>
/// In-memory catalog of the main menu state (DS-001): ordered list of values
/// and session default.
/// </summary>
public static class MainMenuStateCatalog
{
    /// <summary>Controlled values of the catalog, in canonical order.</summary>
    public static IReadOnlyList<MainMenuState> Values { get; } =
        [MainMenuState.Closed, MainMenuState.ExpandedWithActiveEntry];

    /// <summary>Session default: menu closed at startup.</summary>
    public static MainMenuState Default { get; } = MainMenuState.Closed;
}
