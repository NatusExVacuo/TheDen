// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 a.rudenko <creadth@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Kognise <felix.mattick@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vyacheslav Kovalevsky <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 tgrkzus <tgrkzus@gmail.com>
// SPDX-FileCopyrightText: 2023 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Cargo.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Examine;
using Content.Shared.Throwing;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server.Atmos.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasTankSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly ExplosionSystem _explosions = default!;
        [Dependency] private readonly InternalsSystem _internals = default!;
        [Dependency] private readonly SharedAudioSystem _audioSys = default!;
        [Dependency] private readonly SharedContainerSystem _containers = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;

        private const float TimerDelay = 0.5f;
        private float _timer = 0f;
        private const float MinimumSoundValvePressure = 10.0f;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GasTankComponent, ComponentShutdown>(OnGasShutdown);
            SubscribeLocalEvent<GasTankComponent, BeforeActivatableUIOpenEvent>(BeforeUiOpen);
            SubscribeLocalEvent<GasTankComponent, GetItemActionsEvent>(OnGetActions);
            SubscribeLocalEvent<GasTankComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<GasTankComponent, ToggleActionEvent>(OnActionToggle);
            SubscribeLocalEvent<GasTankComponent, EntParentChangedMessage>(OnParentChange);
            SubscribeLocalEvent<GasTankComponent, GasTankSetPressureMessage>(OnGasTankSetPressure);
            SubscribeLocalEvent<GasTankComponent, GasTankToggleInternalsMessage>(OnGasTankToggleInternals);
            SubscribeLocalEvent<GasTankComponent, GasAnalyzerScanEvent>(OnAnalyzed);
            SubscribeLocalEvent<GasTankComponent, PriceCalculationEvent>(OnGasTankPrice);
            SubscribeLocalEvent<GasTankComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        }

        private void OnGasShutdown(Entity<GasTankComponent> gasTank, ref ComponentShutdown args)
        {
            DisconnectFromInternals(gasTank);
        }

        private void OnGasTankToggleInternals(Entity<GasTankComponent> ent, ref GasTankToggleInternalsMessage args)
        {
            ToggleInternals(ent);
        }

        private void OnGasTankSetPressure(Entity<GasTankComponent> ent, ref GasTankSetPressureMessage args)
        {
            var pressure = Math.Clamp(args.Pressure, 0f, ent.Comp.MaxOutputPressure);

            ent.Comp.OutputPressure = pressure;

            UpdateUserInterface(ent, true);
        }

        public void UpdateUserInterface(Entity<GasTankComponent> ent, bool initialUpdate = false)
        {
            var (owner, component) = ent;
            _ui.SetUiState(owner, SharedGasTankUiKey.Key,
                new GasTankBoundUserInterfaceState
                {
                    TankPressure = component.Air?.Pressure ?? 0,
                    OutputPressure = initialUpdate ? component.OutputPressure : null,
                    InternalsConnected = component.IsConnected,
                    CanConnectInternals = CanConnectToInternals(component)
                });
        }

        private void BeforeUiOpen(Entity<GasTankComponent> ent, ref BeforeActivatableUIOpenEvent args)
        {
            // Only initial update includes output pressure information, to avoid overwriting client-input as the updates come in.
            UpdateUserInterface(ent, true);
        }

        private void OnParentChange(EntityUid uid, GasTankComponent component, ref EntParentChangedMessage args)
        {
            // When an item is moved from hands -> pockets, the container removal briefly dumps the item on the floor.
            // So this is a shitty fix, where the parent check is just delayed. But this really needs to get fixed
            // properly at some point.
            component.CheckUser = true;
        }

        private void OnGetActions(EntityUid uid, GasTankComponent component, GetItemActionsEvent args)
        {
            args.AddAction(ref component.ToggleActionEntity, component.ToggleAction);
        }

        private void OnExamined(EntityUid uid, GasTankComponent component, ExaminedEvent args)
        {
            using(args.PushGroup(nameof(GasTankComponent)));
            if (args.IsInDetailsRange)
                args.PushMarkup(Loc.GetString("comp-gas-tank-examine", ("pressure", Math.Round(component.Air?.Pressure ?? 0))));
            if (component.IsConnected)
                args.PushMarkup(Loc.GetString("comp-gas-tank-connected"));
            args.PushMarkup(Loc.GetString(component.IsValveOpen ? "comp-gas-tank-examine-open-valve" : "comp-gas-tank-examine-closed-valve"));
        }

        private void OnActionToggle(Entity<GasTankComponent> gasTank, ref ToggleActionEvent args)
        {
            if (args.Handled)
                return;

            ToggleInternals(gasTank);
            args.Handled = true;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            _timer += frameTime;

            if (_timer < TimerDelay)
                return;

            _timer -= TimerDelay;

            var query = EntityQueryEnumerator<GasTankComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                var gasTank = (uid, comp);
                if (comp.IsValveOpen && !comp.IsLowPressure && comp.OutputPressure > 0)
                {
                    ReleaseGas(gasTank);
                }

                if (comp.CheckUser)
                {
                    comp.CheckUser = false;
                    if (Transform(uid).ParentUid != comp.User)
                    {
                        DisconnectFromInternals(gasTank);
                        continue;
                    }
                }

                if (comp.Air != null)
                {
                    _atmosphereSystem.React(comp.Air, comp);
                }
                CheckStatus(gasTank);
                if (_ui.IsUiOpen(uid, SharedGasTankUiKey.Key))
                {
                    UpdateUserInterface(gasTank);
                }
            }
        }

        private void ReleaseGas(Entity<GasTankComponent> gasTank)
        {
            var removed = RemoveAirVolume(gasTank, gasTank.Comp.ValveOutputRate * TimerDelay);
            var environment = _atmosphereSystem.GetContainingMixture(gasTank.Owner, false, true);
            if (environment != null)
            {
                _atmosphereSystem.Merge(environment, removed);
            }
            var strength = removed.TotalMoles * MathF.Sqrt(removed.Temperature);
            var dir = _random.NextAngle().ToWorldVec();
            _throwing.TryThrow(gasTank, dir * strength, strength);
            if (gasTank.Comp.OutputPressure >= MinimumSoundValvePressure)
                _audioSys.PlayPvs(gasTank.Comp.RuptureSound, gasTank);
        }

        private void ToggleInternals(Entity<GasTankComponent> ent)
        {
            if (ent.Comp.IsConnected)
            {
                DisconnectFromInternals(ent);
            }
            else
            {
                ConnectToInternals(ent);
            }
        }

        public GasMixture? RemoveAir(Entity<GasTankComponent> gasTank, float amount)
        {
            var gas = gasTank.Comp.Air?.Remove(amount);
            CheckStatus(gasTank);
            return gas;
        }

        public GasMixture RemoveAirVolume(Entity<GasTankComponent> gasTank, float volume)
        {
            var component = gasTank.Comp;
            if (component.Air == null)
                return new GasMixture(volume);

            var molesNeeded = component.OutputPressure * volume / (Atmospherics.R * component.Air.Temperature);

            var air = RemoveAir(gasTank, molesNeeded);

            if (air != null)
                air.Volume = volume;
            else
                return new GasMixture(volume);

            return air;
        }

        public bool CanConnectToInternals(GasTankComponent component)
        {
            var internals = GetInternalsComponent(component, component.User);
            return internals != null && internals.BreathTools.Count != 0 && !component.IsValveOpen;
        }

        public void ConnectToInternals(Entity<GasTankComponent> ent)
        {
            var (owner, component) = ent;
            if (component.IsConnected || !CanConnectToInternals(component))
                return;

            var internals = GetInternalsComponent(component);
            if (internals == null)
                return;

            if (_internals.TryConnectTank((internals.Owner, internals), owner))
                component.User = internals.Owner;

            _actions.SetToggled(component.ToggleActionEntity, component.IsConnected);

            // Couldn't toggle!
            if (!component.IsConnected)
                return;

            component.ConnectStream = _audioSys.Stop(component.ConnectStream);
            component.ConnectStream = _audioSys.PlayPvs(component.ConnectSound, component.Owner)?.Entity;

            UpdateUserInterface(ent);
        }

        public void DisconnectFromInternals(Entity<GasTankComponent> ent)
        {
            var (owner, component) = ent;
            if (component.User == null)
                return;

            var internals = GetInternalsComponent(component);
            component.User = null;

            _actions.SetToggled(component.ToggleActionEntity, false);

            _internals.DisconnectTank(internals);
            component.DisconnectStream = _audioSys.Stop(component.DisconnectStream);
            component.DisconnectStream = _audioSys.PlayPvs(component.DisconnectSound, component.Owner)?.Entity;

            UpdateUserInterface(ent);
        }

        private InternalsComponent? GetInternalsComponent(GasTankComponent component, EntityUid? owner = null)
        {
            owner ??= component.User;
            if (Deleted(component.Owner))return null;
            if (owner != null) return CompOrNull<InternalsComponent>(owner.Value);
            return _containers.TryGetContainingContainer(component.Owner, out var container)
                ? CompOrNull<InternalsComponent>(container.Owner)
                : null;
        }

        public void AssumeAir(Entity<GasTankComponent> ent, GasMixture giver)
        {
            _atmosphereSystem.Merge(ent.Comp.Air, giver);
            CheckStatus(ent);
        }

        public void CheckStatus(Entity<GasTankComponent> ent)
        {
            var (owner, component) = ent;
            if (component.Air == null)
                return;

            var pressure = component.Air.Pressure;

            if (pressure > component.TankFragmentPressure)
            {
                // Give the gas a chance to build up more pressure.
                for (var i = 0; i < 3; i++)
                {
                    _atmosphereSystem.React(component.Air, component);
                }

                pressure = component.Air.Pressure;
                var range = MathF.Sqrt((pressure - component.TankFragmentPressure) / component.TankFragmentScale);

                // Let's cap the explosion, yeah?
                // !1984
                if (range > GasTankComponent.MaxExplosionRange)
                {
                    range = GasTankComponent.MaxExplosionRange;
                }

                _explosions.TriggerExplosive(owner, radius: range);

                return;
            }

            if (pressure > component.TankRupturePressure)
            {
                if (component.Integrity <= 0)
                {
                    var environment = _atmosphereSystem.GetContainingMixture(owner, false, true);
                    if(environment != null)
                        _atmosphereSystem.Merge(environment, component.Air);

                    _audioSys.PlayPvs(component.RuptureSound, Transform(component.Owner).Coordinates, AudioParams.Default.WithVariation(0.125f));

                    QueueDel(owner);
                    return;
                }

                component.Integrity--;
                return;
            }

            if (pressure > component.TankLeakPressure)
            {
                if (component.Integrity <= 0)
                {
                    var environment = _atmosphereSystem.GetContainingMixture(owner, false, true);
                    if (environment == null)
                        return;

                    var leakedGas = component.Air.RemoveRatio(0.25f);
                    _atmosphereSystem.Merge(environment, leakedGas);
                }
                else
                {
                    component.Integrity--;
                }

                return;
            }

            if (component.Integrity < 3)
                component.Integrity++;
        }

        /// <summary>
        /// Returns the gas mixture for the gas analyzer
        /// </summary>
        private void OnAnalyzed(EntityUid uid, GasTankComponent component, GasAnalyzerScanEvent args)
        {
            args.GasMixtures ??= new List<(string, GasMixture?)>();
            args.GasMixtures.Add((Name(uid), component.Air));
        }

        private void OnGasTankPrice(EntityUid uid, GasTankComponent component, ref PriceCalculationEvent args)
        {
            args.Price += _atmosphereSystem.GetPrice(component.Air);
        }

        private void OnGetAlternativeVerb(EntityUid uid, GasTankComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract || args.Hands == null)
                return;
            args.Verbs.Add(new AlternativeVerb()
            {
                Text = component.IsValveOpen ? Loc.GetString("comp-gas-tank-close-valve") : Loc.GetString("comp-gas-tank-open-valve"),
                Act = () =>
                {
                    component.IsValveOpen = !component.IsValveOpen;
                    _audioSys.PlayPvs(component.ValveSound, uid);
                },
                Disabled = component.IsConnected,
            });
        }
    }
}
