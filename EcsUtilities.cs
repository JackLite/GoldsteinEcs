using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.Ecs;

namespace EcsCore
{
    /// <summary>
    /// Набор статических утилит для ECS
    /// </summary>
    public static class EcsUtilities
    {
        /// <summary>
        /// Создаёт системы относящиеся к указанному типу точки создания
        /// </summary>
        /// <param name="setupType">Тип точки создания</param>
        /// <returns>Перечисление созданных систем</returns>
        /// <seealso cref="EcsModule.Activate"/>
        public static IEnumerable<IEcsSystem> CreateSystems(Type setupType)
        {
            return
                from type in Assembly.GetExecutingAssembly().GetTypes()
                let attr = type.GetCustomAttribute<EcsSystemAttribute>()
                where attr != null && attr.Setup == setupType
                select (IEcsSystem)Activator.CreateInstance(type);
        }
    }
}