// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AlexMorgan3817 <46600554+AlexMorgan3817@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Explosion.Components;

/// <summary>
/// Gibs on trigger, self explanatory.
/// Also in case of an implant using this, gibs the implant user instead.
/// </summary>
[RegisterComponent]
public sealed partial class GibOnTriggerComponent : Component
{
    /// <summary>
    /// Should gibbing also delete the owners items?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("deleteItems")]
    public bool DeleteItems = false;
}
