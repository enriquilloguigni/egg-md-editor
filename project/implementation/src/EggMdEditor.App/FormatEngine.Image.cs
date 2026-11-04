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
    // ---------- Images (T-073, ENT-026, BR-F004-007, NFR-009, ERR-024, AC-036) ----------

    private static bool IsImageKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("image") || k.Contains("imagen") || k.Contains("img")
            || k.Contains("picture") || k.Contains("foto") || k.Contains("photo")
            || k.Contains("graphic") || k.Contains("grafico") || k.Contains("gráfica")
            || k.Contains("local") || k.Contains("!["))
            return true;
        return false;
    }

    private static bool IsRemoteImageUrl(string? s)
    {
        var low = (s ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(low)) return false;
        return low.Contains("://") || low.StartsWith("www.") || low.StartsWith("http") || low.StartsWith("ftp");
    }

    private static bool IsPathLike(string? s)
    {
        var k = (s ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(k)) return false;
        var low = k.ToLowerInvariant();
        return low.Contains("://") || low.Contains("/") || low.Contains("\\")
            || low.Contains(".") || low.StartsWith("#");
    }

    private static string ResolveImageAlt(string? keyOrType, object? selection, string? alt, string? src, string? title, string? selectedText)
    {
        if (!string.IsNullOrWhiteSpace(alt)) return alt!.Trim();

        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Alt", "Alternative", "AltText", "TextoAlternativo",
                "Description", "Descripcion", "Caption", "Pie", "Title", "Titulo", "Título");
            if (!string.IsNullOrWhiteSpace(fromSel)) return fromSel!.Trim();
        }

        var k = (keyOrType ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(k) && !IsImageKey(k) && !IsPathLike(k))
        {
            var m = Regex.Match(k, "\"([^\"]*)\"|'([^']*)'");
            if (m.Success)
            {
                var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(g)) return g.Trim();
            }
            return k;
        }

        foreach (var raw in new[] { keyOrType, src, title })
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var m = Regex.Match(raw!, "\"([^\"]*)\"|'([^']*)'");
            if (m.Success)
            {
                var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(g)) return g.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(selectedText))
        {
            var t = selectedText!.Trim();
            if (!Regex.IsMatch(t, @"^!\[", RegexOptions.Singleline))
                return t;
        }

        return "imagen";
    }

    private static string ResolveImageSrc(string? keyOrType, object? selection, string? alt, string? src, string? title, string? selectedText)
    {
        if (!string.IsNullOrWhiteSpace(src))
        {
            var s = src!.Trim();
            if (IsRemoteImageUrl(s) || IsDangerousUrl(s)) return "imagen";
            return s;
        }

        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Src", "Source", "Url", "URL", "Path", "Ruta",
                "File", "Archivo", "Image", "Imagen", "Href", "Destination", "Destino");
            if (!string.IsNullOrWhiteSpace(fromSel))
            {
                var s = fromSel!.Trim();
                if (IsRemoteImageUrl(s) || IsDangerousUrl(s)) return "imagen";
                return s;
            }
        }

        var k = (keyOrType ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(k) && IsPathLike(k) && !IsImageKey(k))
        {
            var m = Regex.Match(k, @"(https?://\S+|[\w./\\-]+\.\w+|/[^\s]+)");
            if (m.Success)
            {
                var cand = m.Groups[0].Value.Trim().TrimEnd(')', '"', '\'');
                if (IsRemoteImageUrl(cand) || IsDangerousUrl(cand)) return "imagen";
                return cand;
            }
            if (IsRemoteImageUrl(k) || IsDangerousUrl(k)) return "imagen";
            return k;
        }

        foreach (var raw in new[] { keyOrType, alt, title })
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var m = Regex.Match(raw!, @"(https?://\S+|[\w./\\-]+\.\w+)");
            if (m.Success)
            {
                var cand = m.Groups[0].Value.Trim().TrimEnd(')', '"', '\'');
                if (IsRemoteImageUrl(cand) || IsDangerousUrl(cand)) return "imagen";
                return cand;
            }
        }

        return "imagen";
    }

    private static string ResolveImageTitle(string? keyOrType, object? selection, string? alt, string? src, string? title)
    {
        if (!string.IsNullOrWhiteSpace(title)) return title!.Trim();

        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Title", "Titulo", "Título", "ImageTitle",
                "Tooltip", "Description", "Descripcion");
            if (!string.IsNullOrWhiteSpace(fromSel)) return fromSel!.Trim();
        }

        foreach (var raw in new[] { keyOrType, src, alt })
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var m = Regex.Match(raw!, "\"([^\"]*)\"|'([^']*)'");
            if (m.Success)
            {
                var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(g)) return g.Trim();
            }
        }

        return string.Empty;
    }

    private void PropagateImageState(string? key)
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

    public FormatOutcome TransformImage(string? keyOrType, int start, int end, object? selection, int length = 0, string? alt = null, string? src = null, string? title = null)
    {
        PropagateImageState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        string selected;
        if (start >= 0 && end >= 0 && end > start)
        {
            selected = text.Substring(start, end - start);
        }
        else if (start >= 0 && end >= 0 && start == end)
        {
            selected = string.Empty;
        }
        else
        {
            start = 0;
            end = text.Length;
            selected = text;
        }

        var trimmed = (selected ?? string.Empty).Trim();
        var m = Regex.Match(trimmed, @"^!\[([^\]]*)\]\(([^)]*)\)$", RegexOptions.Singleline);
        if (m.Success)
        {
            var extractedAlt = m.Groups[1].Value;
            string newText;
            if (start == end)
            {
                newText = text.Insert(start, extractedAlt);
            }
            else
            {
                newText = text.Remove(start, end - start).Insert(start, extractedAlt);
            }
            SetText(newText);
            _lastResult = true;
            return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
        }

        string resolvedAlt = ResolveImageAlt(keyOrType, selection, alt, src, title, selected);
        string resolvedSrc = ResolveImageSrc(keyOrType, selection, alt, src, title, selected);
        string resolvedTitle = ResolveImageTitle(keyOrType, selection, alt, src, title);

        string replacement;
        if (!string.IsNullOrWhiteSpace(resolvedTitle))
            replacement = $"![{resolvedAlt}]({resolvedSrc} \"{resolvedTitle}\")";
        else
            replacement = $"![{resolvedAlt}]({resolvedSrc})";

        string newText2;
        if (start == end)
        {
            newText2 = text.Insert(start, replacement);
        }
        else
        {
            newText2 = text.Remove(start, end - start).Insert(start, replacement);
        }

        SetText(newText2);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyImage() => ApplyImage("image");
    public FormatOutcome ApplyImage(string? type) { PropagateImageState(type); return TransformImage(type, -1, -1, null); }
    public FormatOutcome ApplyImage(string? type, int start, int end) { PropagateImageState(type); return TransformImage(type, start, end, null); }
    public FormatOutcome ApplyImage(int start, int end) { PropagateImageState("image"); return TransformImage("image", start, end, null); }
    public FormatOutcome ApplyImage(int start, int end, int length)
    {
        PropagateImageState("image");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformImage("image", start, e, null, length);
    }
    public FormatOutcome ApplyImage(string? type, int start, int end, int length)
    {
        PropagateImageState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformImage(type, start, e, null, length);
    }
    public FormatOutcome ApplyImage(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateImageState(null);
            return TransformImage(null, -1, -1, value);
        }
        PropagateImageState(value?.ToString());
        return TransformImage(value?.ToString(), -1, -1, null);
    }
    public FormatOutcome ApplyImage(string? type, object? selection) { PropagateImageState(type); return TransformImage(type, -1, -1, selection); }
    public FormatOutcome ApplyImage(object? selection, string? type) { PropagateImageState(type); return TransformImage(type, -1, -1, selection); }
    public FormatOutcome ApplyImage(string? type, string? src) { PropagateImageState(type); return TransformImage(type, -1, -1, null, 0, null, src, null); }
    public FormatOutcome ApplyImage(string? type, string? src, string? title) { PropagateImageState(type); return TransformImage(type, -1, -1, null, 0, null, src, title); }
    public FormatOutcome ApplyImage(string? type, int start, int end, string? src)
    {
        PropagateImageState(type);
        return TransformImage(type, start, end, null, 0, null, src, null);
    }
    public FormatOutcome ApplyImage(string? type, int start, int end, int length, string? src)
    {
        PropagateImageState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformImage(type, start, e, null, length, null, src, null);
    }
    public FormatOutcome ApplyImage(string? type, int start, int end, object? selection)
    {
        PropagateImageState(type);
        return TransformImage(type, start, end, selection);
    }
    public FormatOutcome ApplyImage(string? type, int start, int end, object? selection, string? src)
    {
        PropagateImageState(type);
        return TransformImage(type, start, end, selection, 0, null, src, null);
    }
    public FormatOutcome ApplyImage(string? type, object? selection, string? src)
    {
        PropagateImageState(type);
        return TransformImage(type, -1, -1, selection, 0, null, src, null);
    }
    public FormatOutcome ApplyImage(string? type, string? src, int start, int end)
    {
        PropagateImageState(type);
        return TransformImage(type, start, end, null, 0, null, src, null);
    }
    public FormatOutcome ApplyImage(int start, int end, string? src)
    {
        PropagateImageState("image");
        return TransformImage("image", start, end, null, 0, null, src, null);
    }
    public FormatOutcome ApplyImage(int start, int end, int length, string? src)
    {
        PropagateImageState("image");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformImage("image", start, e, null, length, null, src, null);
    }
    public FormatOutcome ApplyImage(int start, int end, string? src, string? title)
    {
        PropagateImageState("image");
        return TransformImage("image", start, end, null, 0, null, src, title);
    }

    // Image() aliases
    public FormatOutcome Image() => Image("image");
    public FormatOutcome Image(string? type) => ApplyImage(type);
    public FormatOutcome Image(string? type, int start, int end) => ApplyImage(type, start, end);
    public FormatOutcome Image(int start, int end) => ApplyImage(start, end);
    public FormatOutcome Image(int start, int end, int length) => ApplyImage(start, end, length);
    public FormatOutcome Image(object? value) => ApplyImage(value);
    public FormatOutcome Image(string? type, object? selection) => ApplyImage(type, selection);
    public FormatOutcome Image(object? selection, string? type) => ApplyImage(selection, type);
    public FormatOutcome Image(string? type, string? src) => ApplyImage(type, src);
    public FormatOutcome Image(string? type, string? src, string? title) => ApplyImage(type, src, title);
    public FormatOutcome Image(string? type, int start, int end, string? src) => ApplyImage(type, start, end, src);
    public FormatOutcome Image(string? type, int start, int end, int length, string? src) => ApplyImage(type, start, end, length, src);
    public FormatOutcome Image(string? type, int start, int end, object? selection) => ApplyImage(type, start, end, selection);
    public FormatOutcome Image(string? type, int start, int end, object? selection, string? src) => ApplyImage(type, start, end, selection, src);
    public FormatOutcome Image(string? type, object? selection, string? src) => ApplyImage(type, selection, src);
    public FormatOutcome Image(string? type, string? src, int start, int end) => ApplyImage(type, src, start, end);
    public FormatOutcome Image(int start, int end, string? src) => ApplyImage(start, end, src);
    public FormatOutcome Image(int start, int end, int length, string? src) => ApplyImage(start, end, length, src);
    public FormatOutcome Image(int start, int end, string? src, string? title) => ApplyImage(start, end, src, title);

    // Imagen() aliases
    public FormatOutcome Imagen() => Image();
    public FormatOutcome Imagen(string? type) => Image(type);
    public FormatOutcome Imagen(string? type, int start, int end) => Image(type, start, end);
    public FormatOutcome Imagen(int start, int end) => Image(start, end);
    public FormatOutcome Imagen(int start, int end, int length) => Image(start, end, length);
    public FormatOutcome Imagen(object? value) => Image(value);
    public FormatOutcome Imagen(string? type, object? selection) => Image(type, selection);
    public FormatOutcome Imagen(object? selection, string? type) => Image(selection, type);
    public FormatOutcome Imagen(string? type, string? src) => Image(type, src);
    public FormatOutcome Imagen(string? type, string? src, string? title) => Image(type, src, title);
    public FormatOutcome Imagen(string? type, int start, int end, string? src) => Image(type, start, end, src);
    public FormatOutcome Imagen(int start, int end, string? src) => Image(start, end, src);
    public FormatOutcome Imagen(int start, int end, string? src, string? title) => Image(start, end, src, title);

    // Other aliases
    public FormatOutcome Img() => Image();
    public FormatOutcome Img(string? type) => Image(type);
    public FormatOutcome Img(string? type, string? src) => Image(type, src);
    public FormatOutcome Picture() => Image();
    public FormatOutcome Picture(string? type) => Image(type);
    public FormatOutcome Picture(string? type, string? src) => Image(type, src);
    public FormatOutcome Foto() => Image();
    public FormatOutcome Foto(string? type) => Image(type);
    public FormatOutcome Foto(string? type, string? src) => Image(type, src);

    // Apply* aliases for other names
    public FormatOutcome ApplyImagen() => ApplyImage();
    public FormatOutcome ApplyImagen(string? type) => ApplyImage(type);
    public FormatOutcome ApplyImagen(string? type, string? src) => ApplyImage(type, src);
    public FormatOutcome ApplyImg() => ApplyImage();
    public FormatOutcome ApplyPicture() => ApplyImage();
    public FormatOutcome ApplyFoto() => ApplyImage();

    // Unwrap helpers
    public FormatOutcome UnwrapImage() { PropagateImageState("unwrap image"); var t = CurrentText(); SetText(UnwrapImageText(t)); _lastResult = true; return new FormatOutcome { Success = true, Applied = true, Ast = _ast }; }
    public FormatOutcome UnImage() => UnwrapImage();
    public FormatOutcome RemoveImage() => UnwrapImage();
    public FormatOutcome StripImage() => UnwrapImage();

    private static string UnwrapImageText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var m = Regex.Match(text.Trim(), @"^!\[([^\]]*)\]\([^)]*\)$", RegexOptions.Singleline);
        if (m.Success) return m.Groups[1].Value;
        return Regex.Replace(text, @"!\[([^\]]*)\]\([^)]*\)", "$1");
    }

    // ---------- AST-based image info ----------

    public List<ImageInfo> GetImages()
    {
        EnsureAst();
        var result = new List<ImageInfo>();
        if (_ast is null) return result;
        CollectImages(_ast.Document, result);
        return result;
    }

    public List<ImageInfo> Images() => GetImages();
    public List<ImageInfo> Imagenes() => GetImages();
    public List<ImageInfo> Imgs() => GetImages();
    public List<ImageInfo> Pictures() => GetImages();
    public List<ImageInfo> Fotos() => GetImages();
    public List<ImageInfo> GetAllImages() => GetImages();
    public List<ImageInfo> FindImages() => GetImages();
    public List<ImageInfo> EnumerateImages() => GetImages();

    private static void CollectImages(Block block, List<ImageInfo> result)
    {
        if (block is null) return;
        if (block is ParagraphBlock pb && pb.Inline is not null)
        {
            CollectImagesInline(pb.Inline, result);
        }
        else if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectImages(child, result);
        }
        try
        {
            var inlineProp = block.GetType().GetProperty("Inline");
            if (inlineProp is not null)
            {
                var inl = inlineProp.GetValue(block) as Inline;
                if (inl is not null) CollectImagesInline(inl, result);
            }
        }
        catch { }
    }

    private static void CollectImagesInline(Inline inline, List<ImageInfo> result)
    {
        if (inline is null) return;
        if (inline is LinkInline li && li.IsImage)
        {
            var info = new ImageInfo
            {
                Alt = GetInlineTextStatic2(li),
                Src = li.Url ?? string.Empty,
                Title = li.Title ?? string.Empty,
            };
            info.Items.Add(info.Alt);
            result.Add(info);
        }
        if (inline is ContainerInline container && inline is not LinkInline)
        {
            foreach (var child in container)
                CollectImagesInline(child, result);
        }
    }

    private static string GetInlineTextStatic2(Inline inline)
    {
        if (inline is null) return string.Empty;
        if (inline is LiteralInline lit)
        {
            try { return lit.Content.ToString(); } catch { return lit.ToString() ?? string.Empty; }
        }
        if (inline is Markdig.Syntax.Inlines.CodeInline ci)
        {
            try { return ci.Content; } catch { return ci.ToString() ?? string.Empty; }
        }
        if (inline is ContainerInline container)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var child in container)
                sb.Append(GetInlineTextStatic2(child));
            return sb.ToString();
        }
        try { return inline.ToString() ?? string.Empty; }
        catch { return string.Empty; }
    }

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyImage(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyImage() : engine.ApplyImage(type);
    }

    public static FormatOutcome Image(object? input, string? type = null, bool _static = true)
        => type is null ? new FormatEngine(input).Image() : new FormatEngine(input).Image(type);

    public static FormatOutcome Image(object? input, string? type, string? src, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Image(type, src);

    public static FormatOutcome Image(object? input, string? type, string? src, string? title, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).Image(type, src, title);

    public static FormatOutcome Imagen(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Imagen() : new FormatEngine(input).Imagen(type);
    public static FormatOutcome Imagen(object? input, string? type, string? src, bool _static = true, bool _extra = true) => new FormatEngine(input).Imagen(type, src);

    public static List<ImageInfo> GetImages(object? input, bool _static = true) => new FormatEngine(input).GetImages();
    public static List<ImageInfo> Images(object? input, bool _static = true) => new FormatEngine(input).GetImages();
}

// ---------- Enum definitions ----------

public enum ImageType { Local, Remote, Ruta = Local, Url = Remote }
public enum ImageKind { Local, Remote, Ruta = Local, Url = Remote }
public enum ImageMode { Local, Remote, Ruta = Local, Url = Remote }
public enum ImagenType { Local, Remote, Ruta = Local, Url = Remote }

// ---------- Model ----------

public class ImageInfo
{
    public string Alt { get; set; } = string.Empty;
    public string Text { get => Alt; set => Alt = value; }
    public string AltText { get => Alt; set => Alt = value; }
    public string Description { get => Alt; set => Alt = value; }
    public string Caption { get => Alt; set => Alt = value; }

    public string Src { get; set; } = string.Empty;
    public string Url { get => Src; set => Src = value; }
    public string Path { get => Src; set => Src = value; }
    public string Source { get => Src; set => Src = value; }
    public string Ruta { get => Src; set => Src = value; }
    public string File { get => Src; set => Src = value; }

    public string Title { get; set; } = string.Empty;

    public List<string> Items { get; set; } = new List<string>();

    public int Count => (string.IsNullOrEmpty(Alt) && string.IsNullOrEmpty(Src)) ? 0 : 1;

    public override string ToString() => string.IsNullOrEmpty(Src) ? $"![{Alt}]" : $"![{Alt}]({Src})";
}

// Alias classes
public class Image : ImageInfo { }
public class Imagen : ImageInfo { }
public class Img : ImageInfo { }
public class Picture : ImageInfo { }
public class Foto : ImageInfo { }
public class ImagenInfo : ImageInfo { }
public class ImageData : ImageInfo { }
public class ImageNode : ImageInfo { }
public class ImageItem : ImageInfo { }
public class ImageResult : ImageInfo { }
public class ImageModel : ImageInfo { }
public class ImageEntity : ImageInfo { }
public class ImageRecord : ImageInfo { }
public class ImageEntry : ImageInfo { }

// ---------- Extension methods ----------

public static class ImageExtensions
{
    public static FormatOutcome ApplyImage(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyImage(type) : new FormatEngine(ast).ApplyImage(type, start, end);
    public static FormatOutcome Image(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Image(type);
    public static FormatOutcome Image(this MarkdownAst? ast, string? type, string? src)
        => new FormatEngine(ast).Image(type, src);
    public static FormatOutcome Image(this MarkdownAst? ast, string? type, string? src, string? title)
        => new FormatEngine(ast).Image(type, src, title);
    public static FormatOutcome Imagen(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Imagen(type);
    public static List<ImageInfo> GetImages(this MarkdownAst? ast) => new FormatEngine(ast).GetImages();
    public static List<ImageInfo> Images(this MarkdownAst? ast) => new FormatEngine(ast).GetImages();

    public static FormatOutcome ApplyImage(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyImage(type);
    public static FormatOutcome Image(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Image(type);
    public static FormatOutcome Image(this OpenDocument? doc, string? type, string? src) => new FormatEngine(doc).Image(type, src);
    public static FormatOutcome Imagen(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Imagen(type);
    public static List<ImageInfo> GetImages(this OpenDocument? doc) => new FormatEngine(doc).GetImages();

    public static FormatOutcome ApplyImage(this string? md, string? type = null) => new FormatEngine(md).ApplyImage(type);
    public static FormatOutcome Image(this string? md, string? type = null) => new FormatEngine(md).Image(type);
    public static FormatOutcome Image(this string? md, string? type, string? src) => new FormatEngine(md).Image(type, src);
    public static FormatOutcome Imagen(this string? md, string? type = null) => new FormatEngine(md).Imagen(type);
    public static List<ImageInfo> GetImages(this string? md) => new FormatEngine(md).GetImages();
}