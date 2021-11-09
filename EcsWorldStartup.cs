using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    ///     Отвечает за запуск ECS-мира
    ///     Создаёт, обрабатывает и удаляет системы
    /// </summary>
    public class EcsWorldStartup : MonoBehaviour
    {
        private static readonly Lazy<EcsWorld> LazyWorld = new Lazy<EcsWorld>();
        public static readonly EcsWorld world = LazyWorld.Value;
        private bool _isInitialize;
        private EcsSystems _systems;
        private EcsModuleSystem _moduleSystem;

        private async void Awake()
        {
            _moduleSystem = new EcsModuleSystem();
            _systems = new EcsSystems(world);
            _systems.Add(_moduleSystem);
            var setups = GetGlobalModules();

            foreach (var type in setups)
                await type.Activate(world);

            _systems.Init();
            _isInitialize = true;
        }

        private void Update()
        {
            if (!_isInitialize)
                return;
            _systems.Run();
            EcsWorldEventsBlackboard.Update();
        }

        private void FixedUpdate()
        {
            if (!_isInitialize)
                return;
            _systems.RunPhysics();
        }

        private void OnDestroy()
        {
            if (!_isInitialize)
                return;
            _systems.Destroy();
            world.Destroy();
        }

        private static IEnumerable<EcsModule> GetGlobalModules()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => t.IsSubclassOf(typeof(EcsModule)))
                           .Where(t => t.GetCustomAttribute<EcsGlobalModuleAttribute>() != null)
                           .Select(t => (EcsModule)Activator.CreateInstance(t));
        }
    }
}