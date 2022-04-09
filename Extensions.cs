using System.Threading.Tasks;
using Leopotam.Ecs;

namespace EcsCore
{
    public static class Extensions
    {
        /// <summary>
        /// Activate module: call Setup() and GetDependencies()
        /// </summary>
        /// <param name="world"></param>
        /// <param name="parent">You can set parent if you want inherited dependencies in activating module</param>
        /// <typeparam name="T">Type of module that you want to activate</typeparam>
        public static void ActivateModule<T>(this EcsWorld world, EcsModule parent = null) where T : EcsModule
        {
            world.NewEntity()
                 .Replace(new EcsModuleActivationSignal
                 {
                     moduleType = typeof(T),
                     dependenciesModule = parent?.GetType()
                 });
        }

        /// <summary>
        /// Deactivate module: calls Deactivate() in module
        /// </summary>
        /// <param name="world"></param>
        /// <typeparam name="T">Type of module that you want to deactivate</typeparam>
        public static void DeactivateModule<T>(this EcsWorld world) where T : EcsModule
        {
            world.NewEntity().Replace(new EcsModuleDeactivationSignal { ModuleType = typeof(T) });
        }

        /// <summary>
        /// Allow to create one frame entity. That entity will be destroyed after all run systems processed (include IEcsRunLate)
        /// WARNING: one frame creates immediately, but if some systems processed BEFORE creation one frame entity
        /// they WILL NOT processed that entity. You can create one frame in RunSystem and processed them in RunLateSystem.
        /// Also you can use GetSystemOrder() in your module for setting order of systems.
        /// </summary>
        /// <param name="world"></param>
        /// <returns>New entity</returns>
        /// <seealso cref="EcsModule.GetSystemsOrder"/>
        public static EcsEntity CreateOneFrame(this EcsWorld world)
        {
            return world.NewEntity().Replace(new EcsOneFrame());
        }

        public static void Forget(this Task task)
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task);
            }

            static async Task ForgetAwaited(Task task)
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}