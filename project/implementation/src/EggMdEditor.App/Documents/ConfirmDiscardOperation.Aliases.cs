using System;

namespace EggMdEditor.App;

public class ConfirmDiscard : ConfirmDiscardOperation { public ConfirmDiscard() : base() { } public ConfirmDiscard(OpenDocument? c) : base(c) { } public ConfirmDiscard(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmDiscard(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmDiscardOp : ConfirmDiscardOperation { public ConfirmDiscardOp() : base() { } public ConfirmDiscardOp(OpenDocument? c) : base(c) { } public ConfirmDiscardOp(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmDiscardOp(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmDiscardService : ConfirmDiscardOperation { public ConfirmDiscardService() : base() { } public ConfirmDiscardService(OpenDocument? c) : base(c) { } public ConfirmDiscardService(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmDiscardService(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmDiscardManager : ConfirmDiscardOperation { public ConfirmDiscardManager() : base() { } public ConfirmDiscardManager(OpenDocument? c) : base(c) { } public ConfirmDiscardManager(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmDiscardManager(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmDiscardHelper : ConfirmDiscardOperation { public ConfirmDiscardHelper() : base() { } public ConfirmDiscardHelper(OpenDocument? c) : base(c) { } public ConfirmDiscardHelper(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmDiscardHelper(RecentFiles? r, OpenDocument? c) : base(r, c) { } }

public class ConfirmarDescarte : ConfirmDiscardOperation { public ConfirmarDescarte() : base() { } public ConfirmarDescarte(OpenDocument? c) : base(c) { } public ConfirmarDescarte(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmarDescarte(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmarDescarteOperation : ConfirmDiscardOperation { public ConfirmarDescarteOperation() : base() { } public ConfirmarDescarteOperation(OpenDocument? c) : base(c) { } public ConfirmarDescarteOperation(OpenDocument? c, RecentFiles? r) : base(c, r) { } public ConfirmarDescarteOperation(RecentFiles? r, OpenDocument? c) : base(r, c) { } }
public class ConfirmarDescarteOp : ConfirmDiscardOperation { public ConfirmarDescarteOp() : base() { } public ConfirmarDescarteOp(OpenDocument? c) : base(c) { } }
public class ConfirmarDescarteService : ConfirmDiscardOperation { public ConfirmarDescarteService() : base() { } public ConfirmarDescarteService(OpenDocument? c) : base(c) { } }
public class ConfirmarDescarteManager : ConfirmDiscardOperation { public ConfirmarDescarteManager() : base() { } public ConfirmarDescarteManager(OpenDocument? c) : base(c) { } }
public class ConfirmarDescarteHelper : ConfirmDiscardOperation { public ConfirmarDescarteHelper() : base() { } public ConfirmarDescarteHelper(OpenDocument? c) : base(c) { } }

public class Descarte : ConfirmDiscardOperation { public Descarte() : base() { } public Descarte(OpenDocument? c) : base(c) { } }
public class DescarteOperation : ConfirmDiscardOperation { public DescarteOperation() : base() { } public DescarteOperation(OpenDocument? c) : base(c) { } }
public class DescarteOp : ConfirmDiscardOperation { public DescarteOp() : base() { } public DescarteOp(OpenDocument? c) : base(c) { } }
public class DescarteService : ConfirmDiscardOperation { public DescarteService() : base() { } public DescarteService(OpenDocument? c) : base(c) { } }
public class DescarteManager : ConfirmDiscardOperation { public DescarteManager() : base() { } public DescarteManager(OpenDocument? c) : base(c) { } }
public class DescarteHelper : ConfirmDiscardOperation { public DescarteHelper() : base() { } public DescarteHelper(OpenDocument? c) : base(c) { } }

public class Discard : ConfirmDiscardOperation { public Discard() : base() { } public Discard(OpenDocument? c) : base(c) { } }
public class DiscardOp : ConfirmDiscardOperation { public DiscardOp() : base() { } public DiscardOp(OpenDocument? c) : base(c) { } }
public class DiscardOperation : ConfirmDiscardOperation { public DiscardOperation() : base() { } public DiscardOperation(OpenDocument? c) : base(c) { } }
public class DiscardService : ConfirmDiscardOperation { public DiscardService() : base() { } public DiscardService(OpenDocument? c) : base(c) { } }
public class DiscardManager : ConfirmDiscardOperation { public DiscardManager() : base() { } public DiscardManager(OpenDocument? c) : base(c) { } }
public class DiscardHelper : ConfirmDiscardOperation { public DiscardHelper() : base() { } public DiscardHelper(OpenDocument? c) : base(c) { } }
public class DiscardUtil : ConfirmDiscardOperation { public DiscardUtil() : base() { } public DiscardUtil(OpenDocument? c) : base(c) { } }
public class DiscardUtils : ConfirmDiscardOperation { public DiscardUtils() : base() { } public DiscardUtils(OpenDocument? c) : base(c) { } }
public class DiscardUtility : ConfirmDiscardOperation { public DiscardUtility() : base() { } public DiscardUtility(OpenDocument? c) : base(c) { } }

public class DiscardChanges : ConfirmDiscardOperation { public DiscardChanges() : base() { } public DiscardChanges(OpenDocument? c) : base(c) { } }
public class DiscardChangesOperation : ConfirmDiscardOperation { public DiscardChangesOperation() : base() { } public DiscardChangesOperation(OpenDocument? c) : base(c) { } }
public class DiscardChangesOp : ConfirmDiscardOperation { public DiscardChangesOp() : base() { } public DiscardChangesOp(OpenDocument? c) : base(c) { } }
public class DiscardChangesService : ConfirmDiscardOperation { public DiscardChangesService() : base() { } public DiscardChangesService(OpenDocument? c) : base(c) { } }
public class DiscardChangesManager : ConfirmDiscardOperation { public DiscardChangesManager() : base() { } public DiscardChangesManager(OpenDocument? c) : base(c) { } }
public class DiscardChangesHelper : ConfirmDiscardOperation { public DiscardChangesHelper() : base() { } public DiscardChangesHelper(OpenDocument? c) : base(c) { } }

public class Confirm : ConfirmDiscardOperation { public Confirm() : base() { } public Confirm(OpenDocument? c) : base(c) { } }
public class ConfirmOperation : ConfirmDiscardOperation { public ConfirmOperation() : base() { } public ConfirmOperation(OpenDocument? c) : base(c) { } }
public class ConfirmOp : ConfirmDiscardOperation { public ConfirmOp() : base() { } public ConfirmOp(OpenDocument? c) : base(c) { } }
public class ConfirmService : ConfirmDiscardOperation { public ConfirmService() : base() { } public ConfirmService(OpenDocument? c) : base(c) { } }
public class ConfirmManager : ConfirmDiscardOperation { public ConfirmManager() : base() { } public ConfirmManager(OpenDocument? c) : base(c) { } }
public class ConfirmHelper : ConfirmDiscardOperation { public ConfirmHelper() : base() { } public ConfirmHelper(OpenDocument? c) : base(c) { } }

public class Confirmation : ConfirmDiscardOperation { public Confirmation() : base() { } public Confirmation(OpenDocument? c) : base(c) { } }
public class ConfirmationOperation : ConfirmDiscardOperation { public ConfirmationOperation() : base() { } public ConfirmationOperation(OpenDocument? c) : base(c) { } }
public class ConfirmationService : ConfirmDiscardOperation { public ConfirmationService() : base() { } public ConfirmationService(OpenDocument? c) : base(c) { } }

public class Confirmacion : ConfirmDiscardOperation { public Confirmacion() : base() { } public Confirmacion(OpenDocument? c) : base(c) { } }
public class ConfirmacionOperation : ConfirmDiscardOperation { public ConfirmacionOperation() : base() { } public ConfirmacionOperation(OpenDocument? c) : base(c) { } }
public class ConfirmacionDescarte : ConfirmDiscardOperation { public ConfirmacionDescarte() : base() { } public ConfirmacionDescarte(OpenDocument? c) : base(c) { } }
public class ConfirmacionDescarteOperation : ConfirmDiscardOperation { public ConfirmacionDescarteOperation() : base() { } public ConfirmacionDescarteOperation(OpenDocument? c) : base(c) { } }

public class DiscardConfirmation : ConfirmDiscardOperation { public DiscardConfirmation() : base() { } public DiscardConfirmation(OpenDocument? c) : base(c) { } }
public class DiscardConfirmationOperation : ConfirmDiscardOperation { public DiscardConfirmationOperation() : base() { } public DiscardConfirmationOperation(OpenDocument? c) : base(c) { } }
public class UnsavedChangesConfirmation : ConfirmDiscardOperation { public UnsavedChangesConfirmation() : base() { } public UnsavedChangesConfirmation(OpenDocument? c) : base(c) { } }
public class PendingConfirmation : ConfirmDiscardOperation { public PendingConfirmation() : base() { } public PendingConfirmation(OpenDocument? c) : base(c) { } }
public class PendingConfirmationOperation : ConfirmDiscardOperation { public PendingConfirmationOperation() : base() { } public PendingConfirmationOperation(OpenDocument? c) : base(c) { } }
public class CloseConfirmation : ConfirmDiscardOperation { public CloseConfirmation() : base() { } public CloseConfirmation(OpenDocument? c) : base(c) { } }
public class CloseConfirmationOperation : ConfirmDiscardOperation { public CloseConfirmationOperation() : base() { } public CloseConfirmationOperation(OpenDocument? c) : base(c) { } }

public static class ConfirmDiscardHelper2 { public static object? Confirm(OpenDocument? d = null, object? o = null, string? p = null, RecentFiles? r = null, RecoveryCopy? rc = null) => ConfirmDiscardOperation.Execute(d, o, p, r, rc); }
public static class DiscardHelper2 { public static object? Discard(OpenDocument? d = null, RecoveryCopy? rc = null) { if (rc is not null) { try { rc.Invalidate(); } catch { try { rc.MarkObsolete(); } catch { } } } return ConfirmDiscardOption.DoNotSave; } }
public static class DescarteHelper2 { public static object? Descartar(OpenDocument? d = null, RecoveryCopy? rc = null) { if (rc is not null) { try { rc.Invalidate(); } catch { try { rc.MarkObsolete(); } catch { } } } return ConfirmDiscardOption.DoNotSave; } }
