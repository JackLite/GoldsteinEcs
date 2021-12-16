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

        [RuntimeInitializeOnLoadMethod]
        private static void Startup()
        {
            if (Object.FindObjectOfType<EcsWorldStartup>())
                return;
            instance = new EcsWorldContainer();
            var ecsMono = new GameObject("EcsWorld").AddComponent<EcsWorldMono>();
            ecsMono.OnUpdate = instance.Update;
            ecsMono.OnFixedUpdate = instance.FixedUpdate;
            ecsMono.OnDestroyed = instance.OnDestroy;
            instance.StartWorld();
        }
        private async void StartWorld()
        {
            _moduleSystem = new EcsModuleSystem();
            _systems = new EcsSystems(World);
            _systems.Add(_moduleSystem);
            _modules = CreateGlobalModules().ToArray();

            foreach (var type in _modules)
                await type.Activate(World);

            var method = typeof(EcsSystems).GetMethod("OneFrame");
            foreach (var oneFrameType in EcsUtilities.GetOneFrameTypes())
                method.MakeGenericMethod(oneFrameType).Invoke(_systems, null);
            
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
            
            _systems.RunOneFrameRemove();
            for (var i = 0; i < _modules.Length; ++i)
            {
                _modules[i].RunOneFrameRemove();
            }
        }

        private void FixedUpdate()
        {
            if (!_isInitialize)
                return;
            _systems.RunPhysics();
            foreach (var module in _modules)
            {
                module.RunPhysics();
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
                           .Select(t => (EcsModule)Activator.CreateInstance(t));
        }
    }
}