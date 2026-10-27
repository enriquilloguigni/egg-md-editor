namespace EggMdEditor.App;

public class AutosaveOperation : AutoguardadoOperation
{
    public AutosaveOperation() : base() { }
    public AutosaveOperation(OpenDocument? c) : base(c) { }
    public AutosaveOperation(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutosaveOperation(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutosaveOperation(OpenDocument? c, string? p) : base(c, p) { }
    public AutosaveOperation(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoSaveOperation : AutoguardadoOperation
{
    public AutoSaveOperation() : base() { }
    public AutoSaveOperation(OpenDocument? c) : base(c) { }
    public AutoSaveOperation(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoSaveOperation(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoSaveOperation(OpenDocument? c, string? p) : base(c, p) { }
    public AutoSaveOperation(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoguardarOperation : AutoguardadoOperation
{
    public AutoguardarOperation() : base() { }
    public AutoguardarOperation(OpenDocument? c) : base(c) { }
    public AutoguardarOperation(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoguardarOperation(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoguardarOperation(OpenDocument? c, string? p) : base(c, p) { }
    public AutoguardarOperation(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoGuardadoOperation : AutoguardadoOperation
{
    public AutoGuardadoOperation() : base() { }
    public AutoGuardadoOperation(OpenDocument? c) : base(c) { }
    public AutoGuardadoOperation(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoGuardadoOperation(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoGuardadoOperation(OpenDocument? c, string? p) : base(c, p) { }
    public AutoGuardadoOperation(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class RecoveryAutosaveOperation : AutoguardadoOperation
{
    public RecoveryAutosaveOperation() : base() { }
    public RecoveryAutosaveOperation(OpenDocument? c) : base(c) { }
    public RecoveryAutosaveOperation(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public RecoveryAutosaveOperation(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public RecoveryAutosaveOperation(OpenDocument? c, string? p) : base(c, p) { }
    public RecoveryAutosaveOperation(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutosaveService : AutoguardadoOperation
{
    public AutosaveService() : base() { }
    public AutosaveService(OpenDocument? c) : base(c) { }
    public AutosaveService(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutosaveService(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutosaveService(OpenDocument? c, string? p) : base(c, p) { }
    public AutosaveService(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoguardadoService : AutoguardadoOperation
{
    public AutoguardadoService() : base() { }
    public AutoguardadoService(OpenDocument? c) : base(c) { }
    public AutoguardadoService(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoguardadoService(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoguardadoService(OpenDocument? c, string? p) : base(c, p) { }
    public AutoguardadoService(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutosaveManager : AutoguardadoOperation
{
    public AutosaveManager() : base() { }
    public AutosaveManager(OpenDocument? c) : base(c) { }
    public AutosaveManager(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutosaveManager(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutosaveManager(OpenDocument? c, string? p) : base(c, p) { }
    public AutosaveManager(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoguardadoManager : AutoguardadoOperation
{
    public AutoguardadoManager() : base() { }
    public AutoguardadoManager(OpenDocument? c) : base(c) { }
    public AutoguardadoManager(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoguardadoManager(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoguardadoManager(OpenDocument? c, string? p) : base(c, p) { }
    public AutoguardadoManager(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutosaveHelper : AutoguardadoOperation
{
    public AutosaveHelper() : base() { }
    public AutosaveHelper(OpenDocument? c) : base(c) { }
    public AutosaveHelper(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutosaveHelper(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutosaveHelper(OpenDocument? c, string? p) : base(c, p) { }
    public AutosaveHelper(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

public class AutoguardadoHelper : AutoguardadoOperation
{
    public AutoguardadoHelper() : base() { }
    public AutoguardadoHelper(OpenDocument? c) : base(c) { }
    public AutoguardadoHelper(OpenDocument? c, RecoveryCopy? r) : base(c, r) { }
    public AutoguardadoHelper(RecoveryCopy? r, OpenDocument? c) : base(r, c) { }
    public AutoguardadoHelper(OpenDocument? c, string? p) : base(c, p) { }
    public AutoguardadoHelper(OpenDocument? c, RecoveryCopy? r, string? p) : base(c, r, p) { }
}

// static helper classes
public static class AutosaveHelper2 { public static RecoveryCopy Save(OpenDocument? d, RecoveryCopy? r = null, string? p = null) => AutoguardadoOperation.Execute(d, r, p); }
public static class AutoguardadoHelper2 { public static RecoveryCopy Save(OpenDocument? d, RecoveryCopy? r = null, string? p = null) => AutoguardadoOperation.Execute(d, r, p); }
public static class AutosaveUtil { public static RecoveryCopy Execute(OpenDocument? d, RecoveryCopy? r = null, string? p = null) => AutoguardadoOperation.Execute(d, r, p); }
public static class AutoguardadoUtil { public static RecoveryCopy Execute(OpenDocument? d, RecoveryCopy? r = null, string? p = null) => AutoguardadoOperation.Execute(d, r, p); }
