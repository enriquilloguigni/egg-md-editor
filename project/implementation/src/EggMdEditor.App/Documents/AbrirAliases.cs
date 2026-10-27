namespace EggMdEditor.App;

public class Abrir : AbrirOperation { public Abrir() : base() { } public Abrir(OpenDocument? c) : base(c) { } }
public class Open : AbrirOperation { public Open() : base() { } public Open(OpenDocument? c) : base(c) { } }
public class AbrirDoc : AbrirOperation { public AbrirDoc() : base() { } public AbrirDoc(OpenDocument? c) : base(c) { } }
public class AbrirDocs : AbrirOperation { public AbrirDocs() : base() { } public AbrirDocs(OpenDocument? c) : base(c) { } }
public class AbrirFichero : AbrirOperation { public AbrirFichero() : base() { } public AbrirFichero(OpenDocument? c) : base(c) { } }
public class AbrirFicheros : AbrirOperation { public AbrirFicheros() : base() { } public AbrirFicheros(OpenDocument? c) : base(c) { } }
public class AbrirMdFile : AbrirOperation { public AbrirMdFile() : base() { } public AbrirMdFile(OpenDocument? c) : base(c) { } }
public class AbrirMarkdownFile : AbrirOperation { public AbrirMarkdownFile() : base() { } public AbrirMarkdownFile(OpenDocument? c) : base(c) { } }
public class AbrirOperationService : AbrirOperation { public AbrirOperationService() : base() { } public AbrirOperationService(OpenDocument? c) : base(c) { } }
public class AbrirService : AbrirOperation { public AbrirService() : base() { } public AbrirService(OpenDocument? c) : base(c) { } }
public class AbrirManager : AbrirOperation { public AbrirManager() : base() { } public AbrirManager(OpenDocument? c) : base(c) { } }
public class AbrirHelper : AbrirOperation { public AbrirHelper() : base() { } public AbrirHelper(OpenDocument? c) : base(c) { } }
public class AbrirUtil : AbrirOperation { public AbrirUtil() : base() { } public AbrirUtil(OpenDocument? c) : base(c) { } }
public class AbrirUtils : AbrirOperation { public AbrirUtils() : base() { } public AbrirUtils(OpenDocument? c) : base(c) { } }
public class AbrirUtility : AbrirOperation { public AbrirUtility() : base() { } public AbrirUtility(OpenDocument? c) : base(c) { } }

public class OpenDoc : AbrirOperation { public OpenDoc() : base() { } public OpenDoc(OpenDocument? c) : base(c) { } }
public class OpenDocs : AbrirOperation { public OpenDocs() : base() { } public OpenDocs(OpenDocument? c) : base(c) { } }
public class OpenMarkdownDoc : AbrirOperation { public OpenMarkdownDoc() : base() { } public OpenMarkdownDoc(OpenDocument? c) : base(c) { } }
public class OpenMdDoc : AbrirOperation { public OpenMdDoc() : base() { } public OpenMdDoc(OpenDocument? c) : base(c) { } }
public class OpenMarkdownFileAlias : AbrirOperation { public OpenMarkdownFileAlias() : base() { } public OpenMarkdownFileAlias(OpenDocument? c) : base(c) { } }
public class OpenMdFileAlias : AbrirOperation { public OpenMdFileAlias() : base() { } public OpenMdFileAlias(OpenDocument? c) : base(c) { } }
public class OpenOperationService : AbrirOperation { public OpenOperationService() : base() { } public OpenOperationService(OpenDocument? c) : base(c) { } }
public class OpenService : AbrirOperation { public OpenService() : base() { } public OpenService(OpenDocument? c) : base(c) { } }
public class OpenManager : AbrirOperation { public OpenManager() : base() { } public OpenManager(OpenDocument? c) : base(c) { } }
public class OpenHelper : AbrirOperation { public OpenHelper() : base() { } public OpenHelper(OpenDocument? c) : base(c) { } }
public class OpenUtil : AbrirOperation { public OpenUtil() : base() { } public OpenUtil(OpenDocument? c) : base(c) { } }
public class OpenUtils : AbrirOperation { public OpenUtils() : base() { } public OpenUtils(OpenDocument? c) : base(c) { } }
public class OpenUtility : AbrirOperation { public OpenUtility() : base() { } public OpenUtility(OpenDocument? c) : base(c) { } }
public class Opener : AbrirOperation { public Opener() : base() { } public Opener(OpenDocument? c) : base(c) { } }
public class DocumentOpener : AbrirOperation { public DocumentOpener() : base() { } public DocumentOpener(OpenDocument? c) : base(c) { } }
public class MarkdownOpener : AbrirOperation { public MarkdownOpener() : base() { } public MarkdownOpener(OpenDocument? c) : base(c) { } }
public class MdOpener : AbrirOperation { public MdOpener() : base() { } public MdOpener(OpenDocument? c) : base(c) { } }
public class FileOpenerAlias : AbrirOperation { public FileOpenerAlias() : base() { } public FileOpenerAlias(OpenDocument? c) : base(c) { } }

public class DragOpener : AbrirOperation { public DragOpener() : base() { } public DragOpener(OpenDocument? c) : base(c) { } }
public class DropOpener : AbrirOperation { public DropOpener() : base() { } public DropOpener(OpenDocument? c) : base(c) { } }
public class DragDropOpener : AbrirOperation { public DragDropOpener() : base() { } public DragDropOpener(OpenDocument? c) : base(c) { } }
public class ArrastreOpener : AbrirOperation { public ArrastreOpener() : base() { } public ArrastreOpener(OpenDocument? c) : base(c) { } }

public static class AbrirHelperStatic { public static OpenDocument Open(string? path) => AbrirOperation.Execute(path); public static OpenDocument Abrir(string? path) => AbrirOperation.Execute(path); }
public static class OpenHelperStatic { public static OpenDocument Open(string? path) => AbrirOperation.Execute(path); public static OpenDocument Load(string? path) => AbrirOperation.Execute(path); }
public static class DragDropHelper { public static OpenDocument Open(System.Collections.Generic.IEnumerable<string>? paths) => AbrirOperation.Execute(paths); }

public class AbrirCommand : AbrirOperation { public AbrirCommand() : base() { } public AbrirCommand(OpenDocument? c) : base(c) { } }
public class OpenCommand : AbrirOperation { public OpenCommand() : base() { } public OpenCommand(OpenDocument? c) : base(c) { } }
public class AbrirAction : AbrirOperation { public AbrirAction() : base() { } public AbrirAction(OpenDocument? c) : base(c) { } }
public class OpenAction : AbrirOperation { public OpenAction() : base() { } public OpenAction(OpenDocument? c) : base(c) { } }