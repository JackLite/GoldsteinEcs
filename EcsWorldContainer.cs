using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;
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
        private EcsModuleSystem _moduleSystem;
        private EcsModule[] _modules;
        private EcsEventTable _eventTable;

        [RuntimeInitializeOnLoadMethod]
        private static void Startup()
        {
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
            _moduleSystem = new EcsModuleSystem(_eventTable);
            _systems = new EcsSystems(World);
            _systems.Add(_moduleSystem);
            _modules = CreateGlobalModules().ToArray();

            foreach (var type in _modules)
                await type.Activate(World, _eventTable);

            _systems.Init();
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