using System;
using System.Collections.Generic;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Internal system for controlling activation and deactivation of modules
    /// </summary>
    internal class EcsModuleSystem : IEcsRunSystem, IEcsRunPhysicSystem, IEcsRunLateSystem, IEcsDestroySystem
    {
        private readonly EcsWorld _world;
        private readonly EcsEventTable _eventTable;
        private EcsFilter<EcsModuleActivationSignal> _activationFilter;
        private EcsFilter<EcsModuleDeactivationSignal> _deactivationFilter;
        private readonly IReadOnlyDictionary<Type, EcsModule> _modules;

        internal EcsModuleSystem(EcsEventTable eventTable, EcsModulesRepository modulesRepository)
        {
            _eventTable = eventTable;
            _modules = modulesRepository.LocalModules;
        }

        public void Run()
        {
            CheckActivationAndDeactivation();

            foreach (var (_, module) in _modules)
            {
                if (module.IsActiveAndInitialized())
                    module.Run();
            }
        }

        public void RunPhysics()
        {
            CheckActivationAndDeactivation();

            foreach (var (_, module) in _modules)
            {
                if (module.IsActiveAndInitialized())
                    module.RunPhysics();
            }
        }

        public void RunLate()
        {
            CheckActivationAndDeactivation();

            foreach (var (_, module) in _modules)
            {
                if (module.IsActiveAndInitialized())
                    module.RunLate();
            }
        }

        private void CheckActivationAndDeactivation()
        {
            foreach (var i in _deactivationFilter)
            {
                var type = _deactivationFilter.Get1(i).ModuleType;
                _modules.TryGetValue(type, out var module);
                if (module != null && module.IsActiveAndInitialized())
                    module.Deactivate();
                _deactivationFilter.GetEntity(i).Destroy();
            }

            foreach (var i in _activationFilter)
            {
                var activationSignal = _activationFilter.Get1(i);
                _modules.TryGetValue(activationSignal.moduleType, out var module);
                if (module != null && !module.IsActiveAndInitialized())
                {
                    EcsModule parent = null;
                    if (activationSignal.dependenciesModule != null)
                        _modules.TryGetValue(activationSignal.dependenciesModule, out parent);
                    module.Activate(_world, _eventTable, parent).Forget();
                }

                _activationFilter.GetEntity(i).Destroy();
            }
        }

        public void Destroy()
        {
            foreach (var module in _modules)
            {
                module.Value.Destroy();
            }
        }
    }
}