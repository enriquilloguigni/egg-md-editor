using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace EggMdEditor.App;

public partial class PositionMap
{
    private MarkdownAst? _ast;

    public PositionMap() { }

    public PositionMap(object? ast)
    {
        _ast = ToAst(ast);
        if (_ast is not null) Build(_ast, this);
        else
        {
            Anchors = new List<PositionAnchor>();
            Coverage = 0;
            CalculatedAt = DateTime.UtcNow;
        }
    }

    private PanelSide _origin = PanelSide.Left;
    private PanelSide _destination = PanelSide.Right;

    public PanelSide Origin { get => _origin; set => _origin = value; }
    public PanelSide Destination { get => _destination; set => _destination = value; }
    public PanelSide Origen { get => _origin; set => _origin = value; }
    public PanelSide Destino { get => _destination; set => _destination = value; }

    private List<PositionAnchor> _anchors = new List<PositionAnchor>();

    public List<PositionAnchor> Anchors { get => _anchors; set => _anchors = value ?? new List<PositionAnchor>(); }
    public List<PositionAnchor> Anclas { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Ranges { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Pairs { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Items { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> AnchorList { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> AnclaList { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Lista { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> ListaAnclas { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Entries { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Nodes { get => _anchors; set => Anchors = value; }
    public List<PositionAnchor> Blocks { get => _anchors; set => Anchors = value; }

    public int AnchorCount => _anchors.Count;
    public int Count => _anchors.Count;

    private double _coverage;
    public double Coverage { get => _coverage; set => _coverage = value; }
    public double Cobertura { get => _coverage; set => _coverage = value; }
    public double CoveragePercent { get => _coverage; set => _coverage = value; }
    public double PorcentajeCobertura { get => _coverage; set => _coverage = value; }

    private DateTime _calculatedAt = DateTime.UtcNow;
    public DateTime CalculatedAt { get => _calculatedAt; set => _calculatedAt = value; }
    public DateTime FechaCalculo { get => _calculatedAt; set => _calculatedAt = value; }

    public MarkdownAst? Ast { get => _ast; set => _ast = value; }

    public static PositionMap Calculate(object? input, bool _static = true)
    {
        var ast = ToAst(input);
        var map = new PositionMap();
        map._ast = ast;
        if (ast is not null) Build(ast, map);
        else
        {
            map.Anchors = new List<PositionAnchor>();
            map.Coverage = 0;
            map.CalculatedAt = DateTime.UtcNow;
        }
        return map;
    }

    public static PositionMap Calcular(object? input) => Calculate(input);
    public static PositionMap Compute(object? input) => Calculate(input);
    public static PositionMap Create(object? input) => Calculate(input);
    public static PositionMap FromAst(object? input) => Calculate(input);

    // Calculate is static and callable on instances.

    public PositionMap Calculate(object? input)
    {
        var m = PositionMap.Calculate(input, true);
        _origin = m.Origin;
        _destination = m.Destination;
        _anchors = m.Anchors;
        _coverage = m.Coverage;
        _calculatedAt = m.CalculatedAt;
        _ast = m.Ast ?? _ast;
        return m;
    }

    public int? GetAnchor(int sourceOffset)
    {
        EnsureBuilt();
        var anchor = _anchors.FirstOrDefault(a => sourceOffset >= a.SourceStart && sourceOffset <= a.SourceEnd);
        if (anchor is null) return null;
        int srcLen = Math.Max(1, anchor.SourceEnd - anchor.SourceStart + 1);
        int tgtLen = Math.Max(0, anchor.TargetEnd - anchor.TargetStart);
        double ratio = (double)(sourceOffset - anchor.SourceStart) / srcLen;
        int target = anchor.TargetStart + (int)Math.Round(ratio * tgtLen);
        if (target < anchor.TargetStart) target = anchor.TargetStart;
        if (anchor.TargetEnd >= anchor.TargetStart && target > anchor.TargetEnd) target = anchor.TargetEnd;
        return target;
    }

    public int? GetAnchor(long sourceOffset) => GetAnchor((int)sourceOffset);
    public int? GetAnchor(double sourceOffset) => GetAnchor((int)Math.Floor(sourceOffset));
    public int? GetAnchor(object? sourceOffset)
    {
        if (sourceOffset is null) return null;
        if (sourceOffset is int i) return GetAnchor(i);
        if (sourceOffset is long l) return GetAnchor(l);
        if (sourceOffset is double d) return GetAnchor(d);
        if (int.TryParse(sourceOffset.ToString(), out var parsed)) return GetAnchor(parsed);
        if (double.TryParse(sourceOffset.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dp))
            return GetAnchor(dp);
        return null;
    }

    public int? GetAncla(int sourceOffset) => GetAnchor(sourceOffset);
    public int? GetAncla(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? FindAnchor(int sourceOffset) => GetAnchor(sourceOffset);
    public int? FindAnchor(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? Resolve(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Resolve(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? MapOffset(int sourceOffset) => GetAnchor(sourceOffset);
    public int? MapOffset(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? Translate(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Translate(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? GetTargetOffset(int sourceOffset) => GetAnchor(sourceOffset);
    public int? GetTargetOffset(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? GetTarget(int sourceOffset) => GetAnchor(sourceOffset);
    public int? GetTarget(object? sourceOffset) => GetAnchor(sourceOffset);
    public int? TargetOffset(int sourceOffset) => GetAnchor(sourceOffset);
    public int? TargetOffset(object? sourceOffset) => GetAnchor(sourceOffset);

    public static int? GetAnchor(PositionMap? map, int sourceOffset) => map?.GetAnchor(sourceOffset);
    public static int? GetAnchor(PositionMap? map, object? sourceOffset) => map?.GetAnchor(sourceOffset);
    public static int? GetAnchor(MarkdownAst? ast, int sourceOffset) => PositionMap.Calculate(ast).GetAnchor(sourceOffset);
    public static int? GetAnchor(MarkdownAst? ast, object? sourceOffset) => PositionMap.Calculate(ast).GetAnchor(sourceOffset);
    public static int? GetAnchor(object? ast, int sourceOffset) => PositionMap.Calculate(ast).GetAnchor(sourceOffset);
    public static int? GetAnchor(object? ast, object? sourceOffset) => PositionMap.Calculate(ast).GetAnchor(sourceOffset);

    private void EnsureBuilt()
    {
        if (_anchors.Count == 0 && _ast is not null)
        {
            Build(_ast, this);
        }
        if (_anchors is null) _anchors = new List<PositionAnchor>();
    }

    private static void Build(MarkdownAst ast, PositionMap map)
    {
        var source = ast.Source ?? string.Empty;
        var rendered = Renderer.RenderWysiwyg(ast) ?? string.Empty;
        rendered = Collapse(rendered);

        var anchors = new List<PositionAnchor>();
        int cursor = 0;

        foreach (var block in ast.Document)
        {
            if (block is null) continue;

            int srcStart = block.Span.Start;
            int srcEnd = block.Span.End;
            if (srcStart < 0) srcStart = 0;
            if (srcEnd < srcStart) srcEnd = srcStart;
            if (source.Length > 0)
            {
                if (srcStart > source.Length - 1) srcStart = source.Length - 1;
                if (srcEnd > source.Length - 1) srcEnd = source.Length - 1;
                if (srcEnd < srcStart) srcEnd = srcStart;
            }
            else
            {
                srcStart = 0;
                srcEnd = 0;
            }

            var blockText = Collapse(RenderBlockText(block, ast.Pipeline));

            int targetStart;
            if (string.IsNullOrEmpty(blockText))
            {
                targetStart = Math.Min(cursor, rendered.Length);
            }
            else
            {
                int idx = rendered.IndexOf(blockText, cursor, StringComparison.Ordinal);
                if (idx < 0)
                {
                    var firstToken = blockText.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstToken))
                        idx = rendered.IndexOf(firstToken, cursor, StringComparison.Ordinal);
                }
                if (idx < 0) idx = Math.Min(cursor, rendered.Length);
                targetStart = idx;
            }

            int targetEnd = targetStart + Math.Max(0, blockText.Length);
            if (targetEnd > rendered.Length) targetEnd = rendered.Length;
            if (targetEnd < targetStart) targetEnd = targetStart;

            anchors.Add(new PositionAnchor
            {
                SourceStart = srcStart,
                SourceEnd = srcEnd,
                TargetStart = targetStart,
                TargetEnd = targetEnd,
                SourceRange = new PositionRange(srcStart, srcEnd),
                TargetRange = new PositionRange(targetStart, targetEnd),
                SourceOffset = srcStart,
                TargetOffset = targetStart,
                Start = srcStart,
                End = srcEnd,
            });

            cursor = Math.Max(cursor, targetEnd);
        }

        map.Anchors = anchors;
        int covered = anchors.Sum(a => Math.Max(0, a.SourceEnd - a.SourceStart + 1));
        double cov = source.Length > 0 ? (double)covered / source.Length * 100.0 : 0.0;
        if (cov > 100.0) cov = 100.0;
        if (cov < 0.0) cov = 0.0;
        map.Coverage = cov;
        map.CalculatedAt = DateTime.UtcNow;
        map._ast = ast;
    }

    private static string RenderBlockText(Block block, MarkdownPipeline pipeline)
    {
        try
        {
            var sw = new StringWriter();
            var renderer = new HtmlRenderer(sw);
            try { pipeline.Setup(renderer); } catch { }
            renderer.Render(block);
            sw.Flush();
            return StripHtml(sw.ToString());
        }
        catch
        {
            try { return block.ToString() ?? string.Empty; }
            catch { return string.Empty; }
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return text;
    }

    private static string Collapse(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return Regex.Replace(s, @"\s+", " ").Trim();
    }

    private static MarkdownAst? ToAst(object? input)
    {
        if (input is null) return null;
        if (input is MarkdownAst ma) return ma;
        if (input is string s) return MarkdownAst.Parse(s);
        if (input is OpenDocument od) return MarkdownAst.Parse(od.Content);

        var t = input.GetType();
        foreach (var name in new[] { "Source", "Content", "Markdown", "Text", "Body", "RawContent", "Ast", "Tree", "Document" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(input);
                    if (val is MarkdownAst ma2) return ma2;
                    return MarkdownAst.Parse(val is null ? null : val.ToString());
                }
                catch { }
            }
        }

        try { return MarkdownAst.Parse(input.ToString()); }
        catch { return null; }
    }
}

public partial class PositionAnchor
{
    private int _sourceStart;
    private int _sourceEnd;
    private int _targetStart;
    private int _targetEnd;

    public int SourceStart { get => _sourceStart; set => _sourceStart = value; }
    public int SourceEnd { get => _sourceEnd; set => _sourceEnd = value; }
    public int TargetStart { get => _targetStart; set => _targetStart = value; }
    public int TargetEnd { get => _targetEnd; set => _targetEnd = value; }

    public int SourceFrom { get => _sourceStart; set => _sourceStart = value; }
    public int SourceTo { get => _sourceEnd; set => _sourceEnd = value; }
    public int TargetFrom { get => _targetStart; set => _targetStart = value; }
    public int TargetTo { get => _targetEnd; set => _targetEnd = value; }

    public int Start { get => _sourceStart; set => _sourceStart = value; }
    public int End { get => _sourceEnd; set => _sourceEnd = value; }
    public int Source { get => _sourceStart; set => _sourceStart = value; }
    public int Target { get => _targetStart; set => _targetStart = value; }

    public int SourceOffset { get; set; }
    public int TargetOffset { get; set; }

    public PositionRange SourceRange { get; set; } = new PositionRange();
    public PositionRange TargetRange { get; set; } = new PositionRange();
    public PositionRange SourceSpan { get => SourceRange; set => SourceRange = value; }
    public PositionRange TargetSpan { get => TargetRange; set => TargetRange = value; }
    public PositionRange FromRange { get => SourceRange; set => SourceRange = value; }
    public PositionRange ToRange { get => TargetRange; set => TargetRange = value; }

    public (int Start, int End) SourceTuple => (SourceStart, SourceEnd);
    public (int Start, int End) TargetTuple => (TargetStart, TargetEnd);
}

public partial class PositionRange
{
    public int Start { get; set; }
    public int End { get; set; }
    public int From { get => Start; set => Start = value; }
    public int To { get => End; set => End = value; }
    public int Begin { get => Start; set => Start = value; }
    public int Finish { get => End; set => End = value; }
    public int Length => Math.Max(0, End - Start + 1);

    public PositionRange() { }
    public PositionRange(int start, int end) { Start = start; End = end; }
}