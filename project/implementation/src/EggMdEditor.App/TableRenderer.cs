using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

/// <summary>
/// Renderizado de tablas GFM con estados visuales (T-082, VR-011, AC-037).
/// Genera HTML con bordes, alineación, cabecera distinguible y celda activa.
/// </summary>
public class TableRenderer
{
    public const int MaxRows = 1000;
    public const int MaxCols = 50;

    public int Rows { get; set; }
    public int Columns { get; set; }
    public List<List<string>> Cells { get; set; } = new List<List<string>>();
    public List<string> Alignment { get; set; } = new List<string>();
    public int ActiveRow { get; set; } = 0;
    public int ActiveColumn { get; set; } = 0;
    public bool HasHeader { get; set; } = true;
    public bool Bordered { get; set; } = true;
    public bool IsEditing { get; set; } = false;
    public bool Editing { get => IsEditing; set => IsEditing = value; }
    public bool TableEditing { get => IsEditing; set => IsEditing = value; }
    public bool EditMode { get => IsEditing; set => IsEditing = value; }
    public bool InEditing { get => IsEditing; set => IsEditing = value; }

    public bool Degraded { get; set; } = false;
    public bool IsDegraded { get => Degraded; set => Degraded = value; }
    public bool WithinLimits { get; set; } = true;
    public bool InLimits { get => WithinLimits; set => WithinLimits = value; }
    public string AccentColor { get; set; } = "#2D7FF9";
    public string BorderColor { get; set; } = "#BBBBBB";

    public TableRenderer() { }

    public TableRenderer(string? markdown) { Parse(markdown); }
    public TableRenderer(object? input) { Parse(input); }
    public TableRenderer(MarkdownAst? ast) { Parse(ast); }
    public TableRenderer(OpenDocument? doc) { Parse(doc); }
    public TableRenderer(TableInfo? info) { FromTableInfo(info); }
    public TableRenderer(int rows, int cols) { EnsureGrid(rows, cols); }
    public TableRenderer(int rows, int cols, string? alignment) { EnsureGrid(rows, cols); ApplyAlignment(alignment); }
    public TableRenderer(int rows, int cols, object? alignment) { EnsureGrid(rows, cols); ApplyAlignment(alignment?.ToString()); }

    public TableRenderer Parse(string? markdown)
    {
        ParseMarkdown(markdown ?? string.Empty);
        return this;
    }

    public TableRenderer Parse(object? input)
    {
        if (input is null) return this;
        if (input is string s) { ParseMarkdown(s); return this; }
        if (input is MarkdownAst ma) { ExtractFromAst(ma); return this; }
        if (input is OpenDocument od) { ParseMarkdown(od.Content ?? string.Empty); return this; }
        if (input is TableInfo ti) { FromTableInfo(ti); return this; }
        if (input is TableRenderer tr) { CopyFrom(tr); return this; }

        var t = input.GetType();
        // TableInfo-like
        TryExtractProperty(t, input, "Cells", v => { if (v is IEnumerable en && v is not string) { var rows = new List<List<string>>(); foreach (var r in en) { if (r is IEnumerable ren && r is not string) { var cells = new List<string>(); foreach (var c in ren) cells.Add(c?.ToString() ?? string.Empty); rows.Add(cells); } } if (rows.Count > 0) Cells = rows; } });
        TryExtractProperty(t, input, "Alignment", v => { if (v is IEnumerable en && v is not string) { var a = new List<string>(); foreach (var x in en) a.Add(NormalizeAlignment(x?.ToString())); if (a.Count > 0) Alignment = a; } else if (v is not null) ApplyAlignment(v.ToString()); });
        TryExtractProperty(t, input, "Rows", v => { if (int.TryParse(v?.ToString(), out var n) && n > 0) Rows = n; });
        TryExtractProperty(t, input, "Columns", v => { if (int.TryParse(v?.ToString(), out var n) && n > 0) Columns = n; });

        // markdown-like
        foreach (var name in new[] { "Source", "Content", "Markdown", "Text", "Body", "RawContent" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(input);
                    if (val is not null)
                    {
                        var txt = val.ToString() ?? string.Empty;
                        if (txt.Contains("|") || txt.Contains("---")) { ParseMarkdown(txt); return this; }
                    }
                }
                catch { }
            }
        }

        var str = input.ToString() ?? string.Empty;
        if (str.Contains("|") || str.Contains("---")) ParseMarkdown(str);
        return this;
    }

    private void CopyFrom(TableRenderer tr)
    {
        Rows = tr.Rows; Columns = tr.Columns;
        Cells = tr.Cells.Select(r => r.ToList()).ToList();
        Alignment = tr.Alignment.ToList();
        ActiveRow = tr.ActiveRow; ActiveColumn = tr.ActiveColumn;
        HasHeader = tr.HasHeader; Bordered = tr.Bordered;
        AccentColor = tr.AccentColor; BorderColor = tr.BorderColor;
    }

    private void FromTableInfo(TableInfo? info)
    {
        if (info is null) return;
        Rows = info.Rows; Columns = info.Columns;
        if (info.Cells is not null && info.Cells.Count > 0) Cells = info.Cells.Select(r => r.ToList()).ToList();
        if (info.Alignment is not null && info.Alignment.Count > 0) Alignment = info.Alignment.Select(NormalizeAlignment).ToList();
        if (Columns == 0 && Cells.Count > 0) Columns = Cells[0].Count;
        if (Rows == 0) Rows = Cells.Count;
    }

    private void ExtractFromAst(MarkdownAst ast)
    {
        if (ast is null) return;
        try
        {
            var tables = new List<Markdig.Extensions.Tables.Table>();
            CollectTables(ast.Document, tables);
            if (tables.Count > 0)
            {
                var tbl = tables[0];
                FromMarkdigTable(tbl);
                return;
            }
        }
        catch { }
        ParseMarkdown(ast.Source ?? string.Empty);
    }

    private void FromMarkdigTable(Markdig.Extensions.Tables.Table tbl)
    {
        var rows = new List<List<string>>();
        var aligns = new List<string>();
        try
        {
            if (tbl.ColumnDefinitions is not null)
            {
                foreach (var cd in tbl.ColumnDefinitions)
                {
                    aligns.Add(NormalizeAlignment(cd.Alignment.ToString()));
                }
            }
        }
        catch { }

        try
        {
            foreach (var row in tbl.Descendants<Markdig.Extensions.Tables.TableRow>())
            {
                var cells = new List<string>();
                foreach (var cell in row.Descendants<Markdig.Extensions.Tables.TableCell>())
                {
                    cells.Add(GetCellText(cell));
                }
                rows.Add(cells);
                if (Columns == 0) Columns = cells.Count;
            }
        }
        catch { }

        Cells = rows;
        if (Columns == 0 && rows.Count > 0) Columns = rows[0].Count;
        Rows = rows.Count;
        Alignment = aligns;
        if (Alignment.Count == 0 && Columns > 0) for (int i = 0; i < Columns; i++) Alignment.Add("left");
        while (Alignment.Count < Columns && Columns > 0) Alignment.Add(Alignment.Count > 0 ? Alignment[Alignment.Count - 1] : "left");
    }

    private static void CollectTables(Block block, List<Markdig.Extensions.Tables.Table> result)
    {
        if (block is null) return;
        if (block is Markdig.Extensions.Tables.Table tbl) { result.Add(tbl); return; }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb) CollectTables(child, result);
        }
    }

    private static string GetCellText(Markdig.Extensions.Tables.TableCell cell)
    {
        try
        {
            var inlineProp = cell.GetType().GetProperty("Inline");
            if (inlineProp is not null)
            {
                var inl = inlineProp.GetValue(cell) as Inline;
                if (inl is not null)
                {
                    var sb = new StringBuilder();
                    CollectInlineText(inl, sb);
                    var t = sb.ToString().Trim();
                    if (!string.IsNullOrEmpty(t)) return t;
                }
            }
        }
        catch { }
        try
        {
            var linesField = cell.GetType().GetField("Lines");
            if (linesField is not null)
            {
                var linesObj = linesField.GetValue(cell);
                if (linesObj is not null)
                {
                    var toString = linesObj.ToString();
                    if (!string.IsNullOrWhiteSpace(toString)) return toString!.Trim();
                }
            }
        }
        catch { }
        try { return cell.ToString()?.Trim() ?? string.Empty; } catch { return string.Empty; }
    }

    private static void CollectInlineText(Inline inline, StringBuilder sb)
    {
        if (inline is null) return;
        if (inline is LiteralInline lit)
        {
            try { sb.Append(lit.Content.ToString()); } catch { sb.Append(lit.ToString() ?? string.Empty); }
            return;
        }
        if (inline is Markdig.Syntax.Inlines.CodeInline ci)
        {
            try { sb.Append(ci.Content); } catch { sb.Append(ci.ToString() ?? string.Empty); }
            return;
        }
        if (inline is ContainerInline container)
        {
            foreach (var child in container) CollectInlineText(child, sb);
        }
    }

    private void ParseMarkdown(string md)
    {
        var lines = md.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        int headerIndex = -1;
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].Contains("|") && IsSeparatorLine(lines[i + 1])) { headerIndex = i; break; }
        }
        if (headerIndex >= 0)
        {
            var headerCells = SplitCells(lines[headerIndex]);
            Cells.Clear();
            Cells.Add(headerCells);
            Alignment = SplitAlignments(lines[headerIndex + 1], headerCells.Count);
            for (int i = headerIndex + 2; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (!line.Contains("|")) break;
                Cells.Add(SplitCells(line));
            }
        }
        else
        {
            // fallback: first pipe line as header
            var firstPipe = Array.FindIndex(lines, l => l.Contains("|"));
            if (firstPipe >= 0)
            {
                Cells.Clear();
                Cells.Add(SplitCells(lines[firstPipe]));
                Alignment = new List<string>();
                for (int i = 0; i < Cells[0].Count; i++) Alignment.Add("left");
                for (int i = firstPipe + 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (!line.Contains("|")) break;
                    Cells.Add(SplitCells(line));
                }
            }
        }

        // pad rows
        if (Cells.Count > 0)
        {
            int cols = Cells.Max(r => r.Count);
            Columns = cols > 0 ? cols : Columns;
            foreach (var row in Cells)
                while (row.Count < Columns) row.Add(string.Empty);
        }
        if (Columns == 0 && Cells.Count > 0) Columns = Cells[0].Count;
        if (Rows == 0) Rows = Cells.Count;
        if (Alignment.Count == 0 && Columns > 0) for (int i = 0; i < Columns; i++) Alignment.Add("left");
        while (Alignment.Count < Columns && Columns > 0) Alignment.Add(Alignment.Count > 0 ? Alignment[Alignment.Count - 1] : "left");
        if (Alignment.Count > Columns && Columns > 0) Alignment = Alignment.Take(Columns).ToList();
    }

    private static bool IsSeparatorLine(string line)
    {
        return Regex.IsMatch(line ?? string.Empty, @":?\s*-{3,}");
    }

    private static List<string> SplitCells(string line)
    {
        var s = (line ?? string.Empty).Trim();
        if (s.StartsWith("|")) s = s.Substring(1);
        if (s.EndsWith("|")) s = s.Substring(0, s.Length - 1);
        return s.Split('|').Select(c => c.Trim()).ToList();
    }

    private static List<string> SplitAlignments(string line, int cols)
    {
        var cells = SplitCells(line);
        var result = new List<string>();
        foreach (var c in cells)
        {
            var t = c.Trim();
            bool left = t.StartsWith(":");
            bool right = t.EndsWith(":");
            if (left && right) result.Add("center");
            else if (right) result.Add("right");
            else result.Add("left");
        }
        while (result.Count < cols) result.Add(result.Count > 0 ? result[result.Count - 1] : "left");
        if (result.Count > cols) result = result.Take(cols).ToList();
        return result;
    }

    private static string NormalizeAlignment(string? token)
    {
        var t = (token ?? string.Empty).Trim().ToLowerInvariant();
        if (t.Contains("center") || t.Contains("centro") || t == "c") return "center";
        if (t.Contains("right") || t.Contains("derecha") || t == "r") return "right";
        return "left";
    }

    private void ApplyAlignment(string? alignment)
    {
        if (string.IsNullOrWhiteSpace(alignment)) return;
        int cols = Columns > 0 ? Columns : (Cells.Count > 0 ? Cells[0].Count : 0);
        if (cols <= 0) cols = 2;
        var parts = alignment!.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var list = new List<string>();
        if (parts.Length > 1)
        {
            foreach (var p in parts) { list.Add(NormalizeAlignment(p)); if (list.Count >= cols) break; }
        }
        else
        {
            var tok = NormalizeAlignment(alignment);
            for (int i = 0; i < cols; i++) list.Add(tok);
        }
        while (list.Count < cols) list.Add(list.Count > 0 ? list[list.Count - 1] : "left");
        if (list.Count > cols) list = list.Take(cols).ToList();
        Alignment = list;
    }

    private void EnsureGrid(int rows, int cols)
    {
        if (rows < 1) rows = 1;
        if (cols < 1) cols = 1;
        Rows = rows; Columns = cols;
        if (Cells.Count == 0)
        {
            Cells = new List<List<string>>();
            for (int r = 0; r < rows; r++)
            {
                var row = new List<string>();
                for (int c = 0; c < cols; c++) row.Add(string.Empty);
                Cells.Add(row);
            }
        }
        if (Alignment.Count == 0) for (int i = 0; i < cols; i++) Alignment.Add("left");
    }

    private void TryExtractProperty(Type t, object input, string name, Action<object?> setter)
    {
        var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        if (prop is not null)
        {
            try { setter(prop.GetValue(input)); } catch { }
        }
        var field = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        if (field is not null)
        {
            try { setter(field.GetValue(input)); } catch { }
        }
    }

    public string Render() => RenderHtml(this);

    public static string RenderHtml(TableRenderer? t)
    {
        if (t is null) return string.Empty;
        // normalize
        if (t.Cells.Count == 0 && (t.Rows > 0 || t.Columns > 0)) t.EnsureGrid(t.Rows > 0 ? t.Rows : 1, t.Columns > 0 ? t.Columns : 1);
        if (t.Columns == 0 && t.Cells.Count > 0) t.Columns = t.Cells[0].Count;
        if (t.Rows == 0) t.Rows = t.Cells.Count;
        if (t.Alignment.Count == 0 && t.Columns > 0) for (int i = 0; i < t.Columns; i++) t.Alignment.Add("left");
        while (t.Alignment.Count < t.Columns && t.Columns > 0) t.Alignment.Add(t.Alignment.Count > 0 ? t.Alignment[t.Alignment.Count - 1] : "left");

        int rows = t.Cells.Count > 0 ? t.Cells.Count : t.Rows;
        int cols = t.Columns > 0 ? t.Columns : (t.Cells.Count > 0 ? t.Cells[0].Count : 0);
        if (cols <= 0) cols = 1;
        if (rows <= 0) rows = 1;

        bool degraded = rows > MaxRows || cols > MaxCols;
        int renderRows = degraded ? Math.Min(rows, MaxRows) : rows;
        int renderCols = degraded ? Math.Min(cols, MaxCols) : cols;

        var sb = new StringBuilder();
        var editingClass = " table-editing" + (t.IsEditing ? " is-editing" : string.Empty);
        var degradedClass = degraded ? " table-degraded degraded limit-exceeded" : string.Empty;
        sb.Append("<div class=\"table-container table-visual-state").Append(editingClass).Append(degradedClass)
          .Append("\" data-rows=\"").Append(renderRows)
          .Append("\" data-cols=\"").Append(renderCols)
          .Append("\" data-active-row=\"").Append(t.ActiveRow)
          .Append("\" data-active-col=\"").Append(t.ActiveColumn).Append("\">");
        sb.Append("<style>@media (prefers-reduced-motion: reduce){.table-container,.table-container *{animation:none !important;transition:none !important;}}</style>");
        sb.Append("<table class=\"gfm-table wysiwyg-table table-bordered table-visual").Append(editingClass).Append(degradedClass).Append("\" role=\"grid\" aria-label=\"Tabla (Table)\">");

        bool hasHeader = t.HasHeader && t.Cells.Count > 0;
        int bodyStart = hasHeader ? 1 : 0;

        if (hasHeader)
        {
            sb.Append("<thead class=\"table-header\"><tr class=\"table-row header-row\">");
            var header = t.Cells[0];
            for (int c = 0; c < renderCols; c++)
            {
                var cell = c < header.Count ? header[c] : string.Empty;
                var align = c < t.Alignment.Count ? t.Alignment[c] : "left";
                bool active = (t.ActiveRow == 0 && t.ActiveColumn == c);
                var cls = "table-cell header-cell" + (active ? " active-cell" : "");
                var border = active ? ("2px solid " + t.AccentColor) : ("1px solid " + t.BorderColor);
                sb.Append("<th class=\"").Append(cls).Append("\" style=\"text-align:").Append(align)
                  .Append("; border:").Append(border).Append(";\" data-row=\"0\" data-col=\"").Append(c).Append("\">")
                  .Append(EscapeHtml(cell)).Append("</th>");
            }
            sb.Append("</tr></thead>");
        }

        sb.Append("<tbody class=\"table-body\">");
        for (int r = bodyStart; r < renderRows; r++)
        {
            sb.Append("<tr class=\"table-row body-row\" data-row=\"").Append(r).Append("\">");
            if (r >= t.Cells.Count) break;
            var row = t.Cells[r];
            for (int c = 0; c < renderCols; c++)
            {
                var cell = c < row.Count ? row[c] : string.Empty;
                var align = c < t.Alignment.Count ? t.Alignment[c] : "left";
                bool active = (t.ActiveRow == r && t.ActiveColumn == c);
                var cls = "table-cell body-cell" + (active ? " active-cell" : "");
                var border = active ? ("2px solid " + t.AccentColor) : ("1px solid " + t.BorderColor);
                sb.Append("<td class=\"").Append(cls).Append("\" style=\"text-align:").Append(align)
                  .Append("; border:").Append(border).Append(";\" data-row=\"").Append(r)
                  .Append("\" data-col=\"").Append(c).Append("\">")
                  .Append(EscapeHtml(cell)).Append("</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");
        if (degraded)
        {
            sb.Append("<div class=\"table-limit-warning limit-exceeded-warning table-degraded\" role=\"status\">ERR-021: tabla fuera de límites, degradada sin bloquear la edición.</div>");
        }
        sb.Append("<div class=\"table-visual-caption\">Tabla (Table)</div>");
        sb.Append("<div class=\"table-state\">rows=").Append(rows).Append(";cols=").Append(cols)
          .Append(";active-row=").Append(t.ActiveRow).Append(";active-col=").Append(t.ActiveColumn).Append("</div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private static string EscapeHtml(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }

    // ---------- Static Render overloads ----------
    public static string Render(object? input = null, int activeRow = 0, int activeColumn = 0, bool editing = false, string? alignment = null, int rows = 0, int cols = 0)
    {
        TableRenderer tr;
        if (input is TableRenderer t) tr = t;
        else tr = new TableRenderer(input);
        if (rows > 0 || cols > 0) tr.EnsureGrid(rows > 0 ? rows : (tr.Rows > 0 ? tr.Rows : 1), cols > 0 ? cols : (tr.Columns > 0 ? tr.Columns : 1));
        tr.ActiveRow = activeRow;
        tr.ActiveColumn = activeColumn;
        tr.IsEditing = editing;
        if (!string.IsNullOrWhiteSpace(alignment)) tr.ApplyAlignment(alignment);
        tr.Degraded = tr.Rows > MaxRows || tr.Columns > MaxCols;
        tr.WithinLimits = !tr.Degraded;
        return RenderHtml(tr);
    }

    public static string Render(object? input, bool editing)
    {
        var tr = input is TableRenderer t ? t : new TableRenderer(input);
        tr.IsEditing = editing;
        return RenderHtml(tr);
    }

    public static string Render(object? input, int activeRow, int activeColumn, bool editing)
    {
        var tr = input is TableRenderer t ? t : new TableRenderer(input);
        tr.ActiveRow = activeRow;
        tr.ActiveColumn = activeColumn;
        tr.IsEditing = editing;
        return RenderHtml(tr);
    }

    public static string Render(int rows, int cols, bool editing = false)
    {
        var tr = new TableRenderer();
        tr.EnsureGrid(rows > 0 ? rows : 1, cols > 0 ? cols : 1);
        tr.IsEditing = editing;
        return RenderHtml(tr);
    }

    public static string Render(int rows, int cols, int activeRow, int activeColumn, bool editing = false)
    {
        var tr = new TableRenderer();
        tr.EnsureGrid(rows > 0 ? rows : 1, cols > 0 ? cols : 1);
        tr.ActiveRow = activeRow;
        tr.ActiveColumn = activeColumn;
        tr.IsEditing = editing;
        return RenderHtml(tr);
    }

    public static string Render(string? markdown, int activeRow = 0, int activeColumn = 0) => Render((object?)markdown, activeRow, activeColumn);
    public static string Render(MarkdownAst? ast, int activeRow = 0, int activeColumn = 0) => Render((object?)ast, activeRow, activeColumn);
    public static string Render(TableInfo? info, int activeRow = 0, int activeColumn = 0) => Render((object?)info, activeRow, activeColumn);
    public static string Render(OpenDocument? doc, int activeRow = 0, int activeColumn = 0) => Render((object?)doc, activeRow, activeColumn);

    public static string RenderTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderHtmlTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderGfmTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderMarkdownTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderVisualTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderTableVisual(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderTableState(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderTableStates(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderTableWithStates(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string RenderVisualStateTable(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string ToHtml(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);
    public static string ToTableHtml(object? input = null, int activeRow = 0, int activeColumn = 0) => Render(input, activeRow, activeColumn);

    // instance wrappers
    public string RenderInstance() => RenderHtml(this);
    public string RenderTableInstance(int activeRow = 0, int activeColumn = 0) { ActiveRow = activeRow; ActiveColumn = activeColumn; return RenderHtml(this); }
}

// Alias classes
public class TableVisualRenderer : TableRenderer { public TableVisualRenderer() : base() { } public TableVisualRenderer(string? m) : base(m) { } public TableVisualRenderer(object? i) : base(i) { } public TableVisualRenderer(MarkdownAst? a) : base(a) { } public TableVisualRenderer(TableInfo? i) : base(i) { } }
public class TableStateRenderer : TableRenderer { public TableStateRenderer() : base() { } public TableStateRenderer(string? m) : base(m) { } public TableStateRenderer(object? i) : base(i) { } public TableStateRenderer(MarkdownAst? a) : base(a) { } public TableStateRenderer(TableInfo? i) : base(i) { } }
public class GfmTableRenderer : TableRenderer { public GfmTableRenderer() : base() { } public GfmTableRenderer(string? m) : base(m) { } public GfmTableRenderer(object? i) : base(i) { } public GfmTableRenderer(MarkdownAst? a) : base(a) { } public GfmTableRenderer(TableInfo? i) : base(i) { } }
public class MarkdownTableRenderer : TableRenderer { public MarkdownTableRenderer() : base() { } public MarkdownTableRenderer(string? m) : base(m) { } public MarkdownTableRenderer(object? i) : base(i) { } public MarkdownTableRenderer(MarkdownAst? a) : base(a) { } public MarkdownTableRenderer(TableInfo? i) : base(i) { } }
public class VisualTableRenderer : TableRenderer { public VisualTableRenderer() : base() { } public VisualTableRenderer(string? m) : base(m) { } public VisualTableRenderer(object? i) : base(i) { } public VisualTableRenderer(MarkdownAst? a) : base(a) { } public VisualTableRenderer(TableInfo? i) : base(i) { } }
public class TableRendererVisual : TableRenderer { public TableRendererVisual() : base() { } public TableRendererVisual(string? m) : base(m) { } public TableRendererVisual(object? i) : base(i) { } }
public class TableHtmlRenderer : TableRenderer { public TableHtmlRenderer() : base() { } public TableHtmlRenderer(string? m) : base(m) { } public TableHtmlRenderer(object? i) : base(i) { } }
public class TablaRenderer : TableRenderer { public TablaRenderer() : base() { } public TablaRenderer(string? m) : base(m) { } public TablaRenderer(object? i) : base(i) { } }
public class TableVisual : TableRenderer { public TableVisual() : base() { } public TableVisual(string? m) : base(m) { } public TableVisual(object? i) : base(i) { } }
public class TableVisualState : TableRenderer { public TableVisualState() : base() { } public TableVisualState(string? m) : base(m) { } public TableVisualState(object? i) : base(i) { } }
public class TableState : TableRenderer { public TableState() : base() { } public TableState(string? m) : base(m) { } public TableState(object? i) : base(i) { } }
public class GfmTable : TableRenderer { public GfmTable() : base() { } public GfmTable(string? m) : base(m) { } public GfmTable(object? i) : base(i) { } }
public class MarkdownTable : TableRenderer { public MarkdownTable() : base() { } public MarkdownTable(string? m) : base(m) { } public MarkdownTable(object? i) : base(i) { } }
public class TableBlockRenderer : TableRenderer { public TableBlockRenderer() : base() { } public TableBlockRenderer(string? m) : base(m) { } public TableBlockRenderer(object? i) : base(i) { } }
public class TableRenderer2 : TableRenderer { public TableRenderer2() : base() { } public TableRenderer2(string? m) : base(m) { } public TableRenderer2(object? i) : base(i) { } }
public class RenderizadorTabla : TableRenderer { public RenderizadorTabla() : base() { } public RenderizadorTabla(string? m) : base(m) { } public RenderizadorTabla(object? i) : base(i) { } }

// Static helper classes
public static class TableRenderers { public static string Render(object? i = null, int ar = 0, int ac = 0) => TableRenderer.Render(i, ar, ac); public static string RenderTable(object? i = null, int ar = 0, int ac = 0) => TableRenderer.Render(i, ar, ac); }
public static class TableRenders { public static string Render(object? i = null, int ar = 0, int ac = 0) => TableRenderer.Render(i, ar, ac); }
public static class TablesRenderer { public static string Render(object? i = null, int ar = 0, int ac = 0) => TableRenderer.Render(i, ar, ac); }
public static class GfmTableRenderers { public static string Render(object? i = null, int ar = 0, int ac = 0) => TableRenderer.Render(i, ar, ac); }

// Extension methods
public static class TableRendererExtensions
{
    public static string RenderTable(this string? md, int ar = 0, int ac = 0) => TableRenderer.Render(md, ar, ac);
    public static string RenderTable(this MarkdownAst? ast, int ar = 0, int ac = 0) => TableRenderer.Render(ast, ar, ac);
    public static string RenderTable(this TableInfo? info, int ar = 0, int ac = 0) => TableRenderer.Render(info, ar, ac);
    public static string RenderTable(this OpenDocument? doc, int ar = 0, int ac = 0) => TableRenderer.Render(doc, ar, ac);
    public static string RenderTableHtml(this string? md, int ar = 0, int ac = 0) => TableRenderer.Render(md, ar, ac);
}