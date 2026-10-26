using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Secciones de la superficie de ayuda SC-003. F-010 conectará el contenido
/// completo; en F-001 la superficie es visible con la guía rápida, el listado
/// de atajos disponibles y el Acerca de (versión, atribuciones y licencias).
/// </summary>
public enum HelpSection
{
    About = 0,
    Guide = 1,
    Shortcuts = 2,
}

/// <summary>
/// Definición de una sección visible de SC-003: clave estable y título y
/// texto resueltos en el idioma activo (BR-F001-007).
/// </summary>
/// <param name="Key">Clave del catálogo i18n de la sección.</param>
/// <param name="Title">Título en el idioma activo.</param>
/// <param name="Content">Texto seleccionable de la sección en el idioma activo.</param>
public sealed record HelpSectionDefinition(string Key, string Title, string Content);

/// <summary>Atajo disponible en F-001 con su descripción localizada.</summary>
/// <param name="Combination">Combinación canónica (p. ej. «Alt+A»).</param>
/// <param name="Description">Descripción de la acción en el idioma activo.</param>
public sealed record HelpShortcutDefinition(string Combination, string Description);

/// <summary>
/// ViewModel de la ventana de ayuda SC-003 (T-014): navegación por secciones
/// con texto seleccionable, Acerca de con versión, atribuciones y licencias
/// MIT, y listado de los atajos disponibles en F-001. Los textos se resuelven
/// en el idioma activo y se relabelizan en caliente (BR-013, COM-002).
/// </summary>
public sealed class HelpWindowViewModel : INotifyPropertyChanged, IDisposable
{
    public const string AboutKey = "Help.Section.About";
    public const string GuideKey = "Help.Section.Guide";
    public const string ShortcutsKey = "Help.Section.Shortcuts";

    private readonly Session? _session;
    private readonly II18nCatalog _catalog;
    private readonly Func<IReadOnlyList<HelpShortcutDefinition>>? _shortcutProvider;
    private string _activeKey;
    private IReadOnlyList<HelpSectionDefinition>? _sections;
    private bool _disposed;

    public HelpWindowViewModel(
        Session? session = null,
        II18nCatalog? catalog = null,
        HelpSection initialSection = HelpSection.About,
        Func<IReadOnlyList<HelpShortcutDefinition>>? shortcutProvider = null)
    {
        _session = session;
        _catalog = catalog ?? session?.Catalog ?? I18nCatalog.Instance;
        _shortcutProvider = shortcutProvider;
        _activeKey = ToKey(initialSection);

        if (_session is not null)
        {
            _session.PropertyChanged += OnSessionPropertyChanged;
        }
    }

    /// <summary>Título de la ventana, en el idioma activo.</summary>
    public string WindowTitle => _catalog.Get("Help.WindowTitle");

    /// <summary>Rótulo de la columna de navegación.</summary>
    public string SectionsHeader => _catalog.Get("Help.Sections");

    /// <summary>Rótulo de la acción de copiar el texto de la sección activa.</summary>
    public string CopyTextLabel => _catalog.Get("Help.CopyText");

    /// <summary>Rótulo de la acción de cierre.</summary>
    public string CloseLabel => _catalog.Get("Help.Close");

    /// <summary>
    /// Secciones visibles de SC-003: Acerca de, Guía de Markdown y Atajos de
    /// teclado, con su texto resuelto en el idioma activo.
    /// </summary>
    public IReadOnlyList<HelpSectionDefinition> Sections => _sections ??= BuildSections();

    /// <summary>
    /// Sección activa. La navegación por secciones actualiza este valor
    /// (screens.md, SC-003).
    /// </summary>
    public HelpSectionDefinition? ActiveSection
    {
        get
        {
            var sections = Sections;
            return sections.FirstOrDefault(section =>
                       string.Equals(section.Key, _activeKey, StringComparison.Ordinal))
                   ?? sections[0];
        }
        set
        {
            if (value is null
                || string.Equals(value.Key, _activeKey, StringComparison.Ordinal))
            {
                return;
            }

            _activeKey = value.Key;
            OnPropertyChanged(nameof(ActiveSection));
            OnPropertyChanged(nameof(ActiveSectionKind));
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(ActiveSectionTitle));
        }
    }

    /// <summary>Texto seleccionable de la sección activa.</summary>
    public string Content => ActiveSection?.Content ?? string.Empty;

    /// <summary>Título de la sección activa.</summary>
    public string ActiveSectionTitle => ActiveSection?.Title ?? string.Empty;

    /// <summary>
    /// Versión de la aplicación mostrada en el Acerca de. Se toma del
    /// ensamblado y se muestra sin metadatos de compilación.
    /// </summary>
    public string Version { get; } = ResolveVersion();

    /// <summary>Selecciona una sección de SC-003.</summary>
    public void SelectSection(HelpSection section)
    {
        var key = ToKey(section);
        if (string.Equals(key, _activeKey, StringComparison.Ordinal))
        {
            return;
        }

        _activeKey = key;
        OnPropertyChanged(nameof(ActiveSection));
        OnPropertyChanged(nameof(ActiveSectionKind));
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(ActiveSectionTitle));
    }

    /// <summary>Sección activa actual, útil para comprobaciones de superficie.</summary>
    public HelpSection ActiveSectionKind => FromKey(_activeKey);

    /// <summary>
    /// Resuelve la versión de la aplicación (FR-010, NFR-011) desde el
    /// ensamblado: versión informativa sin el hash de compilación.
    /// </summary>
    public static string ResolveVersion()
    {
        var assembly = typeof(App).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        var version = string.IsNullOrWhiteSpace(informational)
            ? assembly.GetName().Version?.ToString()
            : informational;

        if (string.IsNullOrWhiteSpace(version))
        {
            return "1.0.0";
        }

        var metadataIndex = version.IndexOf('+', StringComparison.Ordinal);
        return metadataIndex > 0 ? version[..metadataIndex] : version;
    }

    /// <summary>
    /// Relabeliza todos los textos de la superficie al conmutar el idioma
    /// activo (COM-002, BR-013), tanto si el cambio llega por la sesión como
    /// si lo notifica la ventana principal.
    /// </summary>
    public void RefreshLanguage()
    {
        _sections = null;
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(SectionsHeader));
        OnPropertyChanged(nameof(CopyTextLabel));
        OnPropertyChanged(nameof(CloseLabel));
        OnPropertyChanged(nameof(Sections));
        OnPropertyChanged(nameof(ActiveSection));
        OnPropertyChanged(nameof(ActiveSectionTitle));
        OnPropertyChanged(nameof(Content));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_session is not null)
        {
            _session.PropertyChanged -= OnSessionPropertyChanged;
        }
    }

    private IReadOnlyList<HelpSectionDefinition> BuildSections()
    {
        return new ReadOnlyCollection<HelpSectionDefinition>(
        [
            new HelpSectionDefinition(AboutKey, _catalog.Get(AboutKey), BuildAboutContent()),
            new HelpSectionDefinition(GuideKey, _catalog.Get(GuideKey), _catalog.Get("Help.Guide.Content")),
            new HelpSectionDefinition(ShortcutsKey, _catalog.Get(ShortcutsKey), BuildShortcutsContent()),
        ]);
    }

    private string BuildAboutContent()
    {
        var builder = new StringBuilder();
        builder.AppendLine(_catalog.Get("Help.About.Name"));
        builder
            .Append(_catalog.Get("Help.About.VersionLabel"))
            .Append(": ")
            .AppendLine(Version);
        builder.AppendLine();
        builder.AppendLine(_catalog.Get("Help.About.AttributionsTitle"));
        // NFR-011: la librería de iconos de VR-001 se atribuye con nombre,
        // versión y licencia en el Acerca de de SC-003.
        builder.AppendLine(string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            _catalog.Get("Help.About.IconLibraryAttribution"),
            IconLibrary.PackageName,
            IconLibrary.Version,
            IconLibrary.IconSetName,
            IconLibrary.License));
        builder.AppendLine(_catalog.Get("Help.About.Attributions"));
        builder.AppendLine();
        builder.AppendLine(_catalog.Get("Help.About.LicenseTitle"));
        builder.Append(_catalog.Get("Help.About.License"));
        return builder.ToString();
    }

    private string BuildShortcutsContent()
    {
        var builder = new StringBuilder();
        builder.AppendLine(_catalog.Get("Help.Shortcuts.Intro"));
        builder.AppendLine();

        var shortcuts = _shortcutProvider?.Invoke() ?? Array.Empty<HelpShortcutDefinition>();
        foreach (var shortcut in shortcuts)
        {
            builder
                .Append(shortcut.Combination)
                .Append(" — ")
                .AppendLine(shortcut.Description);
        }

        builder.AppendLine();
        builder.Append(_catalog.Get("Help.Shortcuts.Pending"));
        return builder.ToString();
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Session.TrySetLanguage notifica Language una sola vez; así se evita
        // reconstruir la superficie varias veces por conmutación.
        if (e.PropertyName != nameof(Session.Language))
        {
            return;
        }

        RefreshLanguage();
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static string ToKey(HelpSection section) => section switch
    {
        HelpSection.Guide => GuideKey,
        HelpSection.Shortcuts => ShortcutsKey,
        _ => AboutKey,
    };

    private static HelpSection FromKey(string key) => key switch
    {
        GuideKey => HelpSection.Guide,
        ShortcutsKey => HelpSection.Shortcuts,
        _ => HelpSection.About,
    };
}
