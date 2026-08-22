using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    public sealed class EventService
    {
        private readonly object _level;
        private readonly ILevelScopedEventBackend _levelBackend;

        internal EventService(IEventBackend backend)
            : this(backend, null)
        {
        }

        private EventService(IEventBackend backend, object level)
        {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _level = level;
            _levelBackend = level == null ? null : backend as ILevelScopedEventBackend;

            if (level != null && _levelBackend == null)
                throw new NotSupportedException(
                    "This event backend does not support operations against detached LevelData instances.");
        }

        internal IEventBackend Backend { get; }

        /// <summary>
        /// Returns an event service scoped to the supplied LevelData-like object instead of the
        /// level currently mounted in the stock editor.
        /// </summary>
        public EventService ForLevel(object level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            return new EventService(Backend, level);
        }

        /// <summary>イベントを生成し、対象のLevelDataへ直ちに追加する。</summary>
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
                Add(raw, collection);
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
            foreach (var raw in Enumerate(query.Collection))
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

        internal EventCollection GetCollection(object raw)
        {
            return _level == null
                ? Backend.GetCollection(raw)
                : _levelBackend.GetCollectionFromLevel(_level, raw);
        }

        internal bool Remove(object raw)
        {
            return _level == null
                ? Backend.Remove(raw)
                : _levelBackend.RemoveFromLevel(_level, raw);
        }

        internal void EnsureProperty(object raw, string key)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A property key is required.", nameof(key));
            if (!Backend.HasProperty(raw, key))
                throw new EventPropertyException(
                    "Event '" + Backend.GetName(raw) + "' does not define property '" + key + "'.");
        }

        private void Add(object raw, EventCollection collection)
        {
            if (_level == null)
                Backend.Add(raw, collection);
            else
                _levelBackend.AddToLevel(_level, raw, collection);
        }

        private IEnumerable<object> Enumerate(EventCollection collection)
        {
            return _level == null
                ? Backend.Enumerate(collection)
                : _levelBackend.EnumerateFromLevel(_level, collection);
        }
    }
}
