using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    public sealed class DelegateEventBackend : IEventBackend
    {
        private readonly Func<string, int, object> _create;
        private readonly Action<object, EventCollection> _add;
        private readonly Func<EventCollection, IEnumerable<object>> _enumerate;
        private readonly Func<object, EventCollection> _getCollection;
        private readonly Func<object, string> _getName;
        private readonly Func<object, int> _getFloor;
        private readonly Func<object, string, bool> _hasProperty;
        private readonly Func<object, string, Type> _getPropertyType;
        private readonly Func<object, string, object> _getProperty;
        private readonly Action<object, string, object> _setProperty;
        private readonly Func<object, string, bool> _isPropertyDisabled;
        private readonly Action<object, string, bool> _setPropertyDisabled;
        private readonly Func<object, bool> _remove;

        public DelegateEventBackend(
            Func<string, int, object> create,
            Action<object, EventCollection> add,
            Func<EventCollection, IEnumerable<object>> enumerate,
            Func<object, EventCollection> getCollection,
            Func<object, string> getName,
            Func<object, int> getFloor,
            Func<object, string, bool> hasProperty,
            Func<object, string, Type> getPropertyType,
            Func<object, string, object> getProperty,
            Action<object, string, object> setProperty,
            Func<object, string, bool> isPropertyDisabled,
            Action<object, string, bool> setPropertyDisabled,
            Func<object, bool> remove)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _add = add ?? throw new ArgumentNullException(nameof(add));
            _enumerate = enumerate ?? throw new ArgumentNullException(nameof(enumerate));
            _getCollection = getCollection ?? throw new ArgumentNullException(nameof(getCollection));
            _getName = getName ?? throw new ArgumentNullException(nameof(getName));
            _getFloor = getFloor ?? throw new ArgumentNullException(nameof(getFloor));
            _hasProperty = hasProperty ?? throw new ArgumentNullException(nameof(hasProperty));
            _getPropertyType = getPropertyType ?? throw new ArgumentNullException(nameof(getPropertyType));
            _getProperty = getProperty ?? throw new ArgumentNullException(nameof(getProperty));
            _setProperty = setProperty ?? throw new ArgumentNullException(nameof(setProperty));
            _isPropertyDisabled = isPropertyDisabled ?? throw new ArgumentNullException(nameof(isPropertyDisabled));
            _setPropertyDisabled = setPropertyDisabled ?? throw new ArgumentNullException(nameof(setPropertyDisabled));
            _remove = remove ?? throw new ArgumentNullException(nameof(remove));
        }

        public object Create(string eventName, int floor) { return _create(eventName, floor); }
        public void Add(object levelEvent, EventCollection collection) { _add(levelEvent, collection); }
        public IEnumerable<object> Enumerate(EventCollection collection) { return _enumerate(collection); }
        public EventCollection GetCollection(object levelEvent) { return _getCollection(levelEvent); }
        public string GetName(object levelEvent) { return _getName(levelEvent); }
        public int GetFloor(object levelEvent) { return _getFloor(levelEvent); }
        public bool HasProperty(object levelEvent, string key) { return _hasProperty(levelEvent, key); }
        public Type GetPropertyType(object levelEvent, string key) { return _getPropertyType(levelEvent, key); }
        public object GetProperty(object levelEvent, string key) { return _getProperty(levelEvent, key); }
        public void SetProperty(object levelEvent, string key, object value) { _setProperty(levelEvent, key, value); }
        public bool IsPropertyDisabled(object levelEvent, string key) { return _isPropertyDisabled(levelEvent, key); }
        public void SetPropertyDisabled(object levelEvent, string key, bool disabled) { _setPropertyDisabled(levelEvent, key, disabled); }
        public bool Remove(object levelEvent) { return _remove(levelEvent); }
    }
}
