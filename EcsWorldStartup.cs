using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    /// Old entry point for ecs world
    /// Processing Init, Run and RunPhysics
    /// Create ecs world and global modules
    /// Remove after 01/01/2022
    /// </summary>
    [Obsolete]
    public class EcsWorldStartup : MonoBehaviour
    {
        private static readonly Lazy<EcsWorld> LazyWorld = new Lazy<EcsWorld>();
        public static readonly EcsWorld world = LazyWorld.Value;
        private bool _isInitialize;
        private EcsSystems _systems;
        private EcsEventTable _eventTable;
        private EcsModuleSystem _moduleSystem;
        private IEnumerable<EcsModule> _modules;

        private async void Awake()
        {
            _eventTable = new EcsEventTable();
            _moduleSystem = new EcsModuleSystem(_eventTable);
            _systems = new EcsSystems(world);
            _systems.Add(_moduleSystem);
            _modules = CreateGlobalModules().ToArray();

            foreach (var type in _modules)
                await type.Activate(world, _eventTable);

            _systems.Init();
            _isInitialize = true;
        }

        private void Update()
        {
            if (!_isInitialize)
                return;
            _systems.Run();

            foreach (var module in _modules)
            {
                module.Run();
            }
            EcsWorldEventsBlackboard.Update();
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
            world.Destroy();
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