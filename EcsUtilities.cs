using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Static utilities
    /// </summary>
    internal static class EcsUtilities
    {
        /// <summary>
        /// Create systems for module
        /// </summary>
        /// <param name="moduleType">Type of module</param>
        /// <returns>Enumerable of created systems</returns>
        /// <seealso cref="EcsModule.Activate"/>
        internal static IEnumerable<IEcsSystem> CreateSystems(Type moduleType)
        {
            return GetSystemTypes(moduleType).Select(t => (IEcsSystem) Activator.CreateInstance(t));
        }

        private static IEnumerable<Type> GetSystemTypes(Type moduleType)
        {
            return
                from type in Assembly.GetExecutingAssembly().GetTypes()
                let attr = type.GetCustomAttribute<EcsSystemAttribute>()
                where attr != null && attr.module == moduleType
                select type;
        }
    }
}