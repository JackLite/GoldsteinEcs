using System;
using System.Threading.Tasks;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Базовый класс для всех точек создания ECS-систем
    /// </summary>
    public abstract class EcsModule
    {
        protected EcsSystems _systems;
        protected abstract Type Type { get; }

        private bool _isActive;

        /// <summary>
        /// Создаёт системы для данного модуля
        /// </summary>
        /// <param name="world"></param>
        public async Task Activate(EcsWorld world)
        {
            _systems = new EcsSystems(world);
            await Setup();
            foreach (var system in EcsUtilities.CreateSystems(Type))
            {
                _systems.Add(system);
                InsertDependencies(system);
            }
            _systems.Init();
            _isActive = true;
        }

        public bool IsActiveAndInitialized()
        {
            return _systems ! != null && _isActive;
        }

        public void RunPhysics()
        {
            _systems.RunPhysics();
        }
        
        public void Run()
        {
            _systems.Run();
        }
        
        /// <summary>
        /// Удаляет системы, относящиеся к данному модулю
        /// </summary>
        public virtual void Deactivate()
        {
            _systems.Destroy();
            _systems = null;
            _isActive = false;
        }

        public void Destroy()
        {
            Deactivate();
        }

        protected virtual async Task Setup()
        {
            await Task.CompletedTask;
        }

        protected virtual async void InsertDependencies(IEcsSystem system)
        {
            
        }
    }
}