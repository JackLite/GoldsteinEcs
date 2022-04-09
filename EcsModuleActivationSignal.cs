using System;

namespace EcsCore
{
    /// <summary>
    /// Simple signal for activate module
    /// Example: world.NewEntity().Replace(new EcsModuleActivationSignal {Type = typeof(YourModule)});
    /// Also you can set dependenciesModule field if you want to get dependencies from another module
    /// </summary>
    public struct EcsModuleActivationSignal
    {
        public Type moduleType;
        public Type dependenciesModule;
    }
}