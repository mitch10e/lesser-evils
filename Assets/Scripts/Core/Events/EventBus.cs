using System;
using System.Collections.Generic;

namespace Core.Game.Events {

    public static class EventBus {

        private static Dictionary<Type, List<Delegate>> subscribers = new();

        public static void subscribe<T>(Action<T> handler) {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType)) {
                subscribers[eventType] = new();
            }

            subscribers[eventType].Add(handler);
        }

        public static void unsubscribe<T>(Action<T> handler) {
            Type eventType = typeof(T);

            if (subscribers.ContainsKey(eventType)) {
                subscribers[eventType].Remove(handler);
            }
        }

        public static void publish<T>(T eventData) {
            Type eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType)) return;

            var handlers = new List<Delegate>(subscribers[eventType]);
            foreach (var handler in handlers) {
                ((Action<T>)handler)?.Invoke(eventData);
            }
        }

        public static void clear() {
            subscribers.Clear();
        }

    }

}
