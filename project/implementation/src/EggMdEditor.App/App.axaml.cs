using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;

namespace EggMdEditor.App;

public partial class App : Application
{
    private readonly ApplicationSession? _processSession;
    private readonly LifecycleLogger? _lifecycleLogger;

    /// <summary>
    /// Parameterless constructor for the XAML designer and headless tests: the
    /// window it creates will create its own session with the Spec defaults
    /// (Spanish language, BR-007), with no process logger.
    /// </summary>
    public App() : this(null, null)
    {
    }

    /// <summary>
    /// Creates the process's application (T-012) over the startup's new
    /// session — always in Spanish, with no persistence (BR-F001-006) — and
    /// its lifecycle logger (T-007); with either of them absent (designer,
    /// tests), the window operates with its own defaults.
    /// </summary>
    public App(ApplicationSession? processSession, LifecycleLogger? lifecycleLogger)
    {
        _processSession = processSession;
        _lifecycleLogger = lifecycleLogger;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // SC-001 (BR-003): single window sharing the process's session —
            // one session per run, log correlation (NFR-003) — and its
            // lifecycle logger, so its close emits LOG-003 and applies
            // ERR-005 (COM-004, T-011). If creation fails, the exception goes
            // up to the entry point: ERR-001, no window presented (T-012).
            var window = new MainWindow(
                _processSession ?? new ApplicationSession(),
                lifecycleLogger: _lifecycleLogger);
            desktop.MainWindow = window;

            if (_lifecycleLogger is not null)
            {
                window.Opened += OnWindowShown;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// LOG-001 (T-012): the single window was presented complete — created and
    /// opened, with the startup painting not failing (ERR-002) —; the end of
    /// the initialization is logged exactly once, with the i18n catalog loaded
    /// and the session's active language.
    /// </summary>
    private void OnWindowShown(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.Opened -= OnWindowShown;
        }

        _lifecycleLogger?.LogInitializationEnd();
    }
}
