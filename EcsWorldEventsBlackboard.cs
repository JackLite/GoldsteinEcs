using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    /// Event's blackboard for simple events from UI or for UI
    /// Event remain while someone handle it
    /// Important: Use this way only for global events or something that can not be simple handle by some system
    /// </summary>
    public static class EcsWorldEventsBlackboard
    {
        private const int MAX_EVENT_FRAMES_LIFETIME = 600;
        private static readonly List<object> _readyEvents = new List<object>();
        private static readonly List<object> _buffer = new List<object>();
        private static readonly List<int> _eventsForDelete = new List<int>();

        private static readonly Dictionary<Type, List<HandlerWrapper>> _handlers =
            new Dictionary<Type, List<HandlerWrapper>>();

        private static readonly Dictionary<Type, int> _eventFrameTime = new Dictionary<Type, int>(0);

        /// <summary>
        /// Add handler for event
        /// Don't forget remove handler when it destroyed
        /// </summary>
        /// <param name="handler">Event's handler</param>
        /// <typeparam name="T">Event's type</typeparam>
        /// <seealso cref="RemoveEventHandler{T}"/>
        public static void AddEventHandler<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers.Add(type, new List<HandlerWrapper>());
            _handlers[type].Add(new HandlerWrapper<T>(handler));
        }

        /// <summary>
        /// Remove handler for event
        /// </summary>
        /// <param name="handler">Event' handler</param>
        /// <typeparam name="T">Event's type</typeparam>
        /// <seealso cref="AddEventHandler{T}"/>
        public static void RemoveEventHandler<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);

            if (!_handlers.ContainsKey(type))
                return;

            var handlerWrapper = _handlers[type]
                                 .OfType<HandlerWrapper<T>>()
                                 .FirstOrDefault(hw => hw.Handler == handler);

            if (handlerWrapper == null)
                return;

            _handlers[type].Remove(handlerWrapper);
            if (_handlers[type].Count == 0)
                _handlers.Remove(type);
        }

        /// <summary>
        /// Add event at blackboard
        /// If event exists it will be overridden
        /// </summary>
        /// <param name="newEvent">Event</param>
        /// <typeparam name="T">Event's type</typeparam>
        public static void AddEvent<T>(T newEvent) where T : class
        {
            _buffer.Add(newEvent);
        }

        /// <summary>
        /// Check if event exists
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        public static bool IsEventExist<T>() where T : class
        {
            return _readyEvents.Any(e => e is T);
        }

        /// <summary>
        /// Processing events
        /// Only for internal usage
        /// </summary>
        internal static void Update()
        {
            ReadBuffer();
            HandleEvents();

            if (Debug.isDebugBuild)
                CheckEventsLifetime();
        }

        private static void ReadBuffer()
        {
            foreach (var e in _buffer)
            {
                _readyEvents.Add(e);
            }

            _buffer.Clear();
        }

        private static void HandleEvents()
        {
            for(var i = 0; i < _readyEvents.Count; i++)
            {
                var e = _readyEvents[i];
                if (!_handlers.ContainsKey(e.GetType()))
                    continue;
                foreach (var h in _handlers[e.GetType()])
                    h?.HandleEvent(e);
                _eventsForDelete.Add(i);
            }

            foreach (var index in _eventsForDelete)
            {
                _readyEvents.Remove(_readyEvents[index]);
            }
            _eventsForDelete.Clear();
        }

        private static void CheckEventsLifetime()
        {
            foreach (var e in _readyEvents)
            {
                var type = e.GetType();
                if (!_eventFrameTime.ContainsKey(type))
                    _eventFrameTime.Add(type, 0);
                _eventFrameTime[type]++;

                if (_eventFrameTime[type] > MAX_EVENT_FRAMES_LIFETIME)
                {
                    Debug.LogError($"Event with type {type} was not handle");
                }
            }
        }

        private class HandlerWrapper
        {
            public Type Type;

            public virtual void HandleEvent(object e)
            {
                throw new NotImplementedException();
            }
        }

        private class HandlerWrapper<T> : HandlerWrapper where T : class
        {
            public readonly Action<T> Handler;

            public HandlerWrapper(Action<T> handler)
            {
                Handler = handler;
            }

            public override void HandleEvent(object e)
            {
                Handler?.Invoke(e as T);
            }
        }
    }
}