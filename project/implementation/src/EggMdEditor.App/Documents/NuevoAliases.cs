namespace EggMdEditor.App;

public class NewDoc : NuevoOperation { public NewDoc() : base() { } public NewDoc(OpenDocument? c) : base(c) { } }
public class NewDocs : NuevoOperation { public NewDocs() : base() { } public NewDocs(OpenDocument? c) : base(c) { } }
public class NewFile : NuevoOperation { public NewFile() : base() { } public NewFile(OpenDocument? c) : base(c) { } }
public class NewFiles : NuevoOperation { public NewFiles() : base() { } public NewFiles(OpenDocument? c) : base(c) { } }
public class NewMarkdown : NuevoOperation { public NewMarkdown() : base() { } public NewMarkdown(OpenDocument? c) : base(c) { } }
public class NewMd : NuevoOperation { public NewMd() : base() { } public NewMd(OpenDocument? c) : base(c) { } }
public class NewDocumentCommand : NuevoOperation { public NewDocumentCommand() : base() { } public NewDocumentCommand(OpenDocument? c) : base(c) { } }
public class NewCommand : NuevoOperation { public NewCommand() : base() { } public NewCommand(OpenDocument? c) : base(c) { } }
public class NuevoCommand : NuevoOperation { public NuevoCommand() : base() { } public NuevoCommand(OpenDocument? c) : base(c) { } }
public class FileNew : NuevoOperation { public FileNew() : base() { } public FileNew(OpenDocument? c) : base(c) { } }
public class FileNewOperation : NuevoOperation { public FileNewOperation() : base() { } public FileNewOperation(OpenDocument? c) : base(c) { } }
public class FileNewDocument : NuevoOperation { public FileNewDocument() : base() { } public FileNewDocument(OpenDocument? c) : base(c) { } }
public class FileNuevo : NuevoOperation { public FileNuevo() : base() { } public FileNuevo(OpenDocument? c) : base(c) { } }
public class OperationNew : NuevoOperation { public OperationNew() : base() { } public OperationNew(OpenDocument? c) : base(c) { } }
public class OperationNuevo : NuevoOperation { public OperationNuevo() : base() { } public OperationNuevo(OpenDocument? c) : base(c) { } }
public class DocumentNew : NuevoOperation { public DocumentNew() : base() { } public DocumentNew(OpenDocument? c) : base(c) { } }
public class DocumentNuevo : NuevoOperation { public DocumentNuevo() : base() { } public DocumentNuevo(OpenDocument? c) : base(c) { } }
public class DocumentOperation : NuevoOperation { public DocumentOperation() : base() { } public DocumentOperation(OpenDocument? c) : base(c) { } }
public class DocumentOperations : NuevoOperation { public DocumentOperations() : base() { } public DocumentOperations(OpenDocument? c) : base(c) { } }
public class FileOperation : NuevoOperation { public FileOperation() : base() { } public FileOperation(OpenDocument? c) : base(c) { } }
public class FileOperations2 : NuevoOperation { public FileOperations2() : base() { } public FileOperations2(OpenDocument? c) : base(c) { } }
public class ReplaceDocument : NuevoOperation { public ReplaceDocument() : base() { } public ReplaceDocument(OpenDocument? c) : base(c) { } }
public class Replacer : NuevoOperation { public Replacer() : base() { } public Replacer(OpenDocument? c) : base(c) { } }
public class DocumentReplacementService : NuevoOperation { public DocumentReplacementService() : base() { } public DocumentReplacementService(OpenDocument? c) : base(c) { } }
public class ReplacementOperation : NuevoOperation { public ReplacementOperation() : base() { } public ReplacementOperation(OpenDocument? c) : base(c) { } }
public class NuevoReplacement : NuevoOperation { public NuevoReplacement() : base() { } public NuevoReplacement(OpenDocument? c) : base(c) { } }
public class NewReplacement : NuevoOperation { public NewReplacement() : base() { } public NewReplacement(OpenDocument? c) : base(c) { } }

// Extension-like static helper classes
public static class NuevoHelper { public static OpenDocument Create() => new OpenDocument(); public static OpenDocument New() => new OpenDocument(); public static OpenDocument Nuevo() => new OpenDocument(); }
public static class NewDocumentHelper { public static OpenDocument Create() => new OpenDocument(); public static OpenDocument New() => new OpenDocument(); }
public static class DocumentReplacementHelper { public static OpenDocument Replace(OpenDocument? c = null) => new OpenDocument(); }
