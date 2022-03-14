using Leopotam.Ecs;

namespace EcsCore
{
    public static class Extensions
    {
        public static void ActivateModule<T>(this EcsWorld world) where T : EcsModule
        {
            world.NewEntity().Replace(new EcsModuleActivationSignal { ModuleType = typeof(T) });
        }
        
        public static void DeactivateModule<T>(this EcsWorld world) where T : EcsModule
        {
            world.NewEntity().Replace(new EcsModuleDeactivationSignal { ModuleType = typeof(T) });
        }

        public static EcsEntity CreateOneFrame(this EcsWorld world)
        {
            return world.NewEntity().Replace(new EcsOneFrame());
        }
    }
}