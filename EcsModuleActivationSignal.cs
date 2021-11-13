using System;

namespace EcsCore
{
    /// <summary>
    /// Simple signal for activate module
    /// Example: world.NewEntity().Replace(new EcsModuleActivationSignal {Type = typeof(YourModule)});
    /// </summary>
    public struct EcsModuleActivationSignal
    {
        public Type ModuleType;
    }
}