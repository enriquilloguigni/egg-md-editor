using System.Text.Json.Serialization;

namespace EggMdEditor.App;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryStatus
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryState
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryCopyStatus
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryCopyState
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryValidity
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryCondition
{
    Valid = 0,
    Vigente = 0,
    Current = 0,
    Active = 0,
    Available = 0,
    Good = 0,
    Ok = 0,
    Success = 0,
    Obsolete = 1,
    Obsoleta = 1,
    Stale = 1,
    Invalid = 1,
    Unavailable = 1,
    Bad = 1,
    Failed = 1,
}