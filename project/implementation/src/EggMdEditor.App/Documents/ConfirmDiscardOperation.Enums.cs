using System;

namespace EggMdEditor.App;

// Type aliases for common naming variants.
using ConfirmDiscardChoice = ConfirmDiscardOption;
using ConfirmDiscardAction = ConfirmDiscardOption;
using ConfirmDiscardDecision = ConfirmDiscardOption;
using DiscardOption = ConfirmDiscardOption;
using DiscardChoice = ConfirmDiscardOption;
using DiscardAction = ConfirmDiscardOption;
using DiscardDecision = ConfirmDiscardOption;
using DescarteOption = ConfirmDiscardOption;
using DescarteChoice = ConfirmDiscardOption;
using DescarteAction = ConfirmDiscardOption;
using DescarteDecision = ConfirmDiscardOption;
using ConfirmOption = ConfirmDiscardOption;
using ConfirmChoice = ConfirmDiscardOption;
using ConfirmAction = ConfirmDiscardOption;
using ConfirmDecision = ConfirmDiscardOption;
using ConfirmationOption = ConfirmDiscardOption;
using ConfirmationChoice = ConfirmDiscardOption;
using ConfirmationAction = ConfirmDiscardOption;
using ConfirmationDecision = ConfirmDiscardOption;
using PendingOption = ConfirmDiscardOption;
using PendingChoice = ConfirmDiscardOption;
using PendingAction = ConfirmDiscardOption;
using PendingDecision = ConfirmDiscardOption;
using CloseOption = ConfirmDiscardOption;
using CloseChoice = ConfirmDiscardOption;
using CloseAction = ConfirmDiscardOption;
using CloseDecision = ConfirmDiscardOption;

using DiscardResult = ConfirmDiscardResult;
using DescarteResult = ConfirmDiscardResult;
using ConfirmResult = ConfirmDiscardResult;
using ConfirmationResult = ConfirmDiscardResult;
using PendingResult = ConfirmDiscardResult;
using CloseResult = ConfirmDiscardResult;
using DiscardStatus = ConfirmDiscardStatus;
using DescarteStatus = ConfirmDiscardStatus;
using ConfirmStatus = ConfirmDiscardStatus;
using ConfirmationStatus = ConfirmDiscardStatus;
using PendingStatus = ConfirmDiscardStatus;
using CloseStatus = ConfirmDiscardStatus;

public enum ConfirmDiscardOption
{
    Save = 0,
    Guardar = 0,
    DoNotSave = 1,
    DontSave = 1,
    Discard = 1,
    NoGuardar = 1,
    Cancel = 2,
    Cancelar = 2,
}

public enum ConfirmDiscardChoice { Save = 0, Guardar = 0, DoNotSave = 1, DontSave = 1, Discard = 1, NoGuardar = 1, Cancel = 2, Cancelar = 2 }

public enum ConfirmDiscardResult
{
    Saved = 0,
    Discarded = 1,
    Cancelled = 2,
    Completed = 0,
    Error = 3,
    Proceed = 0,
    Abort = 2,
}

public enum ConfirmDiscardStatus
{
    Saved = 0,
    Discarded = 1,
    Cancelled = 2,
    Completed = 0,
    Error = 3,
    Proceed = 0,
    Abort = 2,
}
