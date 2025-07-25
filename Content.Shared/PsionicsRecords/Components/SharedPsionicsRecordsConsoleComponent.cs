// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.PsionicsRecords.Systems;
using Content.Shared.Radio;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Shared.PsionicsRecords.Components;

/// <summary>
/// A component for Psionics Records Console storing an active station record key and a currently applied filter
/// </summary>
[RegisterComponent]
[Access(typeof(SharedPsionicsRecordsConsoleSystem))]
public sealed partial class PsionicsRecordsConsoleComponent : Component
{
    /// <summary>
    /// Currently active station record key.
    /// There is no station parameter as the console uses the current station.
    /// </summary>
    /// <remarks>
    /// TODO: in the future this should be clientside instead of something players can fight over.
    /// Client selects a record and tells the server the key it wants records for.
    /// Server then sends a state with just the records, not the listing or filter, and the client updates just that.
    /// I don't know if it's possible to have multiple bui states right now.
    /// </remarks>
    [DataField]
    public uint? ActiveKey;

    /// <summary>
    /// Currently applied filter.
    /// </summary>
    [DataField]
    public StationRecordsFilter? Filter;

    /// <summary>
    /// Channel to send messages to when someone's status gets changed.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Science";

    /// <summary>
    /// Max length of psionics listing strings.
    /// </summary>
    [DataField]
    public uint MaxStringLength = 256;
}
