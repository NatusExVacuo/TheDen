// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Drugs;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Drugs;

/// <summary>
///     System to handle drug related overlays.
/// </summary>
public sealed class DrugOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private RainbowOverlay _overlay = default!;

    public static string RainbowKey = "SeeingRainbows";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeeingRainbowsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SeeingRainbowsComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SeeingRainbowsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SeeingRainbowsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, SeeingRainbowsComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SeeingRainbowsComponent component, LocalPlayerDetachedEvent args)
    {
        _overlay.Intoxication = 0;
        _overlay.TimeTicker = 0;
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, SeeingRainbowsComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SeeingRainbowsComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
        {
            _overlay.Intoxication = 0;
            _overlay.TimeTicker = 0;
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
