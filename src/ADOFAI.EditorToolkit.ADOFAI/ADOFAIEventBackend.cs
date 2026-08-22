using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using global::ADOFAI;

namespace ADOFAI.EditorToolkit.Game
{
    public sealed class ADOFAIEventBackend : IEventBackend
    {
        private readonly Func<scnEditor> _getEditor;

        public ADOFAIEventBackend(Func<scnEditor> getEditor)
        {
            _getEditor = getEditor ?? throw new ArgumentNullException(nameof(getEditor));
        }

        public object Create(string eventName, int floor)
        {
            LevelEventInfo info = ResolveEventInfo(eventName);
            return new LevelEvent(floor, info.type, info);
        }

        public void Add(object levelEvent, EventCollection collection)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            if (collection == EventCollection.Auto)
                collection = ev.info != null && ev.info.isDecoration
                    ? EventCollection.Decorations
                    : EventCollection.Actions;
            IList list = GetList(collection);
            list.Add(ev);
        }

        public IEnumerable<object> Enumerate(EventCollection collection)
        {
            if (collection == EventCollection.Actions || collection == EventCollection.All)
            {
                IList actions = GetList(EventCollection.Actions);
                for (int i = 0; i < actions.Count; i++) yield return actions[i];
            }
            if (collection == EventCollection.Decorations || collection == EventCollection.All)
            {
                IList decorations = GetList(EventCollection.Decorations);
                for (int i = 0; i < decorations.Count; i++) yield return decorations[i];
            }
        }

        public EventCollection GetCollection(object levelEvent)
        {
            IList actions = GetList(EventCollection.Actions);
            for (int i = 0; i < actions.Count; i++)
                if (ReferenceEquals(actions[i], levelEvent)) return EventCollection.Actions;

            IList decorations = GetList(EventCollection.Decorations);
            for (int i = 0; i < decorations.Count; i++)
                if (ReferenceEquals(decorations[i], levelEvent)) return EventCollection.Decorations;

            throw new InvalidOperationException("The LevelEvent is not attached to the current LevelData.");
        }

        public string GetName(object levelEvent)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            if (ev.info != null && !string.IsNullOrWhiteSpace(ev.info.name)) return ev.info.name;
            return Convert.ToString(ev.eventType, CultureInfo.InvariantCulture) ?? "<unknown>";
        }

        public int GetFloor(object levelEvent)
        {
            return RequireEvent(levelEvent).floor;
        }

        public bool HasProperty(object levelEvent, string key)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            return ev.info != null
                && ev.info.propertiesInfo != null
                && ev.info.propertiesInfo.ContainsKey(key);
        }

        public Type GetPropertyType(object levelEvent, string key)
        {
            LevelEvent ev = RequireEvent(levelEvent);

            if (ev.info != null && ev.info.propertiesInfo != null)
            {
                global::ADOFAI.PropertyInfo propertyInfo;
                if (ev.info.propertiesInfo.TryGetValue(key, out propertyInfo)
                    && propertyInfo != null
                    && propertyInfo.value_default != null)
                    return propertyInfo.value_default.GetType();
            }

            object current = GetProperty(ev, key);
            return current == null ? null : current.GetType();
        }

        public object GetProperty(object levelEvent, string key)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            try
            {
                return ev[key];
            }
            catch (Exception ex)
            {
                throw new EventPropertyException("Could not read LevelEvent property '" + key + "'.", ex);
            }
        }

        public void SetProperty(object levelEvent, string key, object value)
        {
            RequireEvent(levelEvent)[key] = value;
        }

        public bool IsPropertyDisabled(object levelEvent, string key)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            return ev.disabled != null && ev.disabled.ContainsKey(key) && ev.disabled[key];
        }

        public void SetPropertyDisabled(object levelEvent, string key, bool disabled)
        {
            LevelEvent ev = RequireEvent(levelEvent);
            if (ev.disabled != null) ev.disabled[key] = disabled;
        }

        public bool Remove(object levelEvent)
        {
            IList actions = GetList(EventCollection.Actions);
            if (actions.Contains(levelEvent))
            {
                actions.Remove(levelEvent);
                return true;
            }

            IList decorations = GetList(EventCollection.Decorations);
            if (decorations.Contains(levelEvent))
            {
                decorations.Remove(levelEvent);
                return true;
            }
            return false;
        }

        private IList GetList(EventCollection collection)
        {
            if (collection == EventCollection.All || collection == EventCollection.Auto)
                throw new ArgumentException(collection + " is not a single LevelData list.", nameof(collection));

            scnEditor editor = _getEditor();
            if (editor == null || editor.levelData == null)
                throw new InvalidOperationException("ADOFAI stock editor has no active LevelData.");

            IList list = collection == EventCollection.Decorations
                ? editor.levelData.decorations as IList
                : editor.levelData.levelEvents as IList;
            if (list == null)
                throw new InvalidOperationException("The requested LevelData event collection is not IList-compatible.");
            return list;
        }

        private static LevelEvent RequireEvent(object value)
        {
            LevelEvent ev = value as LevelEvent;
            if (ev == null) throw new ArgumentException("The value is not an ADOFAI LevelEvent.", nameof(value));
            return ev;
        }

        private static LevelEventInfo ResolveEventInfo(string requestedName)
        {
            if (GCS.levelEventsInfo == null)
                throw new InvalidOperationException("ADOFAI level-event metadata is not initialized.");

            LevelEventInfo direct;
            if (GCS.levelEventsInfo.TryGetValue(requestedName, out direct) && direct != null)
                return direct;

            string target = NormalizeEventName(requestedName);
            var suffixMatches = new List<LevelEventInfo>();
            foreach (KeyValuePair<string, LevelEventInfo> pair in GCS.levelEventsInfo)
            {
                LevelEventInfo info = pair.Value;
                if (info == null) continue;

                string keyName = NormalizeEventName(pair.Key);
                string infoName = NormalizeEventName(info.name);
                if (keyName == target || infoName == target) return info;

                if (keyName.EndsWith(target, StringComparison.Ordinal)
                    || infoName.EndsWith(target, StringComparison.Ordinal))
                {
                    if (!suffixMatches.Contains(info)) suffixMatches.Add(info);
                }
            }

            if (suffixMatches.Count == 1) return suffixMatches[0];
            if (suffixMatches.Count > 1)
                throw new InvalidOperationException(
                    "Event name '" + requestedName + "' is ambiguous after Mod-prefix matching.");
            throw new InvalidOperationException(
                "No LevelEventInfo named '" + requestedName + "' is registered in this ADOFAI session.");
        }

        private static string NormalizeEventName(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var result = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c)) result.Append(char.ToLowerInvariant(c));
            }
            return result.ToString();
        }
    }
}
