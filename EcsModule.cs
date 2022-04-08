using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EcsCore.DependencyInjection;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Base class for every module
    /// In modules you can create dependencies for your system and instantiate all prefabs that you need
    /// Don't create any entities in modules - use IEcsInitSystem instead
    /// </summary>
    /// <seealso cref="IEcsRunSystem"/>
    /// <seealso cref="EcsGlobalModuleAttribute"/>
    public abstract class EcsModule
    {
        private SortedDictionary<int, EcsSystems> _systems;
        private bool _isActive;
        private readonly bool _isGlobal;
        private static readonly Dictionary<Type, object> _globalDependencies = new Dictionary<Type, object>();
        private static Exception _exception;

        [Obsolete] protected virtual Type Type => GetType();

        private Type ConcreteType => GetType();

        protected EcsModule()
        {
            _isGlobal = ConcreteType.GetCustomAttribute<EcsGlobalModuleAttribute>() != null;
        }

        /// <summary>
        /// Activate concrete module: call and await EcsModule.Setup(), create all systems and insert dependencies
        /// </summary>
        /// <param name="world">The world where systems and entities will live</param>
        /// <param name="eventTable">The table for events</param>
        /// <seealso cref="Setup"/>
        public async Task Activate(EcsWorld world, EcsEventTable eventTable)
        {
            try
            {
                await Setup();

                UpdateGlobalDependencies();

                _systems = new SortedDictionary<int, EcsSystems>();
                var systemOrder = GetSystemsOrder();
                foreach (var system in EcsUtilities.CreateSystems(ConcreteType))
                {
                    var order = 0;
                    if (systemOrder != null && systemOrder.ContainsKey(system.GetType()))
                        order = systemOrder[system.GetType()];

                    if (!_systems.ContainsKey(order))
                    {
                        _systems[order] = new EcsSystems(world);
                        _systems[order].Inject(eventTable);
                    }

                    InsertDependencies(system);
                    _systems[order].Add(system);
                }

                foreach (var p in _systems)
                {
                    p.Value.Init();
                }

                _isActive = true;
            }
            catch (Exception e)
            {
                _exception = new Exception(e.Message, e);
            }
        }

        protected virtual Dictionary<Type, int> GetSystemsOrder()
        {
            return null;
        }

        /// <summary>
        /// Return true if systems was create and init
        /// </summary>
        /// <returns></returns>
        public bool IsActiveAndInitialized()
        {
            return _systems != null && _isActive;
        }

        /// <summary>
        /// Just call RunPhysics at systems
        /// </summary>
        internal void RunPhysics()
        {
            foreach (var p in _systems)
            {
                p.Value.RunPhysics();
            }
        }

        /// <summary>
        /// Just call Run at systems
        /// </summary>
        internal void Run()
        {
            CheckException();
            foreach (var p in _systems)
            {
                p.Value.Run();
            }
        }
        
        
        /// <summary>
        /// Just call RunLate at systems
        /// </summary>
        internal void RunLate()
        {
            foreach (var p in _systems)
            {
                p.Value.RunLate();
            }
        }


        /// <summary>
        /// Destroy systems in the module
        /// You can clear something at child, like release some resources
        /// </summary>
        public virtual void Deactivate()
        {
            foreach (var p in _systems)
            {
                p.Value.Destroy();
            }

            _systems = null;
            _isActive = false;
        }

        /// <summary>
        /// For internal usage only
        /// </summary>
        internal void Destroy()
        {
            Deactivate();
        }

        /// <summary>
        /// Call when module activate
        /// You can create here all dependencies and game objects, that you need
        /// </summary>
        protected virtual async Task Setup()
        {
            await Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckException()
        {
            if (_exception == null)
                return;
            throw _exception;
        }

        private void UpdateGlobalDependencies()
        {
            if (!_isGlobal)
                return;

            foreach (var kvp in GetDependencies())
            {
                if (_globalDependencies.ContainsKey(kvp.Key))
                    continue;
                _globalDependencies.Add(kvp.Key, kvp.Value);
            }
        }

        private void InsertDependencies(IEcsSystem system)
        {
            var dependencies = GetDependencies();
            var setupMethod = GetSetupMethod(system);
            if (setupMethod != null)
            {
                var parameters = setupMethod.GetParameters();
                var injections = new object[parameters.Length];
                var i = 0;
                foreach (var parameter in parameters)
                {
                    var t = parameter.ParameterType;
                    if (_globalDependencies.ContainsKey(t))
                    {
                        injections[i++] = _globalDependencies[t];
                        continue;
                    }

                    if (dependencies.ContainsKey(t))
                    {
                        injections[i++] = dependencies[t];
                        continue;
                    }

                    throw new Exception($"Can't find injection {parameter.ParameterType} in method {setupMethod.Name}");
                }

                setupMethod.Invoke(system, injections);
                return;
            }

            var fields = system.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var t = field.FieldType;
                if (_globalDependencies.ContainsKey(t))
                    field.SetValue(system, _globalDependencies[t]);
                if (dependencies.ContainsKey(t))
                    field.SetValue(system, dependencies[t]);
            }
        }

        private MethodInfo GetSetupMethod(IEcsSystem system)
        {
            var methods = system.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods)
            {
                if (methodInfo.GetCustomAttribute<SetupAttribute>() == null)
                    continue;
                return methodInfo;
            }

            return null;
        }

        /// <summary>
        /// Must return dictionary of dependencies for all systems in the module
        /// Dependencies in systems MUST BE private and non-static
        /// </summary>
        protected virtual Dictionary<Type, object> GetDependencies()
        {
            return new Dictionary<Type, object>(0);
        }
    }
}