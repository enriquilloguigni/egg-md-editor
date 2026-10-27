using System;

namespace EggMdEditor.App;

public class ConfirmacionDeDescarte : ConfirmDiscardOperation { public ConfirmacionDeDescarte() : base() { } public ConfirmacionDeDescarte(OpenDocument? c) : base(c) { } }
public class ConfirmacionDeDescarteOperation : ConfirmDiscardOperation { public ConfirmacionDeDescarteOperation() : base() { } public ConfirmacionDeDescarteOperation(OpenDocument? c) : base(c) { } }
public class ConfirmarDescarteDeCambios : ConfirmDiscardOperation { public ConfirmarDescarteDeCambios() : base() { } public ConfirmarDescarteDeCambios(OpenDocument? c) : base(c) { } }
public class ConfirmarDescarteDeCambiosOperation : ConfirmDiscardOperation { public ConfirmarDescarteDeCambiosOperation() : base() { } public ConfirmarDescarteDeCambiosOperation(OpenDocument? c) : base(c) { } }
public class DiscardUnsavedChanges : ConfirmDiscardOperation { public DiscardUnsavedChanges() : base() { } public DiscardUnsavedChanges(OpenDocument? c) : base(c) { } }
public class DiscardUnsavedChangesOperation : ConfirmDiscardOperation { public DiscardUnsavedChangesOperation() : base() { } public DiscardUnsavedChangesOperation(OpenDocument? c) : base(c) { } }
public class UnsavedChangesDiscard : ConfirmDiscardOperation { public UnsavedChangesDiscard() : base() { } public UnsavedChangesDiscard(OpenDocument? c) : base(c) { } }
public class UnsavedChangesDiscardOperation : ConfirmDiscardOperation { public UnsavedChangesDiscardOperation() : base() { } public UnsavedChangesDiscardOperation(OpenDocument? c) : base(c) { } }
public class DescarteDeCambios : ConfirmDiscardOperation { public DescarteDeCambios() : base() { } public DescarteDeCambios(OpenDocument? c) : base(c) { } }
public class DescarteDeCambiosOperation : ConfirmDiscardOperation { public DescarteDeCambiosOperation() : base() { } public DescarteDeCambiosOperation(OpenDocument? c) : base(c) { } }
public class ConfirmDiscardChanges : ConfirmDiscardOperation { public ConfirmDiscardChanges() : base() { } public ConfirmDiscardChanges(OpenDocument? c) : base(c) { } }
public class ConfirmDiscardChangesOperation : ConfirmDiscardOperation { public ConfirmDiscardChangesOperation() : base() { } public ConfirmDiscardChangesOperation(OpenDocument? c) : base(c) { } }
