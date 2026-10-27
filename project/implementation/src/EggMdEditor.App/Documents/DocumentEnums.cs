namespace EggMdEditor.App;

public enum DocumentEncoding
{
    Utf8 = 0,
}

public enum LineEnding
{
    CRLF = 0,
    LF = 1,
}

public enum DocumentChangeState
{
    Clean = 0,
    Modified = 1,
    Dirty = 1,
    Changed = 1,
    WithChanges = 1,
    ConCambios = 1,
}

public enum DocumentOrigin
{
    New = 0,
    Open = 1,
    Recent = 2,
    Drag = 3,
    DragDrop = 3,
    Arrastrar = 3,
    Recovered = 4,
    RecoveredDocument = 4,
}

public enum DocumentLastOperation
{
    None = 0,
    Ninguna = 0,
    Open = 1,
    Abrir = 1,
    Save = 2,
    Guardar = 2,
    SaveAs = 3,
    SaveAsCopy = 3,
    GuardarComo = 3,
    Recover = 4,
    Recuperar = 4,
}

public enum MetadataPosition
{
    Start = 0,
    Inicio = 0,
}