// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Damage.Prototypes
{
    /// <summary>
    ///     A Group of <see cref="DamageTypePrototype"/>s.
    /// </summary>
    /// <remarks>
    ///     These groups can be used to specify supported damage types of a <see cref="DamageContainerPrototype"/>, or
    ///     to change/get/set damage in a <see cref="DamageableComponent"/>.
    /// </remarks>
    [Prototype("damageGroup", 2)]
    [Serializable, NetSerializable]
    public sealed partial class DamageGroupPrototype : IPrototype
    {
        [IdDataField] public string ID { get; } = default!;

        [DataField(required: true)]
        private LocId Name { get; set; }

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString(Name);

        [DataField("damageTypes", required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<DamageTypePrototype>))]
        public List<string> DamageTypes { get; private set; } = default!;
    }
}
