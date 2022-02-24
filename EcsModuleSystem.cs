using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Internal system for controlling activation and deactivation of modules
    /// </summary>
    internal class EcsModuleSystem : IEcsRunSystem, IEcsRunPhysicSystem, IEcsRunLateSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsEventTable _eventTable;
        private EcsFilter<EcsModuleActivationSignal> _activationFilter;
        private EcsFilter<EcsModuleDeactivationSignal> _deactivationFilter;
        private readonly EcsModule[] _modules;

        internal EcsModuleSystem(EcsEventTable eventTable)
        {
            _eventTable = eventTable;
            _modules = GetAllEcsSetups().ToArray();
        }

        public void Run()
        {
            CheckActivationAndDeactivation();

            foreach (var module in _modules)
            {
                if (module.IsActiveAndInitialized())
                    module.Run();
            }
        }

        public void RunPhysics()
        {
            CheckActivationAndDeactivation();

            foreach (var module in _modules)
            {
                if (module.IsActiveAndInitialized())
                    module.RunPhysics();
            }
        }

        public void RunLate()
        {
            CheckActivationAndDeactivation();

            foreach (var module in _modules)
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
                var module = _modules.FirstOrDefault(m => m.GetType() == type);
                if (module != null && module.IsActiveAndInitialized())
                    module.Deactivate();
                _deactivationFilter.GetEntity(i).Destroy();
            }

            foreach (var i in _activationFilter)
            {
                var type = _activationFilter.Get1(i).ModuleType;
                var module = _modules.FirstOrDefault(m => m.GetType() == type);
                if (module != null && !module.IsActiveAndInitialized())
                    module.Activate(_world, _eventTable);

                _activationFilter.GetEntity(i).Destroy();
            }
        }

        private static IEnumerable<EcsModule> GetAllEcsSetups()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => t.IsSubclassOf(typeof(EcsModule)))
                           .Where(t => t.GetCustomAttribute<EcsGlobalModuleAttribute>() == null)
                           .Select(t => (EcsModule) Activator.CreateInstance(t));
        }
    }
}