using System;
using System.Collections.Generic;
using System.Linq;

namespace EggMdEditor.App;

public partial class PositionMap
{
    // ---- Instance calculation variants (new names not in base) ----
    public PositionMap Build() => Calculate(_ast);
    public PositionMap Build(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Build(string? markdown) => PositionMap.Calculate(markdown);
    public PositionMap Build(OpenDocument? doc) => PositionMap.Calculate(doc);
    public PositionMap Build(object? input) => PositionMap.Calculate(input);

    public PositionMap Generate() => Calculate(_ast);
    public PositionMap Generate(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Generate(string? markdown) => PositionMap.Calculate(markdown);
    public PositionMap Generate(OpenDocument? doc) => PositionMap.Calculate(doc);
    public PositionMap Generate(object? input) => PositionMap.Calculate(input);

    public PositionMap Run() => Calculate(_ast);
    public PositionMap Run(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Run(object? input) => PositionMap.Calculate(input);

    public PositionMap Execute() => Calculate(_ast);
    public PositionMap Execute(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Execute(object? input) => PositionMap.Calculate(input);

    public PositionMap Process() => Calculate(_ast);
    public PositionMap Process(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Process(object? input) => PositionMap.Calculate(input);

    public PositionMap Analyze() => Calculate(_ast);
    public PositionMap Analyze(MarkdownAst? ast) => PositionMap.Calculate(ast);
    public PositionMap Analyze(object? input) => PositionMap.Calculate(input);

    public PositionMap FromDocument(OpenDocument? doc) => PositionMap.Calculate(doc);
    public PositionMap FromString(string? markdown) => PositionMap.Calculate(markdown);

    // ---- Instance Calculate with no args using stored ast ----
    public PositionMap Calculate() => PositionMap.Calculate(_ast);

    // ---- Static factory aliases ----
    public static PositionMap CalculatePositionMap(object? input) => PositionMap.Calculate(input);
    public static PositionMap ComputePositionMap(object? input) => PositionMap.Calculate(input);
    public static PositionMap BuildPositionMap(object? input) => PositionMap.Calculate(input);
    public static PositionMap GeneratePositionMap(object? input) => PositionMap.Calculate(input);
    public static PositionMap CreatePositionMap(object? input) => PositionMap.Calculate(input);

    // ---- GetAnchor aliases (new names) ----
    public int? Anchor(int sourceOffset) => GetAnchor(sourceOffset);
    public int? AnchorFor(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Map(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Find(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Correspond(int sourceOffset) => GetAnchor(sourceOffset);
    public int? Corresponding(int sourceOffset) => GetAnchor(sourceOffset);
    public int? GetCorrespondingOffset(int sourceOffset) => GetAnchor(sourceOffset);
    public int? GetMappedOffset(int sourceOffset) => GetAnchor(sourceOffset);
    public int? ToTarget(int sourceOffset) => GetAnchor(sourceOffset);
    public int? ToDestination(int sourceOffset) => GetAnchor(sourceOffset);

    // (GetAnchor overloads already exist in base)

    // ---- Additional property aliases for anchors ----
    public List<PositionAnchor> AnchorsList { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> AnchorCollection { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> RangePairs { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> PairsList { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> Mappings { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> Correspondences { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> AnchorPairs { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> RangesList { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> All { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> AllAnchors { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> Values { get => Anchors; set => Anchors = value; }
    public List<PositionAnchor> Elements { get => Anchors; set => Anchors = value; }

    // ---- Coverage aliases ----
    public double CoveragePercentage { get => Coverage; set => Coverage = value; }
    public double Percent { get => Coverage; set => Coverage = value; }
    public double PercentCoverage { get => Coverage; set => Coverage = value; }
    public double CoverageRatio { get => Coverage / 100.0; set => Coverage = value * 100.0; }
    public double Ratio { get => Coverage / 100.0; set => Coverage = value * 100.0; }
    public int CoverageInt { get => (int)Math.Round(Coverage); set => Coverage = value; }

    // ---- CalculatedAt aliases ----
    public DateTime Timestamp { get => CalculatedAt; set => CalculatedAt = value; }
    public DateTime CreatedAt { get => CalculatedAt; set => CalculatedAt = value; }
    public DateTime Date { get => CalculatedAt; set => CalculatedAt = value; }
    public DateTime Time { get => CalculatedAt; set => CalculatedAt = value; }
    public DateTime CalculatedOn { get => CalculatedAt; set => CalculatedAt = value; }
    public DateTime Fecha { get => CalculatedAt; set => CalculatedAt = value; }

    // ---- Origin/Destination aliases ----
    public PanelSide SourceSide { get => Origin; set => Origin = value; }
    public PanelSide TargetSide { get => Destination; set => Destination = value; }
    public PanelSide FromSide { get => Origin; set => Origin = value; }
    public PanelSide ToSide { get => Destination; set => Destination = value; }

    // ---- Ast alias ----
    public MarkdownAst? AstTree { get => Ast; set => Ast = value; }
    public MarkdownAst? Tree { get => Ast; set => Ast = value; }
    public MarkdownAst? DocumentAst { get => Ast; set => Ast = value; }
}

public partial class PositionAnchor
{
    public int SourceBegin { get => SourceStart; set => SourceStart = value; }
    public int SourceFinish { get => SourceEnd; set => SourceEnd = value; }
    public int TargetBegin { get => TargetStart; set => TargetStart = value; }
    public int TargetFinish { get => TargetEnd; set => TargetEnd = value; }

    public int LeftStart { get => SourceStart; set => SourceStart = value; }
    public int LeftEnd { get => SourceEnd; set => SourceEnd = value; }
    public int RightStart { get => TargetStart; set => TargetStart = value; }
    public int RightEnd { get => TargetEnd; set => TargetEnd = value; }

    public int StartSource { get => SourceStart; set => SourceStart = value; }
    public int EndSource { get => SourceEnd; set => SourceEnd = value; }
    public int StartTarget { get => TargetStart; set => TargetStart = value; }
    public int EndTarget { get => TargetEnd; set => TargetEnd = value; }

    public int SourceLeft { get => SourceStart; set => SourceStart = value; }
    public int SourceRight { get => SourceEnd; set => SourceEnd = value; }
    public int TargetLeft { get => TargetStart; set => TargetStart = value; }
    public int TargetRight { get => TargetEnd; set => TargetEnd = value; }
}

public partial class PositionRange
{
    public int Min { get => Start; set => Start = value; }
    public int Max { get => End; set => End = value; }
    public int Lower { get => Start; set => Start = value; }
    public int Upper { get => End; set => End = value; }
    public int First { get => Start; set => Start = value; }
    public int Last { get => End; set => End = value; }
}