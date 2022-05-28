using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;
using Leopotam.Ecs.UnityIntegration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EcsCore
{
    public class EcsWorldContainer
    {
        private static Lazy<EcsWorld> lazyWorld = new Lazy<EcsWorld>();
        private static EcsWorldContainer instance;
        public static EcsWorld World => lazyWorld.Value;
        private bool _isInitialize;
        private EcsSystems _systems;
        private EcsSystems _oneFrameSystems;
        private EcsModule[] _modules;
        private EcsEventTable _eventTable;
        private EcsModulesRepository _modulesRepository;

        [RuntimeInitializeOnLoadMethod]
        private static void Startup()
        {
            #if UNITY_EDITOR
                EcsWorldObserver.Create (World);
            #endif  
            instance = new EcsWorldContainer();
            var ecsMono = new GameObject("EcsWorld").AddComponent<EcsWorldMono>();
            ecsMono.onUpdate = instance.Update;
            ecsMono.onFixedUpdate = instance.FixedUpdate;
            ecsMono.onLateUpdate = instance.LateUpdate;
            ecsMono.onDestroyed = instance.OnDestroy;
            instance.StartWorld();
        }

        private async void StartWorld()
        {
            _eventTable = new EcsEventTable();
            _modulesRepository = new EcsModulesRepository();
            _systems = new EcsSystems(World);
            _systems.Add(new EcsModuleSystem(_eventTable, _modulesRepository));
            _oneFrameSystems = new EcsSystems(World);
            _oneFrameSystems.Add(new EcsOneFrameSystem());
            _modules = _modulesRepository.GlobalModules.Values.ToArray();

            foreach (var type in _modules)
                await type.Activate(World, _eventTable);

            _systems.Init();
            _oneFrameSystems.Init();
            _isInitialize = true;
        }

        private void Update()
        {
            if (!_isInitialize)
                return;
            _systems.Run();

            for (var i = 0; i < _modules.Length; ++i)
            {
                _modules[i].Run();
            }

            EcsWorldEventsBlackboard.Update();
            _eventTable.Update();
        }

        private void FixedUpdate()
        {
            if (!_isInitialize)
                return;
            _systems.RunPhysics();
            for (var i = 0; i < _modules.Length; ++i)
            {
                _modules[i].RunPhysics();
            }
        }

        private void LateUpdate()
        {
            if (!_isInitialize)
                return;
            _systems.RunLate();
            for (var i = 0; i < _modules.Length; ++i)
            {
                _modules[i].RunLate();
            }
            _oneFrameSystems.RunLate();
        }

        private void OnDestroy()
        {
            if (!_isInitialize)
                return;
            foreach (var module in _modules)
            {
                module.Destroy();
            }

            _systems.Destroy();
            World.Destroy();
            lazyWorld = new Lazy<EcsWorld>();
        }

        private static IEnumerable<EcsModule> CreateGlobalModules()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(EcsModule)))
                .Where(t => t.GetCustomAttribute<EcsGlobalModuleAttribute>() != null)
                .Select(t => (EcsModule) Activator.CreateInstance(t));
        }
    }
}