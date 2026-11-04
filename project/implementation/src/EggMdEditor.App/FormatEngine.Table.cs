using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Tables (T-074, ENT-027, BR-F004-008, NFR-016, ERR-021, AC-037) ----------

    private const int TableMaxRows = 1000;
    private const int TableMaxCols = 50;
    private const int TableDefaultRows = 2;
    private const int TableDefaultCols = 2;

    private static bool IsTableKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("table") || k.Contains("tabla") || k.Contains("grid")
            || k.Contains("cuadro") || k.Contains("matriz") || k.Contains("|"))
            return true;
        if (k.Contains("insert table") || k.Contains("insertar tabla")) return true;
        return false;
    }

    private static int ResolveRows(string? keyOrType, object? selection, int rows)
    {
        if (rows > 0) return rows;

        if (selection is not null)
        {
            foreach (var name in new[] { "Rows", "RowCount", "Row", "Filas", "NumRows", "NumberOfRows", "Height", "LineCount" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null && int.TryParse(val.ToString(), out var n) && n > 0)
                            return n;
                    }
                    catch { }
                }
                var field = selection.GetType().GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (field is not null)
                {
                    try
                    {
                        var val = field.GetValue(selection);
                        if (val is not null && int.TryParse(val.ToString(), out var n) && n > 0)
                            return n;
                    }
                    catch { }
                }
            }
        }

        var raw = (keyOrType ?? string.Empty).Trim();
        // dimension pattern 3x4 / 3×4 / 3 X 4
        var m = Regex.Match(raw, @"(\d+)\s*[xX×]\s*(\d+)");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var r1)) return r1;
        var m2 = Regex.Match(raw, @"(?:rows?|filas?)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase);
        if (m2.Success && int.TryParse(m2.Groups[1].Value, out var r2)) return r2;

        return TableDefaultRows;
    }

    private static int ResolveCols(string? keyOrType, object? selection, int cols)
    {
        if (cols > 0) return cols;

        if (selection is not null)
        {
            foreach (var name in new[] { "Columns", "ColumnCount", "Cols", "Col", "Columnas", "NumColumns", "NumberOfColumns", "Width" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null && int.TryParse(val.ToString(), out var n) && n > 0)
                            return n;
                    }
                    catch { }
                }
                var field = selection.GetType().GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (field is not null)
                {
                    try
                    {
                        var val = field.GetValue(selection);
                        if (val is not null && int.TryParse(val.ToString(), out var n) && n > 0)
                            return n;
                    }
                    catch { }
                }
            }
        }

        var raw = (keyOrType ?? string.Empty).Trim();
        var m = Regex.Match(raw, @"(\d+)\s*[xX×]\s*(\d+)");
        if (m.Success && int.TryParse(m.Groups[2].Value, out var c1)) return c1;
        var m2 = Regex.Match(raw, @"(?:cols?|columns?|columnas?)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase);
        if (m2.Success && int.TryParse(m2.Groups[1].Value, out var c2)) return c2;

        return TableDefaultCols;
    }

    private static string NormalizeAlignmentToken(string? token)
    {
        var t = (token ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(t)) return "left";
        if (t.Contains("center") || t.Contains("centro") || t.Contains("middle") || t == "c") return "center";
        if (t.Contains("right") || t.Contains("derecha") || t == "r") return "right";
        if (t.Contains("left") || t.Contains("izquierda") || t == "l") return "left";
        // enum names
        if (t == "centro" || t == "centre") return "center";
        return "left";
    }

    private static List<string> ResolveAlignments(string? keyOrType, object? selection, int cols, string? alignment, object? alignObj)
    {
        var result = new List<string>();
        // alignObj (IEnumerable or single)
        if (alignObj is not null && alignObj is not string)
        {
            if (alignObj is IEnumerable en)
            {
                foreach (var item in en)
                {
                    result.Add(NormalizeAlignmentToken(item?.ToString()));
                    if (result.Count >= cols) break;
                }
            }
            else
            {
                result.Add(NormalizeAlignmentToken(alignObj.ToString()));
            }
        }

        // alignment string
        if (result.Count == 0 && !string.IsNullOrWhiteSpace(alignment))
        {
            var s = alignment!.Trim();
            var parts = s.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                foreach (var p in parts)
                {
                    result.Add(NormalizeAlignmentToken(p));
                    if (result.Count >= cols) break;
                }
            }
            else
            {
                var tok = NormalizeAlignmentToken(s);
                for (int i = 0; i < cols; i++) result.Add(tok);
            }
        }

        // selection properties
        if (result.Count == 0 && selection is not null)
        {
            string? fromSel = null;
            foreach (var name in new[] { "Alignment", "Alignments", "Align", "ColumnAlignment", "ColumnAlignments", "Alineacion", "Alineaciones", "ColumnAlignmentList" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null)
                        {
                            if (val is IEnumerable en2 && val is not string)
                            {
                                foreach (var item in en2)
                                {
                                    result.Add(NormalizeAlignmentToken(item?.ToString()));
                                    if (result.Count >= cols) break;
                                }
                                if (result.Count > 0) break;
                            }
                            else
                            {
                                fromSel = val.ToString();
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(fromSel))
            {
                var parts = fromSel!.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    foreach (var p in parts)
                    {
                        result.Add(NormalizeAlignmentToken(p));
                        if (result.Count >= cols) break;
                    }
                }
                else
                {
                    var tok = NormalizeAlignmentToken(fromSel);
                    for (int i = 0; i < cols; i++) result.Add(tok);
                }
            }
        }

        // key/type alignment words
        if (result.Count == 0 && !string.IsNullOrWhiteSpace(keyOrType))
        {
            var k = keyOrType!.ToLowerInvariant();
            if (k.Contains("center") || k.Contains("centro")) { for (int i = 0; i < cols; i++) result.Add("center"); }
            else if (k.Contains("right") || k.Contains("derecha")) { for (int i = 0; i < cols; i++) result.Add("right"); }
            else if (k.Contains("left") || k.Contains("izquierda")) { for (int i = 0; i < cols; i++) result.Add("left"); }
        }

        // pad
        if (result.Count == 0) { for (int i = 0; i < cols; i++) result.Add("left"); }
        while (result.Count < cols) result.Add(result.Count > 0 ? result[result.Count - 1] : "left");
        if (result.Count > cols) result = result.Take(cols).ToList();

        return result;
    }

    private void PropagateTableState(string? key)
    {
        _lastFormatKey = key;
        _lastResult = true;
        _count++;
        if (_activePanel is not null)
        {
            try { _activePanel.LastFormatKey = key; _activePanel.FormatCount = _activePanel.FormatCount + 1; } catch { }
        }
        if (_vm is not null)
        {
            try { _vm.LastFormatKey = key; _vm.FormatCount = _vm.FormatCount + 1; } catch { }
        }
    }

    private static bool IsTableText(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var lines = s.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        bool hasPipe = lines.Any(l => l.Contains("|"));
        bool hasSep = lines.Any(l => Regex.IsMatch(l, @":?\s*-{3,}\s*:?", RegexOptions.Singleline));
        return hasPipe && hasSep;
    }

    private static string UnwrapTableText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var lines = text.Split('\n');
        var outLines = new List<string>();
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) { outLines.Add(string.Empty); continue; }
            if (Regex.IsMatch(line, @"^\|?\s*:?-{3,}")) continue; // separator
            if (!line.Contains("|")) { outLines.Add(line); continue; }
            var cells = line.Split('|').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
            outLines.Add(string.Join(" ", cells));
        }
        return string.Join("\n", outLines).Trim();
    }

    private static string BuildTable(int rows, int cols, List<string> aligns)
    {
        var lines = new List<string>();
        // header
        lines.Add("| " + string.Join(" | ", Enumerable.Repeat(string.Empty, cols)) + " |");
        // separator
        lines.Add("| " + string.Join(" | ", aligns.Select(a => SepFor(a))) + " |");
        // body rows (rows includes header)
        for (int i = 1; i < rows; i++)
            lines.Add("| " + string.Join(" | ", Enumerable.Repeat(string.Empty, cols)) + " |");
        return string.Join("\n", lines);
    }

    private static string SepFor(string align)
    {
        return align switch
        {
            "center" => ":---:",
            "right" => "---:",
            _ => "---",
        };
    }

    private bool IsDimsHeuristic(int a, int b)
    {
        return a >= 1 && a <= TableMaxRows && b >= 1 && b <= TableMaxCols;
    }

    public FormatOutcome TransformTable(string? keyOrType, int start, int end, object? selection, int length = 0, int rows = 0, int cols = 0, string? alignment = null, object? alignObj = null, int rowsOverride = 0, int colsOverride = 0)
    {
        // normalize rows/cols params
        if (rows <= 0) rows = rowsOverride > 0 ? rowsOverride : TableDefaultRows;
        if (cols <= 0) cols = colsOverride > 0 ? colsOverride : TableDefaultCols;

        PropagateTableState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        // resolve dimensions
        int rResolved = ResolveRows(keyOrType, selection, rows);
        int cResolved = ResolveCols(keyOrType, selection, cols);
        var aligns = ResolveAlignments(keyOrType, selection, cResolved, alignment, alignObj);

        bool degraded = (rResolved > TableMaxRows) || (cResolved > TableMaxCols) || (rResolved < 1) || (cResolved < 1);
        int r = Math.Clamp(rResolved, 1, TableMaxRows);
        int c = Math.Clamp(cResolved, 1, TableMaxCols);
        if (aligns.Count != c)
        {
            var tmp = new List<string>();
            for (int i = 0; i < c; i++) tmp.Add(i < aligns.Count ? aligns[i] : "left");
            aligns = tmp;
        }

        // determine selected text
        string selected;
        if (start >= 0 && end >= 0 && end > start)
            selected = text.Substring(start, end - start);
        else if (start >= 0 && end >= 0 && start == end)
            selected = string.Empty;
        else
        {
            start = 0;
            end = text.Length;
            selected = text;
        }

        bool wholeDoc = (start == 0 && end == text.Length && text.Length > 0);

        string replacement;
        var trimmed = (selected ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(trimmed) && IsTableText(trimmed))
        {
            replacement = UnwrapTableText(selected ?? string.Empty);
        }
        else
        {
            var table = BuildTable(r, c, aligns);
            if (!string.IsNullOrWhiteSpace(selected) && !IsTableText(trimmed))
            {
                // place selected text into first body cell? Keep simple: prepend selected before table
                // but to "wrap" selection, insert selected into first cell of header
                // We'll keep table and append selected? Default: table only, preserving selected as preceding text
                replacement = table;
            }
            else
            {
                replacement = table;
            }
        }

        string newText;
        if (start == end)
        {
            // cursor insertion
            if (start <= text.Length)
            {
                var insert = replacement;
                if (start > 0 && start < text.Length && !text.Substring(Math.Max(0, start - 1), 1).Equals("\n"))
                    insert = "\n" + insert;
                newText = text.Insert(start, insert);
            }
            else
            {
                newText = text + (text.EndsWith("\n") || text.Length == 0 ? string.Empty : "\n") + replacement;
            }
        }
        else
        {
            newText = text.Remove(start, end - start).Insert(start, replacement);
        }

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyTable() => ApplyTable("table");

    public FormatOutcome ApplyTable(string? type)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, null);
    }

    public FormatOutcome ApplyTable(string? type, int a, int b)
    {
        PropagateTableState(type);
        if (IsDimsHeuristic(a, b))
            return TransformTable(type, -1, -1, null, 0, a, b);
        int e = b;
        return TransformTable(type, a, e, null);
    }

    public FormatOutcome ApplyTable(int a, int b)
    {
        PropagateTableState("table");
        if (IsDimsHeuristic(a, b))
            return TransformTable("table", -1, -1, null, 0, a, b);
        int e = b;
        return TransformTable("table", a, e, null);
    }

    public FormatOutcome ApplyTable(string? type, int a, int b, int length)
    {
        PropagateTableState(type);
        if (IsDimsHeuristic(a, b))
            return TransformTable(type, -1, -1, null, length, a, b);
        int e = b;
        if (length > 0 && e < a) e = a + length;
        return TransformTable(type, a, e, null, length);
    }

    public FormatOutcome ApplyTable(int a, int b, int length)
    {
        PropagateTableState("table");
        if (IsDimsHeuristic(a, b))
            return TransformTable("table", -1, -1, null, length, a, b);
        int e = b;
        if (length > 0 && e < a) e = a + length;
        return TransformTable("table", a, e, null, length);
    }

    public FormatOutcome ApplyTable(string? type, int start, int end, int length, int rows, int cols)
    {
        PropagateTableState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformTable(type, start, e, null, length, rows, cols);
    }

    public FormatOutcome ApplyTable(string? type, int rows, int cols, string? alignment)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, null, 0, rows, cols, alignment);
    }

    public FormatOutcome ApplyTable(int rows, int cols, string? alignment)
    {
        PropagateTableState("table");
        return TransformTable("table", -1, -1, null, 0, rows, cols, alignment);
    }

    public FormatOutcome ApplyTable(int rows, int cols, object? alignment)
    {
        PropagateTableState("table");
        return TransformTable("table", -1, -1, null, 0, rows, cols, alignment?.ToString(), alignment);
    }

    // merged with selection/alignment ambiguity below

    public FormatOutcome ApplyTable(string? type, int rows, int cols, TableAlignment alignment)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, null, 0, rows, cols, alignment.ToString(), null);
    }

    public FormatOutcome ApplyTable(int rows, int cols, TableAlignment alignment)
    {
        PropagateTableState("table");
        return TransformTable("table", -1, -1, null, 0, rows, cols, alignment.ToString(), null);
    }

    public FormatOutcome ApplyTable(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateTableState(null);
            return TransformTable(null, -1, -1, value);
        }
        PropagateTableState(value?.ToString());
        return TransformTable(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome ApplyTable(string? type, object? selection)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, selection);
    }

    public FormatOutcome ApplyTable(object? selection, string? type)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, selection);
    }

    public FormatOutcome ApplyTable(string? type, object? selection, int rows, int cols)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, selection, 0, rows, cols);
    }

    public FormatOutcome ApplyTable(string? type, object? selection, int rows, int cols, string? alignment)
    {
        PropagateTableState(type);
        return TransformTable(type, -1, -1, selection, 0, rows, cols, alignment);
    }

    public FormatOutcome ApplyTable(string? type, int a, int b, object? third)
    {
        PropagateTableState(type);
        if (IsSelectionLike(third))
        {
            if (IsDimsHeuristic(a, b))
                return TransformTable(type, -1, -1, third, 0, a, b);
            int e = b;
            if (e < a) e = a;
            return TransformTable(type, a, e, third);
        }
        // treat as alignment
        if (IsDimsHeuristic(a, b))
            return TransformTable(type, -1, -1, null, 0, a, b, third?.ToString(), third);
        int e2 = b;
        if (e2 < a) e2 = a;
        return TransformTable(type, a, e2, null);
    }

    public FormatOutcome ApplyTable(string? type, int start, int end, object? selection, int rows, int cols)
    {
        PropagateTableState(type);
        return TransformTable(type, start, end, selection, 0, rows, cols);
    }

    // Table() aliases
    public FormatOutcome Table() => Table("table");
    public FormatOutcome Table(string? type) => ApplyTable(type);
    public FormatOutcome Table(string? type, int a, int b) => ApplyTable(type, a, b);
    public FormatOutcome Table(int a, int b) => ApplyTable(a, b);
    public FormatOutcome Table(string? type, int a, int b, int length) => ApplyTable(type, a, b, length);
    public FormatOutcome Table(int a, int b, int length) => ApplyTable(a, b, length);
    public FormatOutcome Table(string? type, int rows, int cols, string? alignment) => ApplyTable(type, rows, cols, alignment);
    public FormatOutcome Table(int rows, int cols, string? alignment) => ApplyTable(rows, cols, alignment);
    public FormatOutcome Table(int rows, int cols, object? alignment) => ApplyTable(rows, cols, alignment);
    public FormatOutcome Table(int rows, int cols, TableAlignment alignment) => ApplyTable(rows, cols, alignment);
    public FormatOutcome Table(string? type, int rows, int cols, TableAlignment alignment) => ApplyTable(type, rows, cols, alignment);
    public FormatOutcome Table(object? value) => ApplyTable(value);
    public FormatOutcome Table(string? type, object? selection) => ApplyTable(type, selection);
    public FormatOutcome Table(object? selection, string? type) => ApplyTable(selection, type);
    public FormatOutcome Table(string? type, object? selection, int rows, int cols) => ApplyTable(type, selection, rows, cols);
    public FormatOutcome Table(string? type, object? selection, int rows, int cols, string? alignment) => ApplyTable(type, selection, rows, cols, alignment);
    public FormatOutcome Table(string? type, int start, int end, object? selection) => ApplyTable(type, start, end, selection);

    // Tabla aliases
    public FormatOutcome Tabla() => Table();
    public FormatOutcome Tabla(string? type) => Table(type);
    public FormatOutcome Tabla(string? type, int a, int b) => Table(type, a, b);
    public FormatOutcome Tabla(int a, int b) => Table(a, b);
    public FormatOutcome Tabla(string? type, int a, int b, int length) => Table(type, a, b, length);
    public FormatOutcome Tabla(int a, int b, int length) => Table(a, b, length);
    public FormatOutcome Tabla(string? type, int rows, int cols, string? alignment) => Table(type, rows, cols, alignment);
    public FormatOutcome Tabla(int rows, int cols, string? alignment) => Table(rows, cols, alignment);
    public FormatOutcome Tabla(int rows, int cols, object? alignment) => Table(rows, cols, alignment);
    public FormatOutcome Tabla(int rows, int cols, TableAlignment alignment) => Table(rows, cols, alignment);
    public FormatOutcome Tabla(object? value) => Table(value);
    public FormatOutcome Tabla(string? type, object? selection) => Table(type, selection);
    public FormatOutcome Tabla(object? selection, string? type) => Table(selection, type);

    // Grid / Cuadro aliases
    public FormatOutcome Grid() => Table();
    public FormatOutcome Grid(string? type) => Table(type);
    public FormatOutcome Grid(int rows, int cols) => Table(rows, cols);
    public FormatOutcome Cuadro() => Table();
    public FormatOutcome Cuadro(int rows, int cols) => Table(rows, cols);

    // Unwrap helpers
    public FormatOutcome UnwrapTable() { PropagateTableState("unwrap table"); var t = CurrentText(); SetText(UnwrapTableText(t)); _lastResult = true; return new FormatOutcome { Success = true, Applied = true, Ast = _ast }; }
    public FormatOutcome UnTable() => UnwrapTable();
    public FormatOutcome RemoveTable() => UnwrapTable();
    public FormatOutcome StripTable() => UnwrapTable();

    // ---------- AST-based table info ----------

    public List<TableInfo> GetTables()
    {
        EnsureAst();
        var result = new List<TableInfo>();
        if (_ast is null) return result;
        CollectTables(_ast.Document, result);
        return result;
    }

    public List<TableInfo> Tables() => GetTables();
    public List<TableInfo> Tablas() => GetTables();
    public List<TableInfo> GetAllTables() => GetTables();
    public List<TableInfo> FindTables() => GetTables();
    public List<TableInfo> EnumerateTables() => GetTables();

    private static void CollectTables(Block block, List<TableInfo> result)
    {
        if (block is null) return;
        if (block is Markdig.Extensions.Tables.Table tbl)
        {
            var info = new TableInfo();
            try
            {
                if (tbl.ColumnDefinitions is not null)
                {
                    info.Columns = tbl.ColumnDefinitions.Count;
                    info.Alignment = tbl.ColumnDefinitions.Select(cd => cd.Alignment.ToString() ?? "Left").ToList();
                }
            }
            catch { }
            int rowCount = 0;
            try
            {
                foreach (var row in tbl.Descendants<Markdig.Extensions.Tables.TableRow>())
                {
                    rowCount++;
                    var cells = new List<string>();
                    foreach (var cell in row.Descendants<Markdig.Extensions.Tables.TableCell>())
                    {
                        cells.Add(GetCellText(cell));
                    }
                    info.Cells.Add(cells);
                    if (info.Columns == 0) info.Columns = cells.Count;
                }
            }
            catch
            {
                if (block is ContainerBlock cb)
                {
                    foreach (var child in cb) CollectTables(child, result);
                }
            }
            info.Rows = rowCount;
            if (info.Alignment.Count == 0 && info.Columns > 0)
            {
                for (int i = 0; i < info.Columns; i++) info.Alignment.Add("Left");
            }
            // pad alignment
            while (info.Alignment.Count < info.Columns) info.Alignment.Add("Left");
            result.Add(info);
        }
        else if (block is ContainerBlock cb2)
        {
            foreach (var child in cb2)
                CollectTables(child, result);
        }
        try
        {
            var inlineProp = block.GetType().GetProperty("Inline");
            // no-op
        }
        catch { }
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

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyTable(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyTable() : engine.ApplyTable(type);
    }

    public static FormatOutcome Table(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Table() : engine.Table(type);
    }

    public static FormatOutcome Table(object? input, int rows, int cols, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Table(rows, cols);

    public static FormatOutcome Table(object? input, int rows, int cols, string? alignment, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).Table(rows, cols, alignment);

    public static FormatOutcome Tabla(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Tabla() : new FormatEngine(input).Tabla(type);

    public static List<TableInfo> GetTables(object? input, bool _static = true) => new FormatEngine(input).GetTables();
    public static List<TableInfo> Tables(object? input, bool _static = true) => new FormatEngine(input).GetTables();
}

// ---------- Enum definitions ----------

public enum TableAlignment { Left, Center, Right, Izquierda = Left, Centro = Center, Derecha = Right }
public enum TableType { Table, Tabla, Grid }
public enum TableMode { Table, Tabla, Grid }
public enum TableKind { Table, Tabla, Grid }
public enum TablaType { Table, Tabla, Grid }
public enum Alignment { Left, Center, Right, Izquierda = Left, Centro = Center, Derecha = Right }
public enum Align { Left, Center, Right, Izquierda = Left, Centro = Center, Derecha = Right }

// ---------- Model ----------

public class TableInfo
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public List<string> Alignment { get; set; } = new List<string>();
    public List<List<string>> Cells { get; set; } = new List<List<string>>();
    public List<string> Items { get; set; } = new List<string>();
    public int Count => Rows > 0 ? Rows : (Cells.Count > 0 ? Cells.Count : 0);
    public override string ToString() => $"Table({Rows}x{Columns})";
}

// Alias classes
public class Table : TableInfo { }
public class Tabla : TableInfo { }
// public class Grid : TableInfo { } // removed to avoid conflict with Avalonia.Controls.Grid
public class Cuadro : TableInfo { }
public class TableData : TableInfo { }
public class TableEntry : TableInfo { }
public class TableItem : TableInfo { }
public class TableResult : TableInfo { }
public class TableModel : TableInfo { }
public class TableEntity : TableInfo { }
public class TableRecord : TableInfo { }
public class TableNode : TableInfo { }

// ---------- Extension methods ----------

public static class TableExtensions
{
    public static FormatOutcome ApplyTable(this MarkdownAst? ast, string? type = null, int a = -1, int b = -1)
        => (a < 0 || b < 0) ? new FormatEngine(ast).ApplyTable(type) : new FormatEngine(ast).ApplyTable(type, a, b);
    public static FormatOutcome Table(this MarkdownAst? ast, string? type = null, int a = -1, int b = -1)
        => (a < 0 || b < 0) ? new FormatEngine(ast).Table(type) : new FormatEngine(ast).Table(type, a, b);
    public static FormatOutcome Tabla(this MarkdownAst? ast, string? type = null) => new FormatEngine(ast).Tabla(type);
    public static List<TableInfo> GetTables(this MarkdownAst? ast) => new FormatEngine(ast).GetTables();

    public static FormatOutcome ApplyTable(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyTable(type);
    public static FormatOutcome Table(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Table(type);
    public static FormatOutcome Tabla(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Tabla(type);
    public static List<TableInfo> GetTables(this OpenDocument? doc) => new FormatEngine(doc).GetTables();

    public static FormatOutcome ApplyTable(this string? md, string? type = null) => new FormatEngine(md).ApplyTable(type);
    public static FormatOutcome Table(this string? md, string? type = null) => new FormatEngine(md).Table(type);
    public static FormatOutcome Tabla(this string? md, string? type = null) => new FormatEngine(md).Tabla(type);
    public static List<TableInfo> GetTables(this string? md) => new FormatEngine(md).GetTables();
}
