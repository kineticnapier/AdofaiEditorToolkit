using System;

namespace ADOFAI.EditorToolkit
{
    public sealed class EventQuery
    {
        public EventQuery()
        {
            Collection = EventCollection.All;
        }

        public string Name { get; set; }
        public int? Floor { get; set; }
        public EventCollection Collection { get; set; }
        public Func<EventHandle, bool> Predicate { get; set; }

        internal bool Matches(EventHandle handle)
        {
            if (Name != null && !string.Equals(Name, handle.Name, StringComparison.OrdinalIgnoreCase))
                return false;
            if (Floor.HasValue && Floor.Value != handle.Floor)
                return false;
            return Predicate == null || Predicate(handle);
        }
    }
}
