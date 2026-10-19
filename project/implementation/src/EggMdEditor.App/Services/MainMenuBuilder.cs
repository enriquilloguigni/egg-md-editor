using Avalonia.Controls;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Builder of SC-001's persistent menu bar (T-008, T-010): fills a
/// <see cref="Menu"/> control with 100 % of the menus and entries of
/// <see cref="MainMenuCatalog"/> —single source of count and order (FR-011,
/// DS-001)— in the catalog's declared order, with no extra or missing
/// entries. Labels are resolved from the i18n string catalog (T-003) in the
/// session's active language (Spanish at startup, BR-007) and each element
/// exposes its catalog key in <see cref="Control.Tag"/>, with which the hot
/// relabeling (T-011, COM-001) re-resolves the labels. The Interface language
/// entry of Tools (T-010) deploys the two values of the
/// <see cref="InterfaceLanguage"/> catalog as radio items of the same group —
/// mutually exclusive selection, always one checked (SC-001)— with the check
/// taken from the session's active language. Every entry remains visible with
/// no business action connected (ADR-001): no command and no disabled marks,
/// with the same appearance as a connected entry; opening and closing the
/// menu does not alter the document.
/// </summary>
public static class MainMenuBuilder
{
    /// <summary>Key of the Tools → Interface language entry (FR-011).</summary>
    public const string LanguageEntryKey = "menu.herramientas.idioma-interfaz";

    /// <summary>
    /// Radio group of the language values: within the group only one value
    /// stays checked at a time (mutually exclusive selection, SC-001).
    /// </summary>
    private const string LanguageGroup = "idioma-de-la-interfaz";

    /// <summary>
    /// Fills the given menu bar with the 6 menus of FR-011 in order and 100 %
    /// of their entries in order (DS-001), built from the main menu catalog.
    /// The bar is cleared before filling, so the building can be repeated over
    /// the same bar.
    /// </summary>
    public static void Build(Menu menuBar, ApplicationSession session)
    {
        ArgumentNullException.ThrowIfNull(menuBar);
        ArgumentNullException.ThrowIfNull(session);

        menuBar.Items.Clear();

        foreach (var menu in MainMenuCatalog.Menus)
        {
            menuBar.Items.Add(BuildMenuElement(menu, session));
        }
    }

    /// <summary>
    /// Catalog key of a language value of the Interface language entry
    /// (T-010): derived from the i18n string catalog's language code, the
    /// single source of identity of the value. It serves to look the value up
    /// in the tree and to re-check it in the wiring of T-011 (COM-001).
    /// </summary>
    public static string LanguageValueKey(InterfaceLanguage language) =>
        $"{LanguageEntryKey}.{I18nStringCatalog.CodeOf(language)}";

    /// <summary>
    /// Builds the first-level element of a catalog menu, with its entries in
    /// declared order. The catalog key stays in <see cref="Control.Tag"/> of
    /// each element.
    /// </summary>
    private static MenuItem BuildMenuElement(MainMenu menu, ApplicationSession session)
    {
        var item = new MenuItem
        {
            Header = I18nStringCatalog.Resolve(menu.Key, session.ActiveLanguage),
            Tag = menu.Key,
        };

        foreach (var entry in menu.Entries)
        {
            item.Items.Add(entry.Key == LanguageEntryKey
                ? BuildLanguageEntry(entry, session)
                : BuildMenuEntry(entry, session));
        }

        return item;
    }

    /// <summary>
    /// Builds a regular menu entry: label resolved in the active language and
    /// catalog key in <see cref="Control.Tag"/>, with no connected action
    /// (ADR-001).
    /// </summary>
    private static MenuItem BuildMenuEntry(MenuEntry entry, ApplicationSession session) => new()
    {
        Header = I18nStringCatalog.Resolve(entry.Key, session.ActiveLanguage),
        Tag = entry.Key,
    };

    /// <summary>
    /// Builds the Interface language entry (T-010) with the values of
    /// <see cref="LanguageCatalog.Values"/> —in canonical order, with no extra
    /// or missing values— as radio items of the same group, so the selection
    /// is mutually exclusive: there is always one checked and it is the
    /// session's active language one (Spanish at startup, BR-F001-004). No
    /// business action connected (ADR-001): switching the language and
    /// re-checking is the task of the wiring of T-011 (COM-001).
    /// </summary>
    private static MenuItem BuildLanguageEntry(MenuEntry entry, ApplicationSession session)
    {
        var item = BuildMenuEntry(entry, session);

        foreach (var language in LanguageCatalog.Values)
        {
            var key = LanguageValueKey(language);

            item.Items.Add(new MenuItem
            {
                Header = I18nStringCatalog.Resolve(key, session.ActiveLanguage),
                Tag = key,
                ToggleType = MenuItemToggleType.Radio,
                GroupName = LanguageGroup,
                IsChecked = language == session.ActiveLanguage,
            });
        }

        return item;
    }
}
