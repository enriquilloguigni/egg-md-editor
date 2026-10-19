namespace EggMdEditor.App.Catalogs;

/// <summary>
/// Application session (DS-001): ephemeral state from startup until the
/// SC-001 window closes; not persistent (FR-013 → F-030) and with startup
/// defaults per the Spec: Español language (BR-007), Closed menu and hidden
/// tooltip. It contains exactly one SC-001 window (BR-003), which is not
/// modeled here (the window is the responsibility of T-008).
/// </summary>
public sealed class ApplicationSession
{
    /// <summary>
    /// Unique identifier of the session, assigned when it is created (one
    /// session per run): correlation for the technical logs, which all share
    /// it and go to standard output, with no writing to disk (NFR-003).
    /// </summary>
    public string SessionId { get; } = Guid.NewGuid().ToString("D");

    /// <summary>Active language of the session. Always exactly one (BR-007).</summary>
    public InterfaceLanguage ActiveLanguage { get; set; } = LanguageCatalog.Default;

    /// <summary>State of the main menu of SC-001.</summary>
    public MainMenuState MainMenuState { get; set; } = MainMenuStateCatalog.Default;

    /// <summary>
    /// Tooltip visible (VR-002). Boolean with default No; when visible, it is
    /// always attached to a control of the format bar (identifiable by the
    /// key of <see cref="FormatBarCatalog"/>).
    /// </summary>
    public bool IsToolTipVisible { get; set; }
}
