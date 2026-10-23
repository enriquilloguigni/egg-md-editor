using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using EggMdEditor.App.I18n;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// ViewModel de la ventana principal SC-001 (T-006): expone los 6 menús
/// principales de FR-011 (ENT-004) en orden, con el rótulo resuelto desde el
/// catálogo i18n (ENT-006) en el idioma activo. Solo presentación: no hay
/// acciones conectadas a documento (ADR-001); la conmutación de idioma es la
/// única acción disponible (OP-001).
/// </summary>
public partial class MainWindowViewModel : INotifyPropertyChanged
{
    private ICommand? _changeLanguageCommand;

    /// <summary>Claves i18n de los 6 menús de FR-011, en el orden declarado.</summary>
    public static IReadOnlyList<string> MenuKeys { get; } = Array.AsReadOnly(
    [
        "Menu.File",
        "Menu.Edit",
        "Menu.View",
        "Menu.Format",
        "Menu.Tools",
        "Menu.Help",
    ]);

    public const string LanguageMenuKey = "Menu.Tools.Language";
    public const string SpanishLanguageKey = "Language.Spanish";
    public const string EnglishLanguageKey = "Language.English";

    /// <summary>
    /// Controles de la barra de formato de FR-012 en orden de escritura
    /// (ENT-005): clave i18n estable y glyph de su icono (VR-001). El orden
    /// de la lista es el orden visible de la barra: no admite controles de
    /// más ni ausentes.
    /// </summary>
    private static readonly (string Key, string Glyph)[] FormatBarLayout =
    [
        ("barra.enfasis", "B"),
        ("barra.encabezados", "H▾"),
        ("barra.listas", "≡"),
        ("barra.cita", "“"),
        ("barra.codigo", "</>"),
        ("barra.enlace", "↗"),
        ("barra.imagen", "▣"),
        ("barra.tabla", "▦"),
        ("barra.nota-al-pie", "†"),
        ("barra.formula", "Σ"),
        ("barra.diagrama", "◇"),
        ("barra.indice", "⋮"),
    ];

    /// <summary>Claves i18n de los controles de la barra de formato, en orden.</summary>
    public static IReadOnlyList<string> FormatBarKeys { get; } = Array.AsReadOnly(
        FormatBarLayout.Select(control => control.Key).ToArray());

    /// <summary>Glyph del icono (VR-001) de cada control, por clave i18n.</summary>
    public static IReadOnlyDictionary<string, string> FormatBarGlyphs { get; } =
        FormatBarLayout.ToDictionary(
            control => control.Key,
            control => control.Glyph,
            StringComparer.Ordinal);

    /// <summary>
    /// Menús de primer nivel de SC-001, en el orden de FR-011. Cada elemento
    /// conserva su clave i18n y el rótulo resuelto en el idioma activo.
    /// </summary>
    public IReadOnlyList<MainMenuDefinition> Menus
    {
        get
        {
            var catalog = ResolveCatalog();
            var menus = new List<MainMenuDefinition>(MenuKeys.Count);
            foreach (var key in MenuKeys)
            {
                menus.Add(new MainMenuDefinition(key, catalog.Get(key)));
            }

            return new ReadOnlyCollection<MainMenuDefinition>(menus);
        }
    }

    /// <summary>
    /// Controles visibles de la barra de formato de SC-001 (ENT-005), en el
    /// orden de escritura de FR-012: clave i18n, glyph del icono (VR-001) y
    /// nombre resuelto en el idioma activo, que alimenta tanto el aviso
    /// flotante (VR-002) como el nombre accesible (NFR-009).
    /// </summary>
    public IReadOnlyList<FormatBarControlDefinition> FormatBarControls
    {
        get
        {
            var catalog = ResolveCatalog();
            var controls = new List<FormatBarControlDefinition>(FormatBarKeys.Count);
            foreach (var key in FormatBarKeys)
            {
                controls.Add(new FormatBarControlDefinition(key, FormatBarGlyphs[key], catalog.Get(key)));
            }

            return new ReadOnlyCollection<FormatBarControlDefinition>(controls);
        }
    }

    public string FileMenuHeader => Menus[0].Header;
    public string EditMenuHeader => Menus[1].Header;
    public string ViewMenuHeader => Menus[2].Header;
    public string FormatMenuHeader => Menus[3].Header;
    public string ToolsMenuHeader => Menus[4].Header;
    public string HelpMenuHeader => Menus[5].Header;

    /// <summary>Idioma activo de la interfaz (BR-F001-004).</summary>
    public string CurrentLanguage => ResolveCatalog().CurrentLanguage;

    public bool IsSpanish => string.Equals(CurrentLanguage, I18nCatalog.Spanish, StringComparison.OrdinalIgnoreCase);
    public bool IsEnglish => string.Equals(CurrentLanguage, I18nCatalog.English, StringComparison.OrdinalIgnoreCase);

    /// <summary>Entrada Herramientas → Idioma de la interfaz (OP-001, SC-001).</summary>
    public string LanguageMenuHeader => ResolveCatalog().Get(LanguageMenuKey);

    /// <summary>Rótulo de la opción Español de la selección de idioma.</summary>
    public string SpanishOptionHeader => ResolveCatalog().Get(SpanishLanguageKey);

    /// <summary>Rótulo de la opción English de la selección de idioma.</summary>
    public string EnglishOptionHeader => ResolveCatalog().Get(EnglishLanguageKey);

    /// <summary>
    /// Registro técnico de LOG-002. Se inyecta desde la composición de la
    /// aplicación; sin logger el cambio de idioma sigue funcionando igual.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Comando del selector de idioma (Herramientas → Idioma de la interfaz).
    /// El parámetro es el código destino {es,en}.
    /// </summary>
    public ICommand ChangeLanguageCommand =>
        _changeLanguageCommand ??= new RelayCommand<string?>(code => TryChangeLanguage(code));

    /// <summary>
    /// Conmuta el idioma en caliente (OP-001, TR-002/TR-003). Si el catálogo
    /// destino no carga se aborta sin cambio parcial: no se notifica ningún
    /// cambio a la UI y se conservan idioma, ventana, menú y aviso (ERR-003).
    /// </summary>
    public bool TryChangeLanguage(string? language)
    {
        var catalog = ResolveCatalog();
        var previousLanguage = catalog.CurrentLanguage;
        var code = UiPreferences.NormalizeLanguage(language);

        if (code is not ("es" or "en"))
        {
            Logger?.LogError(
                "No se pudo conmutar el idioma a '{TargetLanguage}': no es un idioma soportado; se conserva '{CurrentLanguage}'.",
                language,
                previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        bool succeeded;
        try
        {
            succeeded = Session is not null
                ? Session.TrySetLanguage(code)
                : catalog.TrySetLanguage(code);
        }
        catch (Exception exception)
        {
            Logger?.LogError(
                exception,
                "Fallo al cargar el catálogo i18n del idioma destino '{TargetLanguage}'; se conserva '{CurrentLanguage}'.",
                code,
                previousLanguage);
            RollbackLanguage(catalog, previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        if (!succeeded
            || !string.Equals(catalog.CurrentLanguage, code, StringComparison.OrdinalIgnoreCase))
        {
            // I18nCatalog registra el Error de LOG-002 (ERR-003). No se
            // relabeliza nada; solo se restablece la marca del selector para
            // que siga mostrando el idioma activo real (ERR-003, BR-F001-004).
            RollbackLanguage(catalog, previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        RaiseLanguageChanged();
        LogLanguageSwitch(previousLanguage, catalog.CurrentLanguage);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RestoreLanguageSelection()
    {
        // El selector de radio cambia su marca antes de ejecutar el comando.
        // Al abortar, se reemite el idioma activo para que la opción marcada
        // vuelva a ser la real, sin tocar rótulos ni avisos.
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(IsSpanish));
        OnPropertyChanged(nameof(IsEnglish));
    }

    private void RaiseLanguageChanged()
    {
        OnPropertyChanged(nameof(Menus));
        OnPropertyChanged(nameof(FormatBarControls));
        OnPropertyChanged(nameof(FileMenuHeader));
        OnPropertyChanged(nameof(EditMenuHeader));
        OnPropertyChanged(nameof(ViewMenuHeader));
        OnPropertyChanged(nameof(FormatMenuHeader));
        OnPropertyChanged(nameof(ToolsMenuHeader));
        OnPropertyChanged(nameof(HelpMenuHeader));
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(IsSpanish));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(LanguageMenuHeader));
        OnPropertyChanged(nameof(SpanishOptionHeader));
        OnPropertyChanged(nameof(EnglishOptionHeader));
    }

    private void LogLanguageSwitch(string previousLanguage, string currentLanguage)
    {
        if (Logger is null)
        {
            return;
        }

        var unresolvedKeys = ResolveCatalog().GetUnresolvedKeys();
        if (unresolvedKeys.Count > 0)
        {
            // LOG-002: Warning si quedan claves i18n sin resolver (ERR-004).
            Logger.LogWarning(
                "Conmutación de idioma completada: {SourceLanguage} -> {TargetLanguage}; {UnresolvedCount} claves i18n sin resolver: {UnresolvedKeys}.",
                previousLanguage,
                currentLanguage,
                unresolvedKeys.Count,
                string.Join(", ", unresolvedKeys));
            return;
        }

        // LOG-002: Debug si la conmutación es exitosa y completa.
        Logger.LogDebug(
            "Conmutación de idioma completada: {SourceLanguage} -> {TargetLanguage}.",
            previousLanguage,
            currentLanguage);
    }

    private void RollbackLanguage(II18nCatalog catalog, string previousLanguage)
    {
        if (!string.Equals(catalog.CurrentLanguage, previousLanguage, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                catalog.TrySetLanguage(previousLanguage);
            }
            catch
            {
                // Rollback best-effort: el estado previo ya se conservaba en la UI.
            }
        }

        if (Session is not null
            && !string.Equals(Session.Language, previousLanguage, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Session.TrySetLanguage(previousLanguage);
            }
            catch
            {
                // Rollback best-effort.
            }
        }
    }

    private II18nCatalog ResolveCatalog() => Catalog ?? Session?.Catalog ?? I18nCatalog.Instance;
}

/// <summary>
/// Menú de primer nivel de la barra persistente de SC-001 (ENT-004): clave
/// estable del catálogo i18n y rótulo visible en el idioma activo.
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006).</param>
/// <param name="Header">Rótulo resuelto en el idioma activo.</param>
public sealed record MainMenuDefinition(string Key, string Header);

/// <summary>
/// Control de la barra de formato de SC-001 (ENT-005): clave i18n estable,
/// glyph del icono (VR-001) y nombre resuelto en el idioma activo, que se
/// muestra como aviso flotante (VR-002) y se expone como nombre accesible
/// (NFR-009). Sin acción conectada en F-001 (ADR-001).
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006); también identifica la posición.</param>
/// <param name="Glyph">Glyph visible del icono del control (VR-001).</param>
/// <param name="Name">Nombre del control en el idioma activo (VR-002, NFR-009).</param>
public sealed record FormatBarControlDefinition(string Key, string Glyph, string Name);
