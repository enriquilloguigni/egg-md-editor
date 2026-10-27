using System;

namespace EggMdEditor.App;

/// <summary>
/// Operación Nuevo y reemplazo de documento (T-025, OP-008, BR-F002-001/002).
/// </summary>
public class NuevoOperation
{
    public OpenDocument? CurrentDocument { get; set; }
    public OpenDocument Document { get => CurrentDocument ?? new OpenDocument(); set => CurrentDocument = value; }
    public OpenDocument? OpenDocument { get => CurrentDocument; set => CurrentDocument = value; }

    public NuevoOperation() { }
    public NuevoOperation(OpenDocument? current) { CurrentDocument = current; }

    public static OpenDocument Execute() => new OpenDocument();

    public OpenDocument ExecuteInstance()
    {
        var doc = new OpenDocument();
        CurrentDocument = doc;
        return doc;
    }

    public static OpenDocument Execute(OpenDocument? current) => new OpenDocument();

    public static OpenDocument Execute(OpenDocument? current, OpenDocument? replacement) => replacement ?? new OpenDocument();

    public OpenDocument ExecuteInstance(OpenDocument? current = null)
    {
        var doc = new OpenDocument();
        CurrentDocument = doc;
        return doc;
    }

    public OpenDocument ExecuteInstance(OpenDocument? current, OpenDocument? replacement)
    {
        CurrentDocument = replacement ?? new OpenDocument();
        return CurrentDocument;
    }

    public OpenDocument Execute(OpenDocument? current, bool force)
    {
        CurrentDocument = current;
        return Execute();
    }

    public static OpenDocument Run(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Invoke(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Perform(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Apply(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Create(OpenDocument? current = null) => Execute(current);
    public static OpenDocument New(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Nuevo(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Replace(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Replace(OpenDocument? current, OpenDocument? replacement) => Execute(current, replacement);
    public static OpenDocument ReplaceDocument(OpenDocument? current = null) => Execute(current);
    public static OpenDocument ReplaceDocument(OpenDocument? current, OpenDocument? replacement) => Execute(current, replacement);
    public static OpenDocument ReplaceCurrent(OpenDocument? current = null) => Execute(current);
    public static OpenDocument ReplaceWithNew(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Do(OpenDocument? current = null) => Execute(current);
    public static OpenDocument Start(OpenDocument? current = null) => Execute(current);

    public bool TryExecute(out OpenDocument document)
    {
        document = Execute();
        return true;
    }
    public bool TryNew(out OpenDocument document)
    {
        document = Execute();
        return true;
    }

    public static OpenDocument ExecuteNew() => new OpenDocument();
    public static OpenDocument RunNew() => new OpenDocument();
    public static OpenDocument PerformNew() => new OpenDocument();

    public NuevoResult Result { get; set; } = NuevoResult.Completed;
    public NuevoStatus Status { get => (NuevoStatus)Result; set => Result = (NuevoResult)value; }
    public OperationResult OperationResult { get; set; } = OperationResult.Completed;
    public FileOperationResult FileOperationResult { get; set; } = FileOperationResult.Completed;
}

public enum NuevoResult
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
    Completada = 0,
    Cancelada = 1,
    ErrorOperacion = 2,
}

public enum NuevoStatus
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
}

public enum OperationResult
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
    Completada = 0,
    Cancelada = 1,
    ErrorOperacion = 2,
}

public enum FileOperationResult
{
    Completed = 0,
    Cancelled = 1,
    Error = 2,
    Completada = 0,
    Cancelada = 1,
    ErrorOperacion = 2,
}

public class NewDocumentOperation : NuevoOperation
{
    public NewDocumentOperation() : base() { }
    public NewDocumentOperation(OpenDocument? current) : base(current) { }
}

public class NewOperation : NuevoOperation
{
    public NewOperation() : base() { }
    public NewOperation(OpenDocument? current) : base(current) { }
}

public class DocumentReplacement : NuevoOperation
{
    public DocumentReplacement() : base() { }
    public DocumentReplacement(OpenDocument? current) : base(current) { }
}

public class DocumentReplacer : NuevoOperation
{
    public DocumentReplacer() : base() { }
    public DocumentReplacer(OpenDocument? current) : base(current) { }
}

public class ReplaceDocumentOperation : NuevoOperation
{
    public ReplaceDocumentOperation() : base() { }
    public ReplaceDocumentOperation(OpenDocument? current) : base(current) { }
}

public class NewDocument : NuevoOperation
{
    public NewDocument() : base() { }
    public NewDocument(OpenDocument? current) : base(current) { }
}

public class Nuevo : NuevoOperation
{
    public Nuevo() : base() { }
    public Nuevo(OpenDocument? current) : base(current) { }
}

public class NuevoDocumento : NuevoOperation
{
    public NuevoDocumento() : base() { }
    public NuevoDocumento(OpenDocument? current) : base(current) { }
}

public class NewDocumentService : NuevoOperation
{
    public NewDocumentService() : base() { }
    public NewDocumentService(OpenDocument? current) : base(current) { }
}

public class NuevoService : NuevoOperation
{
    public NuevoService() : base() { }
    public NuevoService(OpenDocument? current) : base(current) { }
}

public class NewDocumentManager : NuevoOperation
{
    public NewDocumentManager() : base() { }
    public NewDocumentManager(OpenDocument? current) : base(current) { }
}

public class NuevoManager : NuevoOperation
{
    public NuevoManager() : base() { }
    public NuevoManager(OpenDocument? current) : base(current) { }
}
