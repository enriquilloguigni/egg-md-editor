namespace EggMdEditor.App;

public class ArchivoCoordinator : FileCoordinator
{
    public ArchivoCoordinator() : base() { }
    public ArchivoCoordinator(OpenDocument? document) : base(document) { }
    public ArchivoCoordinator(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public ArchivoCoordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class FileOperationCoordinator : FileCoordinator
{
    public FileOperationCoordinator() : base() { }
    public FileOperationCoordinator(OpenDocument? document) : base(document) { }
    public FileOperationCoordinator(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public FileOperationCoordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class OperationCoordinator : FileCoordinator
{
    public OperationCoordinator() : base() { }
    public OperationCoordinator(OpenDocument? document) : base(document) { }
    public OperationCoordinator(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public OperationCoordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class DocumentCoordinator : FileCoordinator
{
    public DocumentCoordinator() : base() { }
    public DocumentCoordinator(OpenDocument? document) : base(document) { }
    public DocumentCoordinator(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public DocumentCoordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class FileCoordinatorService : FileCoordinator
{
    public FileCoordinatorService() : base() { }
    public FileCoordinatorService(OpenDocument? document) : base(document) { }
    public FileCoordinatorService(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public FileCoordinatorService(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class ArchivoCoordinatorService : FileCoordinator
{
    public ArchivoCoordinatorService() : base() { }
    public ArchivoCoordinatorService(OpenDocument? document) : base(document) { }
    public ArchivoCoordinatorService(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public ArchivoCoordinatorService(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class FileCoordinatorManager : FileCoordinator
{
    public FileCoordinatorManager() : base() { }
    public FileCoordinatorManager(OpenDocument? document) : base(document) { }
    public FileCoordinatorManager(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public FileCoordinatorManager(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class Coordinator : FileCoordinator
{
    public Coordinator() : base() { }
    public Coordinator(OpenDocument? document) : base(document) { }
    public Coordinator(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public Coordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class ArchivoCoordinador : FileCoordinator
{
    public ArchivoCoordinador() : base() { }
    public ArchivoCoordinador(OpenDocument? document) : base(document) { }
    public ArchivoCoordinador(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public ArchivoCoordinador(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public class FileCoordinatorOperation : FileCoordinator
{
    public FileCoordinatorOperation() : base() { }
    public FileCoordinatorOperation(OpenDocument? document) : base(document) { }
    public FileCoordinatorOperation(OpenDocument? document, RecentFiles? recentFiles) : base(document, recentFiles) { }
    public FileCoordinatorOperation(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) : base(document, recentFiles, recoveryCopy) { }
}

public static class FileCoordinators
{
    public static FileCoordinator Create() => new FileCoordinator();
    public static FileCoordinator Create(OpenDocument? document) => new FileCoordinator(document);
    public static FileCoordinator Create(OpenDocument? document, RecentFiles? recentFiles) => new FileCoordinator(document, recentFiles);
    public static FileCoordinator Create(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy) => new FileCoordinator(document, recentFiles, recoveryCopy);
    public static FileCoordinator New() => new FileCoordinator();
}
