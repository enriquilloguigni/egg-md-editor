using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;

namespace EggMdEditor.App.Tests;

// Bootstrap of Avalonia's headless platform for the tests (once per
// assembly): it allows building SC-001 controls and windows —like the
// AC-015 empty document area check (T-010)— without a real window, without
// a screen and without network (C-001). The tests that do not need it are
// not affected.
internal static class AvaloniaTestBootstrapper
{
    [ModuleInitializer]
    internal static void Initialize() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
}
