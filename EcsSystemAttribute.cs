using System;

namespace EcsCore
{
    /// <summary>
    /// Mark system for auto-creation in concrete module
    /// </summary>
    /// <seealso cref="EcsModule"/>
    public class EcsSystemAttribute : Attribute
    {
        /// <summary>
        /// Type of module
        /// </summary>
        public readonly Type Module;

        /// <param name="module">Type of module</param>
        /// <seealso cref="EcsModule"/>
        /// <seealso cref="EcsUtilities"/>
        public EcsSystemAttribute(Type module)
        {
            Module = module;
        }
    }
}