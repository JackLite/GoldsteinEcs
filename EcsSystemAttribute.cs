using System;

namespace EcsCore
{
    /// <summary>
    /// Атрибут, которым помечаются все классы, являющиеся ECS-системами
    /// </summary>
    /// <seealso cref="EcsModule"/>
    /// <seealso cref="EcsUtilities"/>
    public class EcsSystemAttribute : Attribute
    {
        /// <summary>
        /// Тип точки создания системы
        /// </summary>
        public readonly Type Setup;

        /// <param name="setup">Тип точки создания системы</param>
        /// <seealso cref="EcsModule"/>
        /// <seealso cref="EcsUtilities"/>
        public EcsSystemAttribute(Type setup)
        {
            Setup = setup;
        }
    }
}