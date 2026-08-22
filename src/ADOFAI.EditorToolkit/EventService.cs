using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    public sealed class EventService
    {
        internal EventService(IEventBackend backend)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        internal IEventBackend Backend { get; }

        /// <summary>イベントを生成し、現在のLevelDataへ直ちに追加する。</summary>
        public EventHandle Create(string eventName, int floor, EventCollection collection = EventCollection.Auto)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException("An event name is required.", nameof(eventName));
            if (floor < 0)
                throw new ArgumentOutOfRangeException(nameof(floor));
            if (collection == EventCollection.All)
                throw new ArgumentException("A new event must use Auto, Actions, or Decorations, not All.", nameof(collection));

            object raw;
            try
            {
                raw = Backend.Create(eventName, floor);
                Backend.Add(raw, collection);
            }
            catch (Exception ex)
            {
                throw new EditorToolkitException(
                    "Failed to create event '" + eventName + "' at floor " + floor + ".", ex);
            }

            return new EventHandle(this, raw);
        }

        public IReadOnlyList<EventHandle> Query(string eventName = null, int? floor = null)
        {
            return Query(new EventQuery { Name = eventName, Floor = floor });
        }

        public IReadOnlyList<EventHandle> Query(EventQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (query.Collection == EventCollection.Auto)
                throw new ArgumentException("Auto is only valid when creating an event. Use All, Actions, or Decorations for queries.", nameof(query));
            var result = new List<EventHandle>();
            foreach (var raw in Backend.Enumerate(query.Collection))
            {
                var handle = new EventHandle(this, raw);
                if (query.Matches(handle)) result.Add(handle);
            }
            return result.AsReadOnly();
        }

        public int Remove(EventQuery query)
        {
            var matches = Query(query);
            var removed = 0;
            for (var i = matches.Count - 1; i >= 0; i--)
                if (matches[i].Remove()) removed++;
            return removed;
        }

        public int Remove(string eventName = null, int? floor = null)
        {
            return Remove(new EventQuery { Name = eventName, Floor = floor });
        }

        internal object Get(object raw, string key)
        {
            EnsureProperty(raw, key);
            return Backend.GetProperty(raw, key);
        }

        internal void Set(object raw, string key, object value)
        {
            EnsureProperty(raw, key);
            try
            {
                var targetType = Backend.GetPropertyType(raw, key);
                if (targetType == null)
                {
                    var currentValue = Backend.GetProperty(raw, key);
                    targetType = currentValue == null ? (value == null ? typeof(object) : value.GetType()) : currentValue.GetType();
                }

                var converted = EventValueConverter.Convert(value, targetType);
                Backend.SetProperty(raw, key, converted);
                Backend.SetPropertyDisabled(raw, key, false);
            }
            catch (Exception ex)
            {
                throw new EventPropertyException(
                    "Failed to set property '" + key + "' on event '" + Backend.GetName(raw) + "'.", ex);
            }
        }

        internal void EnsureProperty(object raw, string key)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A property key is required.", nameof(key));
            if (!Backend.HasProperty(raw, key))
                throw new EventPropertyException(
                    "Event '" + Backend.GetName(raw) + "' does not define property '" + key + "'.");
        }
    }
}
