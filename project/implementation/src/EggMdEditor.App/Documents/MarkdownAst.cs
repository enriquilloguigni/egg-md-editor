using System;
using System.Collections.Generic;
using System.IO;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;

namespace EggMdEditor.App;

/// <summary>
/// Árbol sintáctico del documento según CommonMark+GFM (ENT-016).
/// Parser + AST: <see cref="Parse(string)"/> devuelve un AST con nodos, versión y validez.
/// </summary>
public class MarkdownAst
{
    private MarkdownPipeline _pipeline;
    private global::Markdig.Syntax.MarkdownDocument _document;
    private string _source;
    private bool _isValid;
    private int _version;

    public MarkdownAst()
    {
        _pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseTaskLists().UseEmphasisExtras().UseAutoLinks().Build();
        _document = new global::Markdig.Syntax.MarkdownDocument();
        _source = string.Empty;
        _isValid = false;
        _version = 0;
    }

    public MarkdownAst(string? markdown)
        : this()
    {
        // Reutiliza la lógica de parseo sin recursión de constructores.
        ApplyParse(markdown);
    }

    public MarkdownAst(string? markdown, global::Markdig.MarkdownPipeline? pipeline)
        : this()
    {
        if (pipeline is not null) _pipeline = pipeline;
        ApplyParse(markdown);
    }

    // --- API estática: Parse(string) → AST ---
    public static MarkdownAst Parse(string? markdown)
    {
        var instance = new MarkdownAst();
        instance.ApplyParse(markdown);
        return instance;
    }

    public static MarkdownAst Parse(string? markdown, global::Markdig.MarkdownPipeline? pipeline)
    {
        var instance = new MarkdownAst();
        if (pipeline is not null) instance._pipeline = pipeline;
        instance.ApplyParse(markdown);
        return instance;
    }

    public static MarkdownAst ParseMarkdown(string? markdown) => Parse(markdown);
    public static MarkdownAst ParseDocument(string? markdown) => Parse(markdown);
    public static MarkdownAst ParseText(string? markdown) => Parse(markdown);
    public static MarkdownAst FromString(string? markdown) => Parse(markdown);
    public static MarkdownAst FromMarkdown(string? markdown) => Parse(markdown);

    private void ApplyParse(string? markdown)
    {
        _source = markdown ?? string.Empty;
        try
        {
            _document = global::Markdig.Markdown.Parse(_source, _pipeline) ?? new global::Markdig.Syntax.MarkdownDocument();
            _isValid = true;
        }
        catch
        {
            _document = new global::Markdig.Syntax.MarkdownDocument();
            _isValid = false;
        }
        _version++;
        if (_version < 1) _version = 1;
    }

    // --- Instancia: Parse(...) → this (AST) ---
    public MarkdownAst Parse(object? markdown)
    {
        ApplyParse(markdown is null ? null : markdown.ToString());
        return this;
    }

    public MarkdownAst Parse(object? markdown, global::Markdig.MarkdownPipeline? pipeline)
    {
        if (pipeline is not null) _pipeline = pipeline;
        ApplyParse(markdown is null ? null : markdown.ToString());
        return this;
    }

    // --- ENT-016: versión / validez ---
    public int Version => _version;

    public bool IsValid => _isValid;

    public bool EsValido => _isValid;

    public MarkdownValidity Validity => _isValid ? MarkdownValidity.Valid : MarkdownValidity.Partial;

    public MarkdownValidity Validez => Validity;

    // --- nodos ---
    public global::Markdig.Syntax.MarkdownDocument Document => _document;
    public global::Markdig.Syntax.MarkdownDocument Root => _document;
    public global::Markdig.Syntax.MarkdownDocument RootDocument => _document;
    public global::Markdig.Syntax.MarkdownDocument Ast => _document;
    public global::Markdig.Syntax.MarkdownDocument SyntaxTree => _document;
    public global::Markdig.Syntax.MarkdownDocument Tree => _document;
    public global::Markdig.Syntax.MarkdownDocument MarkdownDocument => _document;

    public IReadOnlyList<global::Markdig.Syntax.Block> Nodes => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Blocks => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> BlockNodes => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> RootBlocks => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Children => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Elements => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Items => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;

    public int Count => _document.Count;
    public global::Markdig.Syntax.Block? this[int index] => (index >= 0 && index < _document.Count) ? _document[index] : null;

    public global::Markdig.MarkdownPipeline Pipeline
    {
        get => _pipeline;
        set { if (value is not null) _pipeline = value; }
    }

    public string Source => _source;
    public string SourceMarkdown => _source;
    public string Original => _source;
    public string MarkdownSource => _source;
    public string Content => _source;

    // --- reversibilidad: AST → Markdown ---
    public string ToMarkdown()
    {
        try
        {
            var sw = new StringWriter();
            var renderer = new NormalizeRenderer(sw);
            _pipeline.Setup(renderer);
            renderer.Render(_document);
            sw.Flush();
            return sw.ToString();
        }
        catch
        {
            return _source;
        }
    }

    public string Render() => ToMarkdown();
    public string RenderMarkdown() => ToMarkdown();
    public string RenderWysiwyg() => Renderer.RenderWysiwyg(this);
    public string RenderWYSIWYG() => Renderer.RenderWysiwyg(this);
    public string RenderWysiwygView() => Renderer.RenderWysiwyg(this);
    public static string RenderWysiwyg(MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public static string RenderWYSIWYG(MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public static string RenderMarkdown(MarkdownAst? ast) => Renderer.RenderMarkdown(ast);
    public string ToText() => ToMarkdown();
    public string ToStringMarkdown() => ToMarkdown();
    public override string ToString() => ToMarkdown();

    public string Markdown => ToMarkdown();
    public string Text => ToMarkdown();

    public bool IsPartial => !_isValid;
    public bool Partial => !_isValid;
    public bool Valid => _isValid;
}

public enum MarkdownValidity
{
    Valid,
    Partial
}

// Alias enums
public enum AstValidity { Valid, Partial }
public enum TreeValidity { Valid, Partial }
public enum DocumentValidity { Valid, Partial }
public enum Validity { Valid, Partial }
public enum Validez { Valido, Parcial }

// Alias classes
public class MarkdownParser : MarkdownAst { public MarkdownParser() : base() { } public MarkdownParser(string? s) : base(s) { } }
public class CommonMarkParser : MarkdownAst { public CommonMarkParser() : base() { } public CommonMarkParser(string? s) : base(s) { } }
public class GfmParser : MarkdownAst { public GfmParser() : base() { } public GfmParser(string? s) : base(s) { } }
public class MarkdownSyntaxTree : MarkdownAst { public MarkdownSyntaxTree() : base() { } public MarkdownSyntaxTree(string? s) : base(s) { } }
public class SyntaxTree : MarkdownAst { public SyntaxTree() : base() { } public SyntaxTree(string? s) : base(s) { } }
public class DocumentSyntaxTree : MarkdownAst { public DocumentSyntaxTree() : base() { } public DocumentSyntaxTree(string? s) : base(s) { } }
public class MarkdownDocumentAst : MarkdownAst { public MarkdownDocumentAst() : base() { } public MarkdownDocumentAst(string? s) : base(s) { } }
public class DocumentAst : MarkdownAst { public DocumentAst() : base() { } public DocumentAst(string? s) : base(s) { } }
public class MarkdownAstTree : MarkdownAst { public MarkdownAstTree() : base() { } public MarkdownAstTree(string? s) : base(s) { } }
public class MarkdownTree : MarkdownAst { public MarkdownTree() : base() { } public MarkdownTree(string? s) : base(s) { } }
public class DocumentTree : MarkdownAst { public DocumentTree() : base() { } public DocumentTree(string? s) : base(s) { } }
public class Ast : MarkdownAst { public Ast() : base() { } public Ast(string? s) : base(s) { } }
public class MarkdownDocumentTree : MarkdownAst { public MarkdownDocumentTree() : base() { } public MarkdownDocumentTree(string? s) : base(s) { } }
