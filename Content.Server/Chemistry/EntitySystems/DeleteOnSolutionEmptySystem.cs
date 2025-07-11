// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <temporaloroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chemistry.Components.DeleteOnSolutionEmptyComponent;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server.Chemistry.EntitySystems.DeleteOnSolutionEmptySystem
{
    public sealed class DeleteOnSolutionEmptySystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DeleteOnSolutionEmptyComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DeleteOnSolutionEmptyComponent, SolutionContainerChangedEvent>(OnSolutionChange);
        }

        public void OnStartup(Entity<DeleteOnSolutionEmptyComponent> entity, ref ComponentStartup args)
        {
            CheckSolutions(entity);
        }

        public void OnSolutionChange(Entity<DeleteOnSolutionEmptyComponent> entity, ref SolutionContainerChangedEvent args)
        {
            CheckSolutions(entity);
        }

        public void CheckSolutions(Entity<DeleteOnSolutionEmptyComponent> entity)
        {
            if (!TryComp(entity, out SolutionContainerManagerComponent? solutions))
                return;

            if (_solutionContainerSystem.TryGetSolution((entity.Owner, solutions), entity.Comp.Solution, out _, out var solution))
                if (solution.Volume <= 0)
                    EntityManager.QueueDeleteEntity(entity);
        }
    }
}
