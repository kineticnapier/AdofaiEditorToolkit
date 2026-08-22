using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    /// <summary>LevelEventの生成・metadata・保存方法を隠蔽する。</summary>
    public interface IEventBackend
    {
        object Create(string eventName, int floor);
        void Add(object levelEvent, EventCollection collection);
        IEnumerable<object> Enumerate(EventCollection collection);
        EventCollection GetCollection(object levelEvent);
        string GetName(object levelEvent);
        int GetFloor(object levelEvent);

        bool HasProperty(object levelEvent, string key);
        Type GetPropertyType(object levelEvent, string key);
        object GetProperty(object levelEvent, string key);
        void SetProperty(object levelEvent, string key, object value);
        bool IsPropertyDisabled(object levelEvent, string key);
        void SetPropertyDisabled(object levelEvent, string key, bool disabled);

        bool Remove(object levelEvent);
    }
}
