using System;

namespace EcsCore
{
    public class EcsEvent
    {
        public readonly Type Type;
        public bool IsReady;

        public EcsEvent(Type type)
        {
            Type = type;
        }

        public static EcsEvent CreateEvent<T>() where T : struct
        {
            return new EcsEvent(typeof(T));
        }
    }
}