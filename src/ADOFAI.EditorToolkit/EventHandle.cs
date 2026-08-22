using System;

namespace ADOFAI.EditorToolkit
{
    public sealed class EventHandle
    {
        private readonly EventService _owner;

        internal EventHandle(EventService owner, object raw)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Raw = raw ?? throw new ArgumentNullException(nameof(raw));
        }

        public object Raw { get; }
        public string Name { get { return _owner.Backend.GetName(Raw); } }
        public int Floor { get { return _owner.Backend.GetFloor(Raw); } }
        public EventCollection Collection { get { return _owner.Backend.GetCollection(Raw); } }

        public EventHandle Set(string key, object value)
        {
            _owner.Set(Raw, key, value);
            return this;
        }

        public bool TrySet(string key, object value, out Exception error)
        {
            try
            {
                Set(key, value);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
        }

        public object Get(string key)
        {
            return _owner.Get(Raw, key);
        }

        public bool IsDisabled(string key)
        {
            return _owner.Backend.IsPropertyDisabled(Raw, key);
        }

        public EventHandle SetDisabled(string key, bool disabled)
        {
            _owner.EnsureProperty(Raw, key);
            _owner.Backend.SetPropertyDisabled(Raw, key, disabled);
            return this;
        }

        public bool Remove()
        {
            return _owner.Backend.Remove(Raw);
        }
    }
}
