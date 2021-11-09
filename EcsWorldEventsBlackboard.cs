using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EcsCore
{
    /// <summary>
    /// Доска событий, на которую прилетают события из ECS-мира
    /// В основном используется для UI
    /// Событие хранится, пока не будет обработано
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
        /// Добавляет обработчик указанного события
        /// Стоит не забывать отписываться от события, если объект уничтожается
        /// </summary>
        /// <param name="handler">Обработчик события</param>
        /// <typeparam name="T">Тип события</typeparam>
        /// <seealso cref="RemoveEventHandler{T}"/>
        public static void AddEventHandler<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers.Add(type, new List<HandlerWrapper>());
            _handlers[type].Add(new HandlerWrapper<T>(handler));
        }

        /// <summary>
        /// Удаляет обработчик события
        /// Используется для отписки от события
        /// </summary>
        /// <param name="handler">Обработчик события, который ранее был добавлен</param>
        /// <typeparam name="T">Тип события</typeparam>
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
        /// Добавляет событие на доску
        /// Если доска уже содержит данное событие, то оно будет перезаписано
        /// </summary>
        /// <param name="newEvent">Событие</param>
        /// <typeparam name="T">Тип события</typeparam>
        public static void AddEvent<T>(T newEvent) where T : class
        {
            _buffer.Add(newEvent);
        }

        /// <summary>
        /// Проверяет, существует ли на доске событие определённого типа
        /// </summary>
        /// <typeparam name="T">Тип события</typeparam>
        /// <returns>true/false в зависимости от наличия на доске соответствующего события</returns>
        public static bool IsEventExist<T>() where T : class
        {
            return _readyEvents.Any(e => e is T);
        }

        /// <summary>
        /// Проверка буффера событий и вызов события
        /// </summary>
        public static void Update()
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