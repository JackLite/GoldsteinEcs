using System;
using System.Collections.Generic;

namespace EcsCore
{
    public class EcsEventTable
    {
        private HashSet<Type> eventsTypes = new HashSet<Type>();
        private LinkedList<EcsEvent> events = new LinkedList<EcsEvent>();
        private int count;

        public void AddEvent<T>() where T : struct
        {
            AddEvent(EcsEvent.CreateEvent<T>());
        }

        public void AddEvent(EcsEvent ecsEvent)
        {
            events.AddLast(ecsEvent);
        }

        [Obsolete]
        public bool IsEventExist<T>() where T : struct
        {
            return eventsTypes.Contains(typeof(T));
        }

        public bool Has<T>() where T : struct
        {
            return eventsTypes.Contains(typeof(T));
        }

        public void Update()
        {
            var node = events.First;
            eventsTypes.Clear();
            while (node != null)
            {
                var e = node.Value;
                if (e.IsReady)
                    events.Remove(node);
                else
                {
                    e.IsReady = true;
                    if (!eventsTypes.Contains(e.Type))
                        eventsTypes.Add(e.Type);
                }
                node = node.Next;
            }
        }
    }
}