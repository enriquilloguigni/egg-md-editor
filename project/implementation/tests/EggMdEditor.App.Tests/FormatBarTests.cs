using System;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using EggMdEditor.App.I18n;
using Material.Icons;
using Material.Icons.Avalonia;
using Xunit;

namespace EggMdEditor.App.Tests;

// Barra de formato de SC-001 (T-007/T-017): los 12 controles de FR-012 en
// orden de escritura (ENT-005), cada uno con su icono de la librería
// MIT-compatible (VR-001), su aviso flotante (VR-002) y su nombre accesible
// (NFR-009), sin acciones conectadas (ADR-001).
public class FormatBarTests
{
    private static readonly string[] ExpectedKeys =
    [
        "barra.enfasis",
        "barra.encabezados",
        "barra.listas",
        "barra.cita",
        "barra.codigo",
        "barra.enlace",
        "barra.imagen",
        "barra.tabla",
        "barra.nota-al-pie",
        "barra.formula",
        "barra.diagrama",
        "barra.indice",
    ];

    private static readonly string[] ExpectedSpanishNames =
    [
        "Énfasis",
        "Encabezados",
        "Listas",
        "Cita",
        "Código",
        "Enlace",
        "Imagen",
        "Tabla",
        "Nota al pie",
        "Fórmula",
        "Diagrama",
        "Índice",
    ];

    private static readonly string[] ExpectedEnglishNames =
    [
        "Emphasis",
        "Headings",
        "Lists",
        "Quote",
        "Code",
        "Link",
        "Image",
        "Table",
        "Footnote",
        "Formula",
        "Diagram",
        "Table of contents",
    ];

    private static readonly MaterialIconKind[] ExpectedIcons =
    [
        MaterialIconKind.FormatBold,
        MaterialIconKind.FormatHeader1,
        MaterialIconKind.FormatListBulleted,
        MaterialIconKind.FormatQuoteClose,
        MaterialIconKind.CodeTags,
        MaterialIconKind.LinkVariant,
        MaterialIconKind.ImageOutline,
        MaterialIconKind.Table,
        MaterialIconKind.FormatSuperscript,
        MaterialIconKind.FunctionVariant,
        MaterialIconKind.FlowchartOutline,
        MaterialIconKind.Toc,
    ];

    [Fact]
    public void ViewModel_ExposesTheTwelveFr012ControlsInWritingOrder()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());

        Assert.Equal(ExpectedKeys, MainWindowViewModel.FormatBarKeys);
        Assert.Equal(ExpectedKeys, viewModel.FormatBarControls.Select(control => control.Key));
        Assert.Equal(ExpectedSpanishNames, viewModel.FormatBarControls.Select(control => control.Name));
    }

    [Fact]
    public void EveryControlHasAVisibleVectorIconFromTheIconLibrary()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());

        Assert.Equal(ExpectedKeys.Length, MainWindowViewModel.FormatBarIcons.Count);
        Assert.All(ExpectedKeys, key => Assert.True(MainWindowViewModel.FormatBarIcons.ContainsKey(key)));
        Assert.All(viewModel.FormatBarControls, control =>
        {
            Assert.Equal(MainWindowViewModel.FormatBarIcons[control.Key], control.Icon);
            Assert.True(Enum.IsDefined(control.Icon));

            // VR-001: cada control tiene un icono vectorial propio (un icono
            // por control) de la librería MIT-compatible, nunca un glifo.
            Assert.False(string.IsNullOrWhiteSpace(MaterialIconDataProvider.GetData(control.Icon)));
        });
        Assert.Equal(
            ExpectedIcons,
            ExpectedKeys.Select(key => MainWindowViewModel.FormatBarIcons[key]));
        Assert.Equal(ExpectedIcons.Length, viewModel.FormatBarControls.Select(control => control.Icon).Distinct().Count());
    }

    [Fact]
    public void ViewModel_ResolvesNamesInTheActiveLanguage()
    {
        var catalog = new I18nCatalog();
        Assert.True(catalog.TrySetLanguage("en"));

        var viewModel = new MainWindowViewModel(catalog);

        Assert.Equal(ExpectedEnglishNames, viewModel.FormatBarControls.Select(control => control.Name));
    }

    [AvaloniaFact]
    public void Window_ShowsTheBarWithIconTooltipAndAccessibleNamePerControl()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var bar = window.FindControl<ItemsControl>("FormatBar");
            Assert.NotNull(bar);
            Assert.Equal(12, bar.ItemCount);

            var buttons = bar.GetVisualDescendants().OfType<Button>().ToList();
            Assert.Equal(12, buttons.Count);
            Assert.Equal(ExpectedKeys, buttons.Select(button => button.Tag as string));
            Assert.Equal(ExpectedSpanishNames, buttons.Select(ToolTip.GetTip));
            Assert.Equal(ExpectedSpanishNames, buttons.Select(AutomationProperties.GetName));

            Assert.All(buttons, button =>
            {
                // VR-001: icono vectorial de la librería MIT-compatible, no un
                // glifo de texto.
                var icon = Assert.IsType<MaterialIcon>(button.Content);
                Assert.NotNull(icon.Kind);
                Assert.NotNull(icon.Drawing);
                var key = Assert.IsType<string>(button.Tag);
                Assert.Equal<MaterialIconKind?>(MainWindowViewModel.FormatBarIcons[key], icon.Kind);
                Assert.Empty(button.GetVisualDescendants().OfType<TextBlock>());

                // VR-002: aviso de una línea, en el idioma activo.
                var tooltip = Assert.IsType<string>(ToolTip.GetTip(button));
                Assert.DoesNotContain('\n', tooltip);

                // NFR-009: el nombre accesible no se sustituye por el aviso.
                var accessibleName = AutomationProperties.GetName(button);
                Assert.False(string.IsNullOrWhiteSpace(accessibleName));
                Assert.Equal(tooltip, accessibleName);

                // ADR-001: control visible, sin comando ni acción conectada.
                Assert.Null(button.Command);
                Assert.True(button.IsEnabled);
                Assert.True(button.IsVisible);
            });
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Window_KeepsTheBarVisibleAboveTheEmptyDocumentArea()
    {
        var viewModel = new MainWindowViewModel(new I18nCatalog());
        var window = new MainWindow(viewModel);

        try
        {
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var container = window.FindControl<Border>("FormatBarContainer");
            var documentArea = window.FindControl<Border>("DocumentArea");

            Assert.NotNull(container);
            Assert.NotNull(documentArea);
            Assert.Equal(1, Grid.GetRow(container));
            Assert.Equal(2, Grid.GetRow(documentArea));
            Assert.Same(viewModel, window.DataContext);
        }
        finally
        {
            window.Close();
        }
    }
}
