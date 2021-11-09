using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Отвечает за управление модулями, их активацию и деактивацию по сигналу
    /// </summary>
    public class EcsModuleSystem : IEcsRunSystem, IEcsPhysicRunSystem
    {
        private readonly EcsWorld _world;
        private EcsFilter<EcsModuleActivationSignal> _activationFilter;
        private EcsFilter<EcsModuleDeactivationSignal> _deactivationFilter;
        private readonly EcsModule[] _modules;
        public EcsModuleSystem()
        {
            _modules = GetAllEcsSetups().ToArray();
        }

        public void Run()
        {
            foreach (var i in _activationFilter)
            {
                var type = _activationFilter.Get1(i).ModuleType;
                var module = _modules.FirstOrDefault(m => m.GetType() == type);
                module?.Activate(_world);
                _activationFilter.GetEntity(i).Destroy();
            }

            foreach (var i in _deactivationFilter)
            {
                var type = _deactivationFilter.Get1(i).ModuleType;
                var module = _modules.FirstOrDefault(m => m.GetType() == type);
                module?.Deactivate();
                _deactivationFilter.GetEntity(i).Destroy();
            }
            
            foreach (var module in _modules)
            {
                if(module.IsActiveAndInitialized())
                    module.Run();
            }
        }

        public void RunPhysics()
        {
            foreach (var module in _modules)
            {
                if(module.IsActiveAndInitialized())
                    module.RunPhysics();
            }
        }

        private static IEnumerable<EcsModule> GetAllEcsSetups()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => t.IsSubclassOf(typeof(EcsModule)))
                           .Where(t => t.GetCustomAttribute<EcsGlobalModuleAttribute>() == null)
                           .Select(t => (EcsModule)Activator.CreateInstance(t));
        }
    }
}