using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    /// <summary>
    /// Extends an event backend so EventService can target a detached/snapshotted LevelData
    /// instead of only the level currently mounted in the stock editor.
    /// </summary>
    public interface ILevelScopedEventBackend
    {
        void AddToLevel(object level, object levelEvent, EventCollection collection);
        IEnumerable<object> EnumerateFromLevel(object level, EventCollection collection);
        EventCollection GetCollectionFromLevel(object level, object levelEvent);
        bool RemoveFromLevel(object level, object levelEvent);
    }
}
