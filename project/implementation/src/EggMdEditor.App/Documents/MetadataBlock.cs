using System.ComponentModel;

namespace EggMdEditor.App;

public class MetadataBlock : IEquatable<MetadataBlock>, INotifyPropertyChanged
{
    private bool _isPresent;
    private string _content = string.Empty;
    private MetadataPosition _position = MetadataPosition.Start;
    private string _delimiters = string.Empty;

    public MetadataBlock() { }

    public MetadataBlock(bool isPresent, string? content = null, string? delimiters = null)
    {
        _isPresent = isPresent;
        _content = content ?? string.Empty;
        _delimiters = delimiters ?? string.Empty;
    }

    public bool IsPresent
    {
        get => _isPresent;
        set
        {
            if (_isPresent == value) return;
            _isPresent = value;
            OnPropertyChanged(nameof(IsPresent));
            OnPropertyChanged(nameof(Presence));
            OnPropertyChanged(nameof(Present));
        }
    }

    public bool Presence { get => _isPresent; set => IsPresent = value; }
    public bool Present { get => _isPresent; set => IsPresent = value; }
    public bool Presencia { get => _isPresent; set => IsPresent = value; }

    public string Content
    {
        get => _content;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_content, normalized, StringComparison.Ordinal)) return;
            _content = normalized;
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(FullContent));
            OnPropertyChanged(nameof(RawContent));
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(BlockContent));
        }
    }

    public string FullContent { get => _content; set => Content = value; }
    public string ContenidoIntegro { get => _content; set => Content = value; }
    public string RawContent { get => _content; set => Content = value; }
    public string Text { get => _content; set => Content = value; }
    public string BlockContent { get => _content; set => Content = value; }

    public MetadataPosition Position
    {
        get => _position;
        set
        {
            if (_position == value) return;
            _position = value;
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Posicion));
        }
    }

    public MetadataPosition Posicion
    {
        get => _position;
        set => Position = value;
    }

    public string Delimiters
    {
        get => _delimiters;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_delimiters, normalized, StringComparison.Ordinal)) return;
            _delimiters = normalized;
            OnPropertyChanged(nameof(Delimiters));
            OnPropertyChanged(nameof(Delimiter));
            OnPropertyChanged(nameof(DelimiterText));
            OnPropertyChanged(nameof(DelimitersText));
        }
    }

    public string Delimiter { get => _delimiters; set => Delimiters = value; }
    public string DelimiterText { get => _delimiters; set => Delimiters = value; }
    public string DelimitersText { get => _delimiters; set => Delimiters = value; }
    public string Delimitadores { get => _delimiters; set => Delimiters = value; }

    public static MetadataBlock None { get; } = new MetadataBlock(false, string.Empty, string.Empty);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public bool Equals(MetadataBlock? other)
    {
        if (other is null) return false;
        return _isPresent == other._isPresent
            && string.Equals(_content, other._content, StringComparison.Ordinal)
            && _position == other._position
            && string.Equals(_delimiters, other._delimiters, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as MetadataBlock);
    public override int GetHashCode() => HashCode.Combine(_isPresent, _content, _position, _delimiters);
}

public class FrontMatter : MetadataBlock
{
    public FrontMatter() : base() { }
    public FrontMatter(bool isPresent, string? content = null, string? delimiters = null)
        : base(isPresent, content, delimiters) { }
}

public class FrontmatterBlock : MetadataBlock
{
    public FrontmatterBlock() : base() { }
    public FrontmatterBlock(bool isPresent, string? content = null, string? delimiters = null)
        : base(isPresent, content, delimiters) { }
}