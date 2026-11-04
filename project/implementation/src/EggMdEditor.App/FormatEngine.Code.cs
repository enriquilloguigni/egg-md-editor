using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Code (T-071, ENT-024, BR-F004-005, AC-034) ----------

    private static readonly string[] CodeKnownLanguages =
    {
        "csharp", "cs", "c#", "fsharp", "fs", "vb", "visualbasic",
        "python", "py", "javascript", "js", "typescript", "ts", "jsx", "tsx",
        "java", "kotlin", "scala", "go", "golang", "rust", "rs", "c", "cpp",
        "c++", "objective-c", "objc", "swift", "php", "ruby", "rb", "perl",
        "r", "matlab", "lua", "shell", "bash", "sh", "zsh", "powershell",
        "ps1", "cmd", "bat", "dockerfile", "makefile", "yaml", "yml", "json",
        "xml", "html", "css", "scss", "sass", "less", "sql", "graphql", "markdown",
        "md", "latex", "tex", "mermaid", "plantuml", "toml", "ini", "properties",
        "diff", "patch", "http", "nginx", "apache", "vim", "asm", "assembly",
        "haskell", "hs", "elixir", "erlang", "clojure", "groovy", "dart", "julia",
        "fortran", "cobol", "pascal", "delphi", "ada", "prolog", "scheme", "lisp",
        "ocaml", "nim", "zig", "v", "crystal", "solidity", "sol", "terraform",
        "hcl", "proto", "protobuf", "thrift", "avro", "csv", "tsv", "log",
        "plaintext", "text", "plain", "nohighlight"
    };

    private static bool IsCodeKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("code") || k.Contains("codigo") || k.Contains("código")
            || k.Contains("inline") || k.Contains("en linea") || k.Contains("en línea")
            || k.Contains("linea") || k.Contains("línea") || k.Contains("block")
            || k.Contains("bloque") || k.Contains("fence") || k.Contains("fenced")
            || k.Contains("backtick") || k.Contains("```") || k.Contains("```"))
            return true;
        // language tokens
        if (CodeKnownLanguages.Contains(k)) return true;
        foreach (var lang in CodeKnownLanguages)
        {
            if (k == lang || k.Contains(" " + lang) || k.Contains(lang + " "))
                return true;
        }
        // generic "lang: x" / "language x"
        if (k.Contains("lang") || k.Contains("lenguaje") || k.Contains("language"))
            return true;
        return false;
    }

    private static bool IsCodeInlineKey(string? s)
    {
        var k = (s ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        return k.Contains("inline") || k.Contains("en linea") || k.Contains("en línea")
            || k.Contains("linea") || k.Contains("línea") || k.Contains("enlinea")
            || k.Contains("backtick") && !k.Contains("```");
    }

    private static bool IsCodeBlockKey(string? s)
    {
        var k = (s ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        return k.Contains("block") || k.Contains("bloque") || k.Contains("fence")
            || k.Contains("fenced") || k.Contains("triple") || k.Contains("```");
    }

    private static string? ResolveCodeLanguage(string? keyOrType, object? selection, string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
            return language!.Trim();

        // selection properties
        if (selection is not null)
        {
            foreach (var name in new[] { "Language", "Lang", "Lenguaje", "CodeLanguage", "Syntax", "Highlight", "Lenguage" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null && !string.IsNullOrWhiteSpace(val.ToString()))
                            return val.ToString()!.Trim();
                    }
                    catch { }
                }
            }
            // fallback generic type/kind/mode/value/style (only if looks like language)
            foreach (var name in new[] { "Type", "Kind", "Mode", "Format", "Value", "Style" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null)
                        {
                            var s = val.ToString()!.Trim();
                            if (!string.IsNullOrWhiteSpace(s) && !IsCodeModeWord(s))
                                return s;
                        }
                    }
                    catch { }
                }
            }
        }

        var raw = (keyOrType ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(raw)) return null;
        var k = raw.ToLowerInvariant();

        // fenced marker language
        var fenceMatch = Regex.Match(raw, @"```+\s*([A-Za-z0-9_+#\-\.]+)");
        if (fenceMatch.Success)
            return fenceMatch.Groups[1].Value.Trim();

        // lang:/language=/lenguaje:
        var langMatch = Regex.Match(k, @"(?:lang|language|lenguaje)\s*[:=]\s*([A-Za-z0-9_+#\-\.]+)");
        if (langMatch.Success)
            return langMatch.Groups[1].Value.Trim();

        // known language token
        foreach (var lang in CodeKnownLanguages)
        {
            if (k == lang) return raw;
            var tokens = k.Split(new[] { ' ', ',', ';', ':', '=', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (t == lang) return t;
            }
        }

        // single token that is not a mode/code word -> treat as language
        if (!k.Contains(" ") && !IsCodeModeWord(raw) && !IsCodeKeyWord(raw))
            return raw;

        // first token if not mode word
        var parts = k.Split(new[] { ' ', ',', ';', ':', '=', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!IsCodeModeWord(p) && !IsCodeKeyWord(p) && !string.IsNullOrWhiteSpace(p))
                return p;
        }
        return null;
    }

    private static bool IsCodeModeWord(string? s)
    {
        var k = (s ?? string.Empty).Trim().ToLowerInvariant();
        return k is "inline" or "block" or "en" or "linea" or "línea" or "bloque" or "code" or "codigo" or "código"
            or "enlinea" or "en-linea" or "en-línea";
    }

    private static bool IsCodeKeyWord(string? s)
    {
        var k = (s ?? string.Empty).Trim().ToLowerInvariant();
        return k is "code" or "codigo" or "código" or "```" or "backtick" or "fence" or "fenced";
    }

    private static string ResolveCodeMode(string? keyOrType, object? selection, string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
        {
            var k0 = (keyOrType ?? string.Empty).ToLowerInvariant();
            if (IsCodeInlineKey(k0)) return "inline";
            return "block";
        }

        var lang = ResolveCodeLanguage(keyOrType, selection, null);
        var combined = ((keyOrType ?? string.Empty) + " " + (lang ?? string.Empty)).ToLowerInvariant();

        if (IsCodeInlineKey(combined) && !IsCodeBlockKey(combined) && lang is null)
            return "inline";
        if (IsCodeBlockKey(combined) || lang is not null)
            return "block";
        if (IsCodeInlineKey(combined))
            return "inline";
        return "inline";
    }

    private void PropagateCodeState(string? key)
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

    public FormatOutcome TransformCode(string? keyOrType, int start, int end, object? selection, int length = 0, string? language = null)
    {
        return TransformCode(keyOrType, start, end, selection, length, language, false);
    }

    private FormatOutcome TransformCode(string? keyOrType, int start, int end, object? selection, int length, string? language, bool _internal)
    {
        PropagateCodeState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var lang = ResolveCodeLanguage(keyOrType, selection, language);
        var mode = ResolveCodeMode(keyOrType, selection, language);
        bool inline = string.Equals(mode, "inline", StringComparison.OrdinalIgnoreCase);

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        // cursor insertion
        if (start >= 0 && end >= 0 && start == end)
        {
            string markers;
            if (inline)
                markers = "``";
            else
            {
                var fence = "```" + (string.IsNullOrWhiteSpace(lang) ? string.Empty : lang);
                markers = fence + "\n\n```";
            }
            SetText(text.Insert(start, markers));
            _lastResult = true;
            return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
        }

        // no selection -> whole document
        if (start < 0 || end < 0)
        {
            start = 0;
            end = text.Length;
        }

        bool whole = (start == 0 && end == text.Length);

        string newText;
        if (start >= 0 && end >= 0 && end >= start)
        {
            var selected = (start == end) ? string.Empty : text.Substring(start, end - start);
            string replacement;
            if (inline)
            {
                if (selected.Length >= 2 && selected.StartsWith("`", StringComparison.Ordinal) && selected.EndsWith("`", StringComparison.Ordinal) && !selected.StartsWith("```", StringComparison.Ordinal))
                {
                    // unwrap
                    replacement = selected.Substring(1, selected.Length - 2);
                }
                else
                {
                    replacement = "`" + selected + "`";
                }
            }
            else
            {
                var trimmedSel = selected.Trim();
                bool isWrapped = trimmedSel.StartsWith("```", StringComparison.Ordinal) && trimmedSel.EndsWith("```", StringComparison.Ordinal) && trimmedSel.Length >= 6;
                if (isWrapped)
                {
                    // unwrap fence
                    var lines = selected.Split('\n');
                    int first = 0;
                    if (lines.Length > 0 && lines[0].TrimStart().StartsWith("```", StringComparison.Ordinal)) first = 1;
                    int last = lines.Length - 1;
                    if (last >= first && lines[last].Trim().StartsWith("```", StringComparison.Ordinal)) last -= 1;
                    var inner = (first <= last) ? string.Join("\n", lines.Skip(first).Take(last - first + 1)) : string.Empty;
                    replacement = inner;
                }
                else
                {
                    var fenceOpen = "```" + (string.IsNullOrWhiteSpace(lang) ? string.Empty : lang);
                    string body = selected;
                    if (body.Length > 0 && !body.EndsWith("\n")) body += "\n";
                    if (body.Length == 0) body = "\n";
                    replacement = fenceOpen + "\n" + body.TrimEnd('\n') + "\n" + "```";
                    // ensure single trailing newline before close? already
                    // rebuild cleanly:
                    var innerBody = selected;
                    replacement = fenceOpen + "\n" + innerBody + (innerBody.EndsWith("\n") ? string.Empty : "\n") + "```";
                }
            }
            newText = text.Remove(start, end - start).Insert(start, replacement);
        }
        else
        {
            // fallback whole
            if (inline)
            {
                if (text.StartsWith("`") && text.EndsWith("`") && !text.StartsWith("```"))
                    newText = text.Substring(1, text.Length - 2);
                else
                    newText = "`" + text + "`";
            }
            else
            {
                var t = text.Trim();
                if (t.StartsWith("```") && t.EndsWith("```"))
                    newText = text; // leave as is
                else
                {
                    var fenceOpen = "```" + (string.IsNullOrWhiteSpace(lang) ? string.Empty : lang);
                    newText = fenceOpen + "\n" + text + (text.EndsWith("\n") ? string.Empty : "\n") + "```";
                }
            }
        }

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // Overload to accept language as 5th param after length via string
    public FormatOutcome TransformCode(string? keyOrType, int start, int end, object? selection, string? language)
    {
        return TransformCode(keyOrType, start, end, selection, 0, language);
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyCode() => ApplyCode("code");

    public FormatOutcome ApplyCode(string? type)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, null);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, null);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end, int length)
    {
        PropagateCodeState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformCode(type, start, e, null, length);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end, int length, string? language)
    {
        PropagateCodeState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformCode(type, start, e, null, length, language);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, null, 0, language);
    }

    public FormatOutcome ApplyCode(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateCodeState(null);
            return TransformCode(null, -1, -1, value);
        }
        PropagateCodeState(value?.ToString());
        return TransformCode(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome ApplyCode(string? type, object? selection)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection);
    }

    public FormatOutcome ApplyCode(object? selection, string? type)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection);
    }

    public FormatOutcome ApplyCode(string? type, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, null, 0, language);
    }

    public FormatOutcome ApplyCode(string? type, object? selection, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection, 0, language);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end, object? selection)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, selection);
    }

    public FormatOutcome ApplyCode(string? type, int start, int end, object? selection, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, selection, 0, language);
    }

    // Code() aliases
    public FormatOutcome Code() => Code("code");

    public FormatOutcome Code(string? type)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, null);
    }

    public FormatOutcome Code(string? type, int start, int end)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, null);
    }

    public FormatOutcome Code(int start, int end)
    {
        PropagateCodeState("code");
        return TransformCode("code", start, end, null);
    }

    public FormatOutcome Code(string? type, int start, int end, int length)
    {
        PropagateCodeState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformCode(type, start, e, null, length);
    }

    public FormatOutcome Code(int start, int end, int length)
    {
        PropagateCodeState("code");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformCode("code", start, e, null, length);
    }

    public FormatOutcome Code(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateCodeState(null);
            return TransformCode(null, -1, -1, value);
        }
        PropagateCodeState(value?.ToString());
        return TransformCode(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome Code(string? type, object? selection)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection);
    }

    public FormatOutcome Code(object? selection, string? type)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection);
    }

    public FormatOutcome Code(string? type, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, null, 0, language);
    }

    public FormatOutcome Code(string? type, int start, int end, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, null, 0, language);
    }

    public FormatOutcome Code(string? type, object? selection, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, -1, -1, selection, 0, language);
    }

    public FormatOutcome Code(string? type, int start, int end, object? selection)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, selection);
    }

    public FormatOutcome Code(string? type, int start, int end, object? selection, string? language)
    {
        PropagateCodeState(type);
        return TransformCode(type, start, end, selection, 0, language);
    }

    // enum overloads
    public FormatOutcome Code(CodeMode mode) => Code(mode.ToString());
    public FormatOutcome Code(CodeType mode) => Code(mode.ToString());
    public FormatOutcome Code(CodeKind mode) => Code(mode.ToString());
    public FormatOutcome Code(CodeFormat mode) => Code(mode.ToString());
    public FormatOutcome Code(CodeStyle mode) => Code(mode.ToString());
    public FormatOutcome ApplyCode(CodeMode mode) => ApplyCode(mode.ToString());
    public FormatOutcome ApplyCode(CodeType mode) => ApplyCode(mode.ToString());
    public FormatOutcome ApplyCode(CodeKind mode) => ApplyCode(mode.ToString());
    public FormatOutcome ApplyCode(CodeFormat mode) => ApplyCode(mode.ToString());
    public FormatOutcome ApplyCode(CodeStyle mode) => ApplyCode(mode.ToString());

    // Codigo aliases
    public FormatOutcome Codigo() => Code();
    public FormatOutcome Codigo(string? type) => Code(type);
    public FormatOutcome Codigo(string? type, int start, int end) => Code(type, start, end);
    public FormatOutcome Codigo(int start, int end) => Code(start, end);
    public FormatOutcome Codigo(string? type, int start, int end, int length) => Code(type, start, end, length);
    public FormatOutcome Codigo(object? value) => Code(value);
    public FormatOutcome Codigo(string? type, object? selection) => Code(type, selection);
    public FormatOutcome Codigo(object? selection, string? type) => Code(selection, type);
    public FormatOutcome Codigo(string? type, string? language) => Code(type, language);
    public FormatOutcome Codigo(string? type, int start, int end, string? language) => Code(type, start, end, language);
    public FormatOutcome Codigo(string? type, object? selection, string? language) => Code(type, selection, language);
    public FormatOutcome ApplyCodigo() => ApplyCode();
    public FormatOutcome ApplyCodigo(string? type) => ApplyCode(type);
    public FormatOutcome ApplyCodigo(string? type, int start, int end) => ApplyCode(type, start, end);
    public FormatOutcome ApplyCodigo(object? value) => ApplyCode(value);
    public FormatOutcome ApplyCodigo(string? type, object? selection) => ApplyCode(type, selection);
    public FormatOutcome ApplyCodigo(object? selection, string? type) => ApplyCode(selection, type);
    public FormatOutcome ApplyCodigo(string? type, string? language) => ApplyCode(type, language);

    // Inline aliases
    public FormatOutcome Inline() => Code("inline");
    public FormatOutcome Inline(int start, int end) => Code("inline", start, end);
    public FormatOutcome Inline(int start, int end, int length) => Code("inline", start, end, length);
    public FormatOutcome Inline(object? selection) => Code("inline", selection);
    public FormatOutcome Inline(string? language) => Code("inline", language);
    public FormatOutcome InlineCode() => Code("inline");
    public FormatOutcome InlineCode(int start, int end) => Code("inline", start, end);
    public FormatOutcome InlineCode(object? selection) => Code("inline", selection);
    public FormatOutcome CodeInline() => Code("inline");
    public FormatOutcome CodeInline(int start, int end) => Code("inline", start, end);
    public FormatOutcome CodeInline(object? selection) => Code("inline", selection);
    public FormatOutcome EnLinea() => Code("inline");
    public FormatOutcome EnLinea(int start, int end) => Code("inline", start, end);
    public FormatOutcome EnLinea(object? selection) => Code("inline", selection);
    public FormatOutcome CodigoEnLinea() => Code("inline");
    public FormatOutcome CodigoEnLinea(int start, int end) => Code("inline", start, end);
    public FormatOutcome CodigoEnLinea(object? selection) => Code("inline", selection);

    // Block aliases
    public FormatOutcome Block() => Code("block");
    public FormatOutcome Block(string? language) => Code("block", language);
    public FormatOutcome Block(int start, int end) => Code("block", start, end);
    public FormatOutcome Block(int start, int end, int length) => Code("block", start, end, length);
    public FormatOutcome Block(int start, int end, string? language) => Code("block", start, end, language);
    public FormatOutcome Block(object? selection) => Code("block", selection);
    public FormatOutcome Block(object? selection, string? language) => Code("block", selection, language);
    public FormatOutcome Bloque() => Code("block");
    public FormatOutcome Bloque(string? language) => Code("block", language);
    public FormatOutcome Bloque(int start, int end) => Code("block", start, end);
    public FormatOutcome Bloque(object? selection) => Code("block", selection);
    public FormatOutcome CodeBlock() => Code("block");
    public FormatOutcome CodeBlock(string? language) => Code("block", language);
    public FormatOutcome CodeBlock(int start, int end) => Code("block", start, end);
    public FormatOutcome CodeBlock(object? selection) => Code("block", selection);
    public FormatOutcome CodigoBloque() => Code("block");
    public FormatOutcome CodigoBloque(string? language) => Code("block", language);
    public FormatOutcome CodigoBloque(int start, int end) => Code("block", start, end);
    public FormatOutcome CodigoBloque(object? selection) => Code("block", selection);
    public FormatOutcome BloqueCodigo() => Code("block");
    public FormatOutcome BloqueCodigo(string? language) => Code("block", language);
    public FormatOutcome BloqueCodigo(int start, int end) => Code("block", start, end);
    public FormatOutcome BloqueCodigo(object? selection) => Code("block", selection);
    public FormatOutcome FencedCode() => Code("block");
    public FormatOutcome FencedCode(string? language) => Code("block", language);
    public FormatOutcome FencedCode(int start, int end) => Code("block", start, end);
    public FormatOutcome FencedCode(object? selection) => Code("block", selection);
    public FormatOutcome TripleBackticks() => Code("block");
    public FormatOutcome TripleBackticks(string? language) => Code("block", language);
    public FormatOutcome CodeFence() => Code("block");
    public FormatOutcome CodeFence(string? language) => Code("block", language);

    // Unwrap helpers
    public FormatOutcome UnwrapCode() { PropagateCodeState("unwrap code"); var t = CurrentText(); SetText(UnwrapCodeText(t)); _lastResult = true; return new FormatOutcome { Success = true, Applied = true, Ast = _ast }; }
    public FormatOutcome UnCode() => UnwrapCode();
    public FormatOutcome RemoveCode() => UnwrapCode();
    public FormatOutcome StripCode() => UnwrapCode();

    private static string UnwrapCodeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var t = text.Trim();
        if (t.StartsWith("```") && t.EndsWith("```"))
        {
            var lines = text.Split('\n');
            int first = 0;
            if (lines.Length > 0 && lines[0].TrimStart().StartsWith("```")) first = 1;
            int last = lines.Length - 1;
            if (last >= first && lines[last].Trim().StartsWith("```")) last -= 1;
            return string.Join("\n", lines.Skip(first).Take(Math.Max(0, last - first + 1)));
        }
        if (t.StartsWith("`") && t.EndsWith("`") && !t.StartsWith("```"))
            return text.Substring(1, text.Length - 2);
        return text;
    }

    // ---------- AST-based code info ----------

    public List<CodeInfo> GetCodes()
    {
        EnsureAst();
        var result = new List<CodeInfo>();
        if (_ast is null) return result;
        CollectCodes(_ast.Document, result);
        return result;
    }

    public List<CodeInfo> Codes() => GetCodes();
    public List<CodeInfo> CodeBlocks() => GetCodes();
    public List<CodeInfo> GetCodeBlocks() => GetCodes();
    public List<CodeInfo> InlineCodes() => GetInlineCodes();
    public List<CodeInfo> GetInlineCodes() => GetCodes().Where(c => c.IsInline).ToList();
    public List<CodeInfo> GetBlockCodes() => GetCodes().Where(c => c.IsBlock).ToList();
    public List<CodeInfo> Codigos() => GetCodes();
    public List<CodeInfo> GetAllCodes() => GetCodes();
    public List<CodeInfo> FindCodes() => GetCodes();
    public List<CodeInfo> EnumerateCodes() => GetCodes();
    public List<string> GetCodeContents()
    {
        return GetCodes().Select(c => c.Content).ToList();
    }
    public List<string> CodeContents() => GetCodeContents();

    private static void CollectCodes(Block block, List<CodeInfo> result)
    {
        if (block is null) return;
        if (block is FencedCodeBlock fcb)
        {
            var info = new CodeInfo { Mode = "Bloque", IsBlock = true, IsInline = false };
            try { info.Language = string.IsNullOrWhiteSpace(fcb.Info) ? null : fcb.Info.Trim(); } catch { info.Language = null; }
            var sb = new System.Text.StringBuilder();
            try
            {
                bool first = true;
                foreach (var sl in fcb.Lines)
                {
                    if (!first) sb.Append('\n');
                    first = false;
                    sb.Append(sl.ToString());
                }
            }
            catch
            {
                try { sb.Append(fcb.ToString()); } catch { }
            }
            info.Content = sb.ToString();
            info.Text = info.Content;
            info.Code = info.Content;
            info.Value = info.Content;
            info.Body = info.Content;
            info.Markdown = info.Content;
            info.Source = info.Content;
            info.Items.Add(info.Content);
            try { info.StartLine = fcb.Line; info.EndLine = fcb.Line; } catch { }
            result.Add(info);
        }

        // inline codes
        if (block is ParagraphBlock pb && pb.Inline is not null)
        {
            CollectInlineCodes(pb.Inline, result);
        }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectCodes(child, result);
        }
        // also inspect inlines of any container
        try
        {
            var inlineProp = block.GetType().GetProperty("Inline");
            if (inlineProp is not null)
            {
                var inl = inlineProp.GetValue(block) as Inline;
                if (inl is not null) CollectInlineCodes(inl, result);
            }
        }
        catch { }
    }

    private static void CollectInlineCodes(Inline inline, List<CodeInfo> result)
    {
        if (inline is null) return;
        if (inline is Markdig.Syntax.Inlines.CodeInline ci)
        {
            var info = new CodeInfo { Mode = "En línea", IsInline = true, IsBlock = false, Language = null };
            try { info.Content = ci.Content; } catch { info.Content = ci.ToString() ?? string.Empty; }
            info.Text = info.Content;
            info.Code = info.Content;
            info.Value = info.Content;
            info.Body = info.Content;
            info.Markdown = info.Content;
            info.Source = info.Content;
            info.Items.Add(info.Content);
            result.Add(info);
        }
        if (inline is ContainerInline container)
        {
            foreach (var child in container)
                CollectInlineCodes(child, result);
        }
    }

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyCode(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyCode() : engine.ApplyCode(type);
    }

    public static FormatOutcome Code(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Code() : engine.Code(type);
    }

    public static FormatOutcome Code(object? input, string? type, string? language, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Code(type, language);

    public static FormatOutcome Inline(object? input, bool _static = true) => new FormatEngine(input).Inline();
    public static FormatOutcome Block(object? input, string? language = null, bool _static = true) => language is null ? new FormatEngine(input).Block() : new FormatEngine(input).Block(language);
    public static FormatOutcome Codigo(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Codigo() : new FormatEngine(input).Codigo(type);
    public static List<CodeInfo> GetCodes(object? input, bool _static = true) => new FormatEngine(input).GetCodes();
    public static List<CodeInfo> Codes(object? input, bool _static = true) => new FormatEngine(input).GetCodes();

    // ---------- Enum definitions ----------
}

public enum CodeMode { Inline, Block, EnLinea = Inline, Bloque = Block, InLine = Inline, BlockCode = Block }
public enum CodeType { Inline, Block, EnLinea = Inline, Bloque = Block }
public enum CodeKind { Inline, Block, EnLinea = Inline, Bloque = Block }
public enum CodeFormat { Inline, Block, EnLinea = Inline, Bloque = Block }
public enum CodeStyle { Inline, Block, EnLinea = Inline, Bloque = Block }
public enum CodeModo { EnLinea, Bloque, Inline = EnLinea, Block = Bloque }

// ---------- Model ----------
public class CodeInfo
{
    public string Mode { get; set; } = "En línea";
    public bool IsInline { get; set; }
    public bool IsBlock { get; set; }
    public string? Language { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Text { get => Content; set => Content = value; }
    public string Code { get => Content; set => Content = value; }
    public string CodeText { get => Content; set => Content = value; }
    public string Value { get => Content; set => Content = value; }
    public string Body { get => Content; set => Content = value; }
    public string Markdown { get => Content; set => Content = value; }
    public string Source { get => Content; set => Content = value; }
    public List<string> Items { get; set; } = new List<string>();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int Count => string.IsNullOrEmpty(Content) ? 0 : 1;
    public override string ToString() => Content;
}

// Alias classes
public class Code : CodeInfo { }
public class CodeBlock : CodeInfo { }
public class CodeInline : CodeInfo { }
public class InlineCode : CodeInfo { }
public class CodeData : CodeInfo { }
public class CodeEntry : CodeInfo { }
public class CodeItem : CodeInfo { }
public class CodeResult : CodeInfo { }
public class CodeModel : CodeInfo { }
public class CodeEntity : CodeInfo { }
public class CodeRecord : CodeInfo { }
public class CodeNode : CodeInfo { }
public class Codigo : CodeInfo { }
public class CodigoInfo : CodeInfo { }
public class CodigoCode : CodeInfo { }
public class InlineCodeInfo : CodeInfo { }
public class BlockCodeInfo : CodeInfo { }
public class FencedCodeInfo : CodeInfo { }
public class FencedCode : CodeInfo { }
public class CodeFenceInfo : CodeInfo { }

// ---------- Extension methods ----------
public static class CodeExtensions
{
    public static FormatOutcome ApplyCode(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyCode(type) : new FormatEngine(ast).ApplyCode(type, start, end);
    public static FormatOutcome Code(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Code(type) : new FormatEngine(ast).Code(type, start, end);
    public static FormatOutcome Inline(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Inline() : new FormatEngine(ast).Inline(start, end);
    public static FormatOutcome Block(this MarkdownAst? ast, string? language = null)
        => language is null ? new FormatEngine(ast).Block() : new FormatEngine(ast).Block(language);
    public static FormatOutcome Codigo(this MarkdownAst? ast, string? type = null) => new FormatEngine(ast).Codigo(type);
    public static List<CodeInfo> GetCodes(this MarkdownAst? ast) => new FormatEngine(ast).GetCodes();

    public static FormatOutcome ApplyCode(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyCode(type);
    public static FormatOutcome Code(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Code(type);
    public static FormatOutcome Inline(this OpenDocument? doc) => new FormatEngine(doc).Inline();
    public static FormatOutcome Block(this OpenDocument? doc, string? language = null) => language is null ? new FormatEngine(doc).Block() : new FormatEngine(doc).Block(language);
    public static FormatOutcome Codigo(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Codigo(type);
    public static List<CodeInfo> GetCodes(this OpenDocument? doc) => new FormatEngine(doc).GetCodes();

    public static FormatOutcome ApplyCode(this string? md, string? type = null) => new FormatEngine(md).ApplyCode(type);
    public static FormatOutcome Code(this string? md, string? type = null) => new FormatEngine(md).Code(type);
    public static FormatOutcome Inline(this string? md) => new FormatEngine(md).Inline();
    public static FormatOutcome Block(this string? md, string? language = null) => language is null ? new FormatEngine(md).Block() : new FormatEngine(md).Block(language);
    public static FormatOutcome Codigo(this string? md, string? type = null) => new FormatEngine(md).Codigo(type);
    public static List<CodeInfo> GetCodes(this string? md) => new FormatEngine(md).GetCodes();
}