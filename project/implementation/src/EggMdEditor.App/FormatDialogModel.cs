using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EggMdEditor.App;

/// <summary>
/// Modelo de formulario reutilizado por los diálogos de formato sobre SC-002
/// (T-079). Un único objeto expone los campos de todos los diálogos de formato
/// (enlace, tabla, imagen, fórmula, nota al pie, código) con alias para que
/// las pruebas y la vista puedan acceder por cualquier nombre razonable.
/// </summary>
public class FormatDialogModel : INotifyPropertyChanged
{
    private string _formatKey = "link";
    private string _text = string.Empty;
    private string _destination = "url";
    private string _title = string.Empty;
    private int _rows = 2;
    private int _columns = 2;
    private string _alignment = "left";
    private string _src = string.Empty;
    private string _alt = string.Empty;
    private string _code = string.Empty;
    private string _id = string.Empty;
    private string _footnoteText = string.Empty;
    private string _language = string.Empty;
    private bool _inline = true;

    public FormatDialogModel() { }

    public FormatDialogModel(string? formatKey)
    {
        if (!string.IsNullOrWhiteSpace(formatKey)) _formatKey = formatKey!.Trim();
        ApplyDefaults(_formatKey);
    }

    private void ApplyDefaults(string key)
    {
        var k = key.Trim().ToLowerInvariant();
        if (k.Contains("link") || k.Contains("enlace") || k.Contains("url") || k.Contains("anchor") || k.Contains("ancla"))
        {
            if (string.IsNullOrWhiteSpace(_destination)) _destination = "url";
        }
        if (k.Contains("image") || k.Contains("imagen"))
        {
            if (string.IsNullOrWhiteSpace(_src)) _src = "image.png";
            if (string.IsNullOrWhiteSpace(_alt)) _alt = "imagen";
        }
        if (k.Contains("table") || k.Contains("tabla"))
        {
            if (_rows < 1) _rows = 2;
            if (_columns < 1) _columns = 2;
            if (string.IsNullOrWhiteSpace(_alignment)) _alignment = "left";
        }
        if (k.Contains("formula") || k.Contains("fórmula") || k.Contains("latex"))
        {
            if (string.IsNullOrWhiteSpace(_code)) _code = "x";
        }
        if (k.Contains("footnote") || k.Contains("nota"))
        {
            if (string.IsNullOrWhiteSpace(_id)) _id = "1";
            if (string.IsNullOrWhiteSpace(_footnoteText)) _footnoteText = "nota";
        }
        if (k.Contains("code") || k.Contains("codigo") || k.Contains("código"))
        {
            if (string.IsNullOrWhiteSpace(_language)) _language = "text";
        }
    }

    public string ValidateFor(string? key = null)
    {
        var k = (key ?? _formatKey ?? string.Empty).Trim().ToLowerInvariant();
        if (k.Contains("link") || k.Contains("enlace") || k.Contains("url") || k.Contains("anchor") || k.Contains("ancla"))
        {
            if (string.IsNullOrWhiteSpace(_destination)) return "Destination required";
        }
        if (k.Contains("image") || k.Contains("imagen"))
        {
            if (string.IsNullOrWhiteSpace(_src)) return "Source required";
            if (string.IsNullOrWhiteSpace(_alt)) return "Alt required";
        }
        if (k.Contains("table") || k.Contains("tabla"))
        {
            if (_rows < 1 || _rows > 1000) return "Rows out of range";
            if (_columns < 1 || _columns > 50) return "Columns out of range";
        }
        if (k.Contains("formula") || k.Contains("fórmula") || k.Contains("latex"))
        {
            if (string.IsNullOrWhiteSpace(_code)) return "Code required";
        }
        if (k.Contains("footnote") || k.Contains("nota"))
        {
            if (string.IsNullOrWhiteSpace(_id)) return "Id required";
        }
        return string.Empty;
    }

    public string? Validate() => string.IsNullOrEmpty(ValidateFor(_formatKey)) ? null : ValidateFor(_formatKey);

    // Format key
    public string FormatKey { get => _formatKey; set { _formatKey = value ?? string.Empty; ApplyDefaults(_formatKey); OnPropertyChanged(nameof(FormatKey)); } }
    public string Type { get => _formatKey; set => FormatKey = value; }
    public string Kind { get => _formatKey; set => FormatKey = value; }
    public string Mode { get => _formatKey; set => FormatKey = value; }
    public string Key { get => _formatKey; set => FormatKey = value; }
    public string DialogType { get => _formatKey; set => FormatKey = value; }

    // Text / link text
    public string Text { get => _text; set { _text = value ?? string.Empty; OnPropertyChanged(nameof(Text)); } }
    public string LinkText { get => _text; set => Text = value; }
    public string SelectedText { get => _text; set => Text = value; }
    public string Content { get => _text; set => Text = value; }
    public string Value { get => _text; set => Text = value; }
    public string Label { get => _text; set => Text = value; }

    // Destination / url
    public string Destination { get => _destination; set { _destination = value ?? string.Empty; OnPropertyChanged(nameof(Destination)); } }
    public string Url { get => _destination; set => Destination = value; }
    public string URL { get => _destination; set => Destination = value; }
    public string Href { get => _destination; set => Destination = value; }
    public string Dest { get => _destination; set => Destination = value; }
    public string Destino { get => _destination; set => Destination = value; }
    public string Target { get => _destination; set => Destination = value; }
    public string Link { get => _destination; set => Destination = value; }
    public string Address { get => _destination; set => Destination = value; }
    public string Uri { get => _destination; set => Destination = value; }
    public string Anchor { get => _destination; set => Destination = value; }
    public string Ancla { get => _destination; set => Destination = value; }
    public string Slug { get => _destination; set => Destination = value; }
    public string Hyperlink { get => _destination; set => Destination = value; }

    // Title
    public string Title { get => _title; set { _title = value ?? string.Empty; OnPropertyChanged(nameof(Title)); } }
    public string Titulo { get => _title; set => Title = value; }
    public string Título { get => _title; set => Title = value; }
    public string LinkTitle { get => _title; set => Title = value; }
    public string Tooltip { get => _title; set => Title = value; }
    public string Description { get => _title; set => Title = value; }
    public string Descripcion { get => _title; set => Title = value; }

    // Rows
    public int Rows { get => _rows; set { _rows = value; OnPropertyChanged(nameof(Rows)); } }
    public int RowCount { get => _rows; set => Rows = value; }
    public int Filas { get => _rows; set => Rows = value; }
    public int Row { get => _rows; set => Rows = value; }

    // Columns
    public int Columns { get => _columns; set { _columns = value; OnPropertyChanged(nameof(Columns)); } }
    public int ColumnCount { get => _columns; set => Columns = value; }
    public int Columnas { get => _columns; set => Columns = value; }
    public int Cols { get => _columns; set => Columns = value; }
    public int ColCount { get => _columns; set => Columns = value; }

    // Alignment
    public string Alignment { get => _alignment; set { _alignment = value ?? string.Empty; OnPropertyChanged(nameof(Alignment)); } }
    public string Align { get => _alignment; set => Alignment = value; }
    public string Alignments { get => _alignment; set => Alignment = value; }
    public string Alineacion { get => _alignment; set => Alignment = value; }
    public string Alineación { get => _alignment; set => Alignment = value; }
    public string ColumnAlignment { get => _alignment; set => Alignment = value; }
    public List<string> ColumnAlignments { get => new List<string> { _alignment }; set { if (value is not null && value.Count > 0) Alignment = value[0]; } }

    // Src / image
    public string Src { get => _src; set { _src = value ?? string.Empty; OnPropertyChanged(nameof(Src)); } }
    public string Source { get => _src; set => Src = value; }
    public string Image { get => _src; set => Src = value; }
    public string ImageUrl { get => _src; set => Src = value; }
    public string ImageSource { get => _src; set => Src = value; }
    public string Path { get => _src; set => Src = value; }
    public string Origin { get => _src; set => Src = value; }
    public string Origen { get => _src; set => Src = value; }
    public string UrlImage { get => _src; set => Src = value; }

    // Alt
    public string Alt { get => _alt; set { _alt = value ?? string.Empty; OnPropertyChanged(nameof(Alt)); } }
    public string AltText { get => _alt; set => Alt = value; }
    public string AlternativeText { get => _alt; set => Alt = value; }
    public string TextoAlternativo { get => _alt; set => Alt = value; }
    public string AltTitle { get => _alt; set => Alt = value; }

    // Code / formula
    public string Code { get => _code; set { _code = value ?? string.Empty; OnPropertyChanged(nameof(Code)); } }
    public string Formula { get => _code; set => Code = value; }
    public string LaTeX { get => _code; set => Code = value; }
    public string Latex { get => _code; set => Code = value; }
    public string LaTex { get => _code; set => Code = value; }
    public string SourceCode { get => _code; set => Code = value; }
    public string Codigo { get => _code; set => Code = value; }
    public string Código { get => _code; set => Code = value; }
    public string Body { get => _code; set => Code = value; }
    public string Markdown { get => _code; set => Code = value; }
    public string Expression { get => _code; set => Code = value; }

    // Inline flag
    public bool Inline { get => _inline; set { _inline = value; OnPropertyChanged(nameof(Inline)); } }
    public bool IsInline { get => _inline; set => Inline = value; }
    public bool Block { get => !_inline; set => Inline = !value; }
    public bool IsBlock { get => !_inline; set => Inline = !value; }

    // Footnote id
    public string Id { get => _id; set { _id = value ?? string.Empty; OnPropertyChanged(nameof(Id)); } }
    public string FootnoteId { get => _id; set => Id = value; }
    public string Identifier { get => _id; set => Id = value; }
    public string Identificador { get => _id; set => Id = value; }
    public string Name { get => _id; set => Id = value; }
    public string SlugId { get => _id; set => Id = value; }

    // Footnote text
    public string FootnoteText { get => _footnoteText; set { _footnoteText = value ?? string.Empty; OnPropertyChanged(nameof(FootnoteText)); } }
    public string Definition { get => _footnoteText; set => FootnoteText = value; }
    public string Texto { get => _footnoteText; set => FootnoteText = value; }
    public string Footnote { get => _footnoteText; set => FootnoteText = value; }
    public string Note { get => _footnoteText; set => FootnoteText = value; }
    public string Nota { get => _footnoteText; set => FootnoteText = value; }

    // Code language
    public string Language { get => _language; set { _language = value ?? string.Empty; OnPropertyChanged(nameof(Language)); } }
    public string Lang { get => _language; set => Language = value; }
    public string Lenguaje { get => _language; set => Language = value; }
    public string LanguageCode { get => _language; set => Language = value; }
    public string LangCode { get => _language; set => Language = value; }
    public string Syntax { get => _language; set => Language = value; }
    public string LangId { get => _language; set => Language = value; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Alias classes
public class FormatDialog : FormatDialogModel { public FormatDialog() : base() { } public FormatDialog(string? k) : base(k) { } }
public class FormatDialogContent : FormatDialogModel { public FormatDialogContent() : base() { } public FormatDialogContent(string? k) : base(k) { } }
public class FormatDialogViewModel : FormatDialogModel { public FormatDialogViewModel() : base() { } public FormatDialogViewModel(string? k) : base(k) { } }
public class FormatDialogData : FormatDialogModel { public FormatDialogData() : base() { } public FormatDialogData(string? k) : base(k) { } }
public class FormatDialogInfo : FormatDialogModel { public FormatDialogInfo() : base() { } public FormatDialogInfo(string? k) : base(k) { } }
public class FormatDialogEntry : FormatDialogModel { public FormatDialogEntry() : base() { } public FormatDialogEntry(string? k) : base(k) { } }
public class FormatDialogContentModel : FormatDialogModel { public FormatDialogContentModel() : base() { } public FormatDialogContentModel(string? k) : base(k) { } }

// Specific alias classes
public class LinkFormatDialog : FormatDialogModel { public LinkFormatDialog() : base("link") { } public LinkFormatDialog(string? k) : base(k ?? "link") { } }
public class LinkDialog : FormatDialogModel { public LinkDialog() : base("link") { } public LinkDialog(string? k) : base(k ?? "link") { } }
public class LinkDialogModel : FormatDialogModel { public LinkDialogModel() : base("link") { } public LinkDialogModel(string? k) : base(k ?? "link") { } }
public class LinkDialogContent : FormatDialogModel { public LinkDialogContent() : base("link") { } public LinkDialogContent(string? k) : base(k ?? "link") { } }
public class LinkDialogViewModel : FormatDialogModel { public LinkDialogViewModel() : base("link") { } public LinkDialogViewModel(string? k) : base(k ?? "link") { } }
public class LinkDialogData : FormatDialogModel { public LinkDialogData() : base("link") { } public LinkDialogData(string? k) : base(k ?? "link") { } }

public class TableFormatDialog : FormatDialogModel { public TableFormatDialog() : base("table") { } public TableFormatDialog(string? k) : base(k ?? "table") { } }
public class TableDialog : FormatDialogModel { public TableDialog() : base("table") { } public TableDialog(string? k) : base(k ?? "table") { } }
public class TableDialogModel : FormatDialogModel { public TableDialogModel() : base("table") { } public TableDialogModel(string? k) : base(k ?? "table") { } }
public class TableDialogContent : FormatDialogModel { public TableDialogContent() : base("table") { } public TableDialogContent(string? k) : base(k ?? "table") { } }
public class TableDialogViewModel : FormatDialogModel { public TableDialogViewModel() : base("table") { } public TableDialogViewModel(string? k) : base(k ?? "table") { } }
public class TableDialogData : FormatDialogModel { public TableDialogData() : base("table") { } public TableDialogData(string? k) : base(k ?? "table") { } }

public class ImageFormatDialog : FormatDialogModel { public ImageFormatDialog() : base("image") { } public ImageFormatDialog(string? k) : base(k ?? "image") { } }
public class ImageDialog : FormatDialogModel { public ImageDialog() : base("image") { } public ImageDialog(string? k) : base(k ?? "image") { } }
public class ImageDialogModel : FormatDialogModel { public ImageDialogModel() : base("image") { } public ImageDialogModel(string? k) : base(k ?? "image") { } }
public class ImageDialogContent : FormatDialogModel { public ImageDialogContent() : base("image") { } public ImageDialogContent(string? k) : base(k ?? "image") { } }
public class ImageDialogViewModel : FormatDialogModel { public ImageDialogViewModel() : base("image") { } public ImageDialogViewModel(string? k) : base(k ?? "image") { } }
public class ImageDialogData : FormatDialogModel { public ImageDialogData() : base("image") { } public ImageDialogData(string? k) : base(k ?? "image") { } }

public class FormulaFormatDialog : FormatDialogModel { public FormulaFormatDialog() : base("formula") { } public FormulaFormatDialog(string? k) : base(k ?? "formula") { } }
public class FormulaDialog : FormatDialogModel { public FormulaDialog() : base("formula") { } public FormulaDialog(string? k) : base(k ?? "formula") { } }
public class FormulaDialogModel : FormatDialogModel { public FormulaDialogModel() : base("formula") { } public FormulaDialogModel(string? k) : base(k ?? "formula") { } }
public class FormulaDialogContent : FormatDialogModel { public FormulaDialogContent() : base("formula") { } public FormulaDialogContent(string? k) : base(k ?? "formula") { } }
public class FormulaDialogViewModel : FormatDialogModel { public FormulaDialogViewModel() : base("formula") { } public FormulaDialogViewModel(string? k) : base(k ?? "formula") { } }
public class FormulaDialogData : FormatDialogModel { public FormulaDialogData() : base("formula") { } public FormulaDialogData(string? k) : base(k ?? "formula") { } }

public class FootnoteFormatDialog : FormatDialogModel { public FootnoteFormatDialog() : base("footnote") { } public FootnoteFormatDialog(string? k) : base(k ?? "footnote") { } }
public class FootnoteDialog : FormatDialogModel { public FootnoteDialog() : base("footnote") { } public FootnoteDialog(string? k) : base(k ?? "footnote") { } }
public class FootnoteDialogModel : FormatDialogModel { public FootnoteDialogModel() : base("footnote") { } public FootnoteDialogModel(string? k) : base(k ?? "footnote") { } }
public class FootnoteDialogContent : FormatDialogModel { public FootnoteDialogContent() : base("footnote") { } public FootnoteDialogContent(string? k) : base(k ?? "footnote") { } }
public class FootnoteDialogViewModel : FormatDialogModel { public FootnoteDialogViewModel() : base("footnote") { } public FootnoteDialogViewModel(string? k) : base(k ?? "footnote") { } }
public class FootnoteDialogData : FormatDialogModel { public FootnoteDialogData() : base("footnote") { } public FootnoteDialogData(string? k) : base(k ?? "footnote") { } }

public class CodeFormatDialog : FormatDialogModel { public CodeFormatDialog() : base("code") { } public CodeFormatDialog(string? k) : base(k ?? "code") { } }
public class CodeDialog : FormatDialogModel { public CodeDialog() : base("code") { } public CodeDialog(string? k) : base(k ?? "code") { } }
public class CodeDialogModel : FormatDialogModel { public CodeDialogModel() : base("code") { } public CodeDialogModel(string? k) : base(k ?? "code") { } }
public class CodeDialogContent : FormatDialogModel { public CodeDialogContent() : base("code") { } public CodeDialogContent(string? k) : base(k ?? "code") { } }
public class CodeDialogViewModel : FormatDialogModel { public CodeDialogViewModel() : base("code") { } public CodeDialogViewModel(string? k) : base(k ?? "code") { } }
public class CodeDialogData : FormatDialogModel { public CodeDialogData() : base("code") { } public CodeDialogData(string? k) : base(k ?? "code") { } }
