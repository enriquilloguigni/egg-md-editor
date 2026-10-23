using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Platform;
using AvaloniaThemeVariant = Avalonia.Styling.ThemeVariant;

namespace EggMdEditor.App;

/// <summary>
/// Aplicación del tema de la interfaz (OP-002, COM-003): recibe el tema
/// {Claro,Oscuro} de ENT-003 y lo aplica a las superficies de la aplicación de
/// forma síncrona.
/// </summary>
public interface IThemeApplier
{
    /// <summary>Tema efectivo aplicado en este momento.</summary>
    Theme AppliedTheme { get; }

    /// <summary>Aplica el tema indicado (OP-002).</summary>
    void Apply(Theme theme);
}

/// <summary>
/// Implementación sobre Avalonia (FluentTheme): fija
/// <see cref="Application.RequestedThemeVariant"/> en la raíz de la aplicación
/// para que el repintado sea consistente en todas las ventanas y controles de
/// SC-001..SC-003 (COM-003). En F-001 la preferencia no se persiste: cada
/// arranque hereda el tema del SO (ENT-003) y la primera conmutación lo fija
/// en la sesión, sin cambio parcial ante una entrada inválida.
/// </summary>
public sealed class ApplicationThemeApplier : IThemeApplier, IDisposable
{
    private readonly Application _application;
    private Session? _session;

    public ApplicationThemeApplier(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        _application = application;
    }

    public Theme AppliedTheme => FromThemeVariant(_application.ActualThemeVariant);

    public void Apply(Theme theme)
    {
        if (theme is not (Theme.Light or Theme.Dark))
        {
            return;
        }

        _application.RequestedThemeVariant = ToThemeVariant(theme);
    }

    /// <summary>
    /// Sigue el tema de la sesión (ENT-001/ENT-003): toda conmutación de OP-002,
    /// venga del ViewModel o de las preferencias, se aplica a la aplicación sin
    /// recrear ventanas ni perder el resto del estado de la sesión.
    /// </summary>
    public void Attach(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!ReferenceEquals(_session, session))
        {
            Detach();
            _session = session;
            session.PropertyChanged += OnSessionPropertyChanged;
        }

        Apply(session.Theme);
    }

    public void Detach()
    {
        if (_session is null)
        {
            return;
        }

        _session.PropertyChanged -= OnSessionPropertyChanged;
        _session = null;
    }

    public void Dispose() => Detach();

    /// <summary>
    /// Tema inicial heredado del SO (ENT-003, design-system). En F-001 no hay
    /// preferencia persistida; si la plataforma no informa del tema del SO se
    /// conserva ST-009 (claro), que es el estado inicial declarado.
    /// </summary>
    public static Theme ResolveSystemTheme(Application? application = null)
    {
        try
        {
            return ResolveSystemTheme((application ?? Application.Current)?.PlatformSettings);
        }
        catch (Exception)
        {
            // Sin información fiable del SO: tema inicial claro (ST-009).
            return Theme.Light;
        }
    }

    /// <summary>
    /// Traduce las preferencias de plataforma al tema de ENT-003. Sin
    /// información del SO se conserva el tema inicial claro (ST-009).
    /// </summary>
    public static Theme ResolveSystemTheme(IPlatformSettings? platformSettings)
    {
        var colorValues = platformSettings?.GetColorValues();
        return colorValues is null ? Theme.Light : ResolveSystemTheme(colorValues.ThemeVariant);
    }

    /// <summary>Traduce el tema informado por el SO al tema de ENT-003.</summary>
    public static Theme ResolveSystemTheme(PlatformThemeVariant systemTheme) =>
        systemTheme == PlatformThemeVariant.Dark ? Theme.Dark : Theme.Light;

    /// <summary>Tema de ENT-003 → variante de tema de Avalonia (COM-003).</summary>
    public static AvaloniaThemeVariant ToThemeVariant(Theme theme) =>
        theme == Theme.Dark ? AvaloniaThemeVariant.Dark : AvaloniaThemeVariant.Light;

    /// <summary>Variante de tema de Avalonia → tema de ENT-003.</summary>
    public static Theme FromThemeVariant(AvaloniaThemeVariant? themeVariant) =>
        themeVariant == AvaloniaThemeVariant.Dark ? Theme.Dark : Theme.Light;

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Session session)
        {
            return;
        }

        if (e.PropertyName is nameof(Session.Theme)
            or nameof(Session.CurrentTheme)
            or nameof(Session.ActiveTheme)
            or nameof(Session.Tema))
        {
            Apply(session.Theme);
        }
    }
}
