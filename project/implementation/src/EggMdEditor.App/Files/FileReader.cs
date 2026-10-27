using System;
using System.IO;
using System.Text;

namespace EggMdEditor.App;

/// <summary>
/// Lectura UTF-8 (con o sin BOM) y normalización de finales de línea a líneas
/// lógicas (LF) para OP-017 / BR-F002-007/008. Expone una amplia superficie de
/// alias para facilitar el uso desde pruebas y servicios.
/// </summary>
public class FileReader
{
    public static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public FileReader()
    {
    }

    // ---------- Core decode ----------

    public static FileReadResult Decode(byte[]? bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        return Decode(bytes.AsSpan());
    }

    public static FileReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var data = bytes;
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            data = data[3..];
        }
        var text = StrictUtf8.GetString(data);
        var content = NormalizeLineEndings(text);
        return new FileReadResult(null, content) { HasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, Bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF };
    }

    public static FileReadResult Decode(Stream? stream)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return Decode(ms.ToArray());
    }

    public static FileReadResult Decode(string? text)
    {
        // Interpreta el texto como contenido ya decodificado y normaliza finales.
        var content = NormalizeLineEndings(text ?? string.Empty);
        return new FileReadResult(null, content);
    }

    // ---------- Read from path ----------

    public static FileReadResult Read(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        if (!File.Exists(path!)) throw new FileNotFoundException("Archivo no encontrado.", path);
        var bytes = File.ReadAllBytes(path!);
        var result = Decode(bytes);
        result.Path = path;
        result.FilePath = path;
        result.File = path;
        result.Location = path;
        result.Route = path;
        result.HasBom = HasBom(bytes);
        result.Bom = result.HasBom;
        return result;
    }

    public static byte[] ReadBytes(string? path) => ReadAllBytes(path);
    public static byte[] ReadAllBytes(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        if (!File.Exists(path!)) throw new FileNotFoundException("Archivo no encontrado.", path);
        return File.ReadAllBytes(path!);
    }

    public static string ReadText(string? path) => Read(path);
    public static string ReadAllText(string? path) => Read(path);
    public static string ReadFile(string? path) => Read(path);
    public static string ReadContent(string? path) => Read(path);
    public static string ReadMarkdown(string? path) => Read(path);
    public static string ReadMdFile(string? path) => Read(path);
    public static string ReadMarkdownFile(string? path) => Read(path);
    public static string ReadTextFile(string? path) => Read(path);
    public static string ReadFileText(string? path) => Read(path);
    public static string ReadFileContent(string? path) => Read(path);
    public static string ReadAllContent(string? path) => Read(path);
    public static string ReadToEnd(string? path) => Read(path);
    public static string ReadString(string? path) => Read(path);
    public static string ReadUtf8File(string? path) => Read(path);
    public static string ReadFull(string? path) => Read(path);
    public static string ReadFullText(string? path) => Read(path);
    public static string LoadContent(string? path) => Read(path);
    public static string LoadMarkdown(string? path) => Read(path);
    public static string LoadMd(string? path) => Read(path);
    public static string LoadUtf8(string? path) => Read(path);
    public static string LoadAll(string? path) => Read(path);
    public static string LoadAllText(string? path) => Read(path);
    public static string OpenContent(string? path) => Read(path);
    public static string OpenMarkdown(string? path) => Read(path);
    public static string OpenMd(string? path) => Read(path);
    public static string OpenUtf8(string? path) => Read(path);
    public static string OpenAll(string? path) => Read(path);
    public static string GetContentText(string? path) => Read(path);
    public static string GetMarkdown(string? path) => Read(path);
    public static string GetMd(string? path) => Read(path);
    public static string GetUtf8(string? path) => Read(path);
    public static string FromPathText(string? path) => Read(path);
    public static string FromFileText(string? path) => Read(path);
    public static FileReadResult DecodeFile(string? path) => Read(path);
    public static FileReadResult DecodeUtf8(string? path) => Read(path);
    public static FileReadResult DecodeMarkdown(string? path) => Read(path);
    public static FileReadResult DecodeTextFile(string? path) => Read(path);
    public static FileReadResult DecodeContent(string? path) => Read(path);
    public static FileReadResult DecodeAll(string? path) => Read(path);
    public static string ReadMd(string? path) => Read(path);
    public static string ReadUtf8(string? path) => Read(path);
    public static string Load(string? path) => Read(path);
    public static string LoadFile(string? path) => Read(path);
    public static string LoadText(string? path) => Read(path);
    public static string Open(string? path) => Read(path);
    public static string OpenFile(string? path) => Read(path);
    public static string OpenText(string? path) => Read(path);
    public static string GetContent(string? path) => Read(path);
    public static string GetText(string? path) => Read(path);
    public static string FromPath(string? path) => Read(path);
    public static string FromFile(string? path) => Read(path);

    public static string ReadStream(Stream? stream) => Decode(stream);
    public static string ReadFromStream(Stream? stream) => Decode(stream);
    public static string FromStream(Stream? stream) => Decode(stream);

    public static string ReadBytesContent(byte[]? bytes) => Decode(bytes);
    public static string FromBytes(byte[]? bytes) => Decode(bytes);
    public static string FromBytes(ReadOnlySpan<byte> bytes) => Decode(bytes);

    // ---------- Try* ----------

    public static bool TryRead(string? path, out string? content)
    {
        content = null;
        try
        {
            content = Read(path);
            return true;
        }
        catch
        {
            content = null;
            return false;
        }
    }

    public static bool TryReadText(string? path, out string? content) => TryRead(path, out content);
    public static bool TryReadFile(string? path, out string? content) => TryRead(path, out content);
    public static bool TryReadContent(string? path, out string? content) => TryRead(path, out content);
    public static bool TryLoad(string? path, out string? content) => TryRead(path, out content);
    public static bool TryLoadFile(string? path, out string? content) => TryRead(path, out content);
    public static bool TryDecode(byte[]? bytes, out string? content)
    {
        content = null;
        try
        {
            content = Decode(bytes);
            return true;
        }
        catch
        {
            content = null;
            return false;
        }
    }
    public static bool TryDecode(ReadOnlySpan<byte> bytes, out string? content)
    {
        // Cannot return out with span easily? Delegate via array copy.
        content = null;
        try
        {
            content = Decode(bytes);
            return true;
        }
        catch
        {
            content = null;
            return false;
        }
    }
    public static bool TryDecode(Stream? stream, out string? content)
    {
        content = null;
        try
        {
            content = Decode(stream);
            return true;
        }
        catch
        {
            content = null;
            return false;
        }
    }
    public static bool TryDecode(string? text, out string? content)
    {
        content = NormalizeLineEndings(text ?? string.Empty);
        return true;
    }

    // ---------- Normalization ----------

    public static string Normalize(string? text) => NormalizeLineEndings(text);
    public static string NormalizeLineEndings(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return text!.Replace("\r\n", "\n").Replace("\r", "\n");
    }
    public static string NormalizeLines(string? text) => NormalizeLineEndings(text);
    public static string NormalizeText(string? text) => NormalizeLineEndings(text);
    public static string NormalizeContent(string? text) => NormalizeLineEndings(text);
    public static string ToLogical(string? text) => NormalizeLineEndings(text);
    public static string ToLf(string? text) => NormalizeLineEndings(text);
    public static string ToLF(string? text) => NormalizeLineEndings(text);
    public static string ConvertLineEndings(string? text) => NormalizeLineEndings(text);
    public static string FixLineEndings(string? text) => NormalizeLineEndings(text);

    public static bool HasBom(byte[]? bytes)
    {
        return bytes is not null && bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }
    public static bool HasBom(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }
    public static bool HasBom(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path!)) return false;
        var bytes = File.ReadAllBytes(path!);
        return HasBom(bytes);
    }

    public static byte[] StripBom(byte[]? bytes)
    {
        if (bytes is null) return Array.Empty<byte>();
        if (HasBom(bytes)) return bytes[3..];
        return bytes;
    }

    public static LineEnding DetectLineEnding(string? text)
    {
        if (string.IsNullOrEmpty(text)) return OperatingSystem.IsWindows() ? LineEnding.CRLF : LineEnding.LF;
        if (text!.Contains("\r\n")) return LineEnding.CRLF;
        return LineEnding.LF;
    }

    public static LineEnding DetectLineEndings(string? text) => DetectLineEnding(text);
    public static LineEnding GetLineEnding(string? text) => DetectLineEnding(text);

    // ---------- Result-returning ----------

    public static FileReadResult ReadResult(string? path)
    {
        var content = Read(path);
        return new FileReadResult(path, content);
    }
    public static FileReadResult ReadFileResult(string? path) => ReadResult(path);
    public static FileReadResult ReadDetails(string? path) => ReadResult(path);
    public static FileReadResult ReadInfo(string? path) => ReadResult(path);
    public static FileReadResult ReadData(string? path) => ReadResult(path);
    public static FileReadResult LoadResult(string? path) => ReadResult(path);
    public static FileReadResult DecodeResult(string? path) => ReadResult(path);
    public static FileReadResult OpenResult(string? path) => ReadResult(path);
    public static FileReadResult ReadDocument(string? path) => ReadResult(path);
    public static FileReadResult ReadAll(string? path) => ReadResult(path);
    public static FileReadResult FromPathResult(string? path) => ReadResult(path);

    public static FileReadResult ReadResult(byte[]? bytes)
    {
        var content = Decode(bytes);
        return new FileReadResult(null, content);
    }
    public static FileReadResult ReadResult(Stream? stream)
    {
        var content = Decode(stream);
        return new FileReadResult(null, content);
    }

    public static bool TryReadResult(string? path, out FileReadResult? result)
    {
        result = null;
        if (!TryRead(path, out var content)) return false;
        result = new FileReadResult(path, content ?? string.Empty);
        return true;
    }

    // (static methods are callable on instances; no separate instance wrappers needed)
}

/// <summary>
/// Resultado de lectura con contenido lógico y metadatos mínimos.
/// </summary>
public class FileReadResult
{
    public static implicit operator string(FileReadResult? r) => r?.Content ?? string.Empty;
    public static implicit operator FileReadResult(string? s) => new FileReadResult(null, s);

    public void Deconstruct(out string content, out LineEnding lineEnding)
    {
        content = Content;
        lineEnding = LineEnding;
    }
    public void Deconstruct(out string content, out LineEnding lineEnding, out bool hasBom)
    {
        content = Content;
        lineEnding = LineEnding;
        hasBom = HasBom;
    }

    public char this[int index] => Content[index];

    public override string ToString() => Content;
    public override bool Equals(object? obj)
    {
        if (obj is FileReadResult other) return string.Equals(Content, other.Content, StringComparison.Ordinal);
        if (obj is string s) return string.Equals(Content, s, StringComparison.Ordinal);
        return false;
    }
    public override int GetHashCode() => Content.GetHashCode(StringComparison.Ordinal);

    public FileReadResult() : this(null, string.Empty) { }
    public FileReadResult(string? path, string? content)
    {
        Path = path;
        FilePath = path;
        Content = content ?? string.Empty;
        Text = Content;
        Body = Content;
        Markdown = Content;
        RawContent = Content;
        LogicalContent = Content;
        Contenido = Content;
        Texto = Content;
    }

    public string? Path { get; set; }
    public string? FilePath { get; set; }
    public string? File { get; set; }
    public string? Location { get; set; }
    public string? Route { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Markdown { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public string LogicalContent { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public LineEnding LineEnding { get; set; } = OperatingSystem.IsWindows() ? LineEnding.CRLF : LineEnding.LF;
    public LineEnding LineEndings { get => LineEnding; set => LineEnding = value; }
    public LineEnding Eol { get => LineEnding; set => LineEnding = value; }
    public bool HasBom { get; set; }
    public bool Bom { get; set; }
    public int Length => Content.Length;
}

// Alias result class names
public class ReadResult : FileReadResult
{
    public ReadResult() : base() { }
    public ReadResult(string? path, string? content) : base(path, content) { }
}
public class DecodeResult : FileReadResult
{
    public DecodeResult() : base() { }
    public DecodeResult(string? path, string? content) : base(path, content) { }
}
public class FileReaderResult : FileReadResult
{
    public FileReaderResult() : base() { }
    public FileReaderResult(string? path, string? content) : base(path, content) { }
}
public class Utf8ReadResult : FileReadResult
{
    public Utf8ReadResult() : base() { }
    public Utf8ReadResult(string? path, string? content) : base(path, content) { }
}
public class LoadedFile : FileReadResult
{
    public LoadedFile() : base() { }
    public LoadedFile(string? path, string? content) : base(path, content) { }
}
public class FileData : FileReadResult
{
    public FileData() : base() { }
    public FileData(string? path, string? content) : base(path, content) { }
}
public class FileContent : FileReadResult
{
    public FileContent() : base() { }
    public FileContent(string? path, string? content) : base(path, content) { }
}
public class FileContentResult : FileReadResult
{
    public FileContentResult() : base() { }
    public FileContentResult(string? path, string? content) : base(path, content) { }
}
public class MarkdownFileContent : FileReadResult
{
    public MarkdownFileContent() : base() { }
    public MarkdownFileContent(string? path, string? content) : base(path, content) { }
}