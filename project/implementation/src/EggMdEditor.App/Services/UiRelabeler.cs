using Avalonia.Controls;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Hot relabeling of SC-001 (COM-001, T-011): re-resolves every visible
/// string against the active language already applied to the session, with no
/// restart (DS-003, NFR-006, BR-F001-005).
///
/// <para>
/// The source of each string is the catalog key the control exposes in
/// <see cref="Control.Tag"/> —put there by the menu builders (T-008, T-010)
/// and the format bar builder (T-009)— and the resolution is that of the i18n
/// string catalog (T-003) in the given language: no text is rebuilt by hand
/// nor a copy is kept, so the relabeling cannot deviate from the catalog
/// (DS-003). Menu labels —menus, entries and the language selection
/// values— are re-resolved in their <see cref="MenuItem.Header"/>; the format
/// bar tooltips (VR-002), in their <see cref="Avalonia.Controls.ToolTip"/>;
/// and the language selection check is repainted from the session state, so
/// the check follows the active language whatever the origin of the switching
/// (menu or keyboard shortcut). Keys absent from the active language fall
/// back to the default Spanish language within the resolution itself
/// (ERR-004).
/// </para>
///
/// <para>
/// The session state —expanded menu and visible tooltip— remains intact: the
/// relabeling only touches texts and checks (BR-F001-005). With an aborted
/// switching (ERR-003) the service does not notify and the wiring does not
/// invoke the relabeling: labels remain in the previous language.
/// </para>
/// </summary>
public static class UiRelabeler
{
    /// <summary>
    /// Re-resolves the menu bar labels and the format bar tooltips in the
    /// given active language, and re-checks the Tools language selection
    /// according to that language. Idempotent: it can be invoked after every
    /// switching with no additional effect.
    /// </summary>
    public static void Relabel(Menu menuBar, StackPanel formatBar, InterfaceLanguage activeLanguage)
    {
        ArgumentNullException.ThrowIfNull(menuBar);
        ArgumentNullException.ThrowIfNull(formatBar);

        foreach (var item in menuBar.Items.OfType<MenuItem>())
        {
            RelabelItem(item, activeLanguage);
        }

        foreach (var button in formatBar.Children.OfType<Button>())
        {
            if (button.Tag is string key)
            {
                ToolTip.SetTip(button, I18nStringCatalog.Resolve(key, activeLanguage));
            }
        }
    }

    /// <summary>
    /// Re-resolves a menu item —label and, if it is a language selection
    /// value, its radio check— and repeats the treatment on its children, in
    /// depth.
    /// </summary>
    private static void RelabelItem(MenuItem item, InterfaceLanguage activeLanguage)
    {
        if (item.Tag is string key)
        {
            item.Header = I18nStringCatalog.Resolve(key, activeLanguage);

            if (item.ToggleType == MenuItemToggleType.Radio)
            {
                item.IsChecked = key == MainMenuBuilder.LanguageValueKey(activeLanguage);
            }
        }

        foreach (var child in item.Items.OfType<MenuItem>())
        {
            RelabelItem(child, activeLanguage);
        }
    }
}
