// This file is intentionally not part of the build.
// Replace the TODO portions with the exact members from the target ADOFAI version.

using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI.EditorToolkit;

internal static class ADOFAIAdapterSketch
{
    public static void Configure()
    {
        var eventBackend = new DelegateEventBackend(
            create: (name, floor) =>
            {
                // TODO: resolve LevelEventInfo by info.name, not only a dictionary key.
                // var info = ResolveEventInfo(name);
                // return new LevelEvent(floor, info.type, info);
                throw new NotImplementedException();
            },
            add: (raw, collection) =>
            {
                // Add to levelEvents or decorations according to collection.
                throw new NotImplementedException();
            },
            enumerate: collection =>
            {
                // Enumerate levelEvents, decorations, or both.
                throw new NotImplementedException();
            },
            getCollection: raw =>
            {
                // Find which LevelData list contains raw.
                throw new NotImplementedException();
            },
            getName: raw =>
            {
                // return ((LevelEvent)raw).info.name;
                throw new NotImplementedException();
            },
            getFloor: raw =>
            {
                // return ((LevelEvent)raw).floor;
                throw new NotImplementedException();
            },
            hasProperty: (raw, key) =>
            {
                // Resolve against LevelEventInfo metadata/defaults.
                throw new NotImplementedException();
            },
            getPropertyType: (raw, key) =>
            {
                // IMPORTANT: return the runtime type expected by ApplyEventsToFloors,
                // not merely the type currently stored in the dictionary.
                throw new NotImplementedException();
            },
            getProperty: (raw, key) =>
            {
                // return ((LevelEvent)raw)[key];
                throw new NotImplementedException();
            },
            setProperty: (raw, key, value) =>
            {
                // ((LevelEvent)raw)[key] = value;
                throw new NotImplementedException();
            },
            isPropertyDisabled: (raw, key) =>
            {
                // return ((LevelEvent)raw).disabled[key];
                throw new NotImplementedException();
            },
            setPropertyDisabled: (raw, key, disabled) =>
            {
                // ((LevelEvent)raw).disabled[key] = disabled;
                throw new NotImplementedException();
            },
            remove: raw =>
            {
                // return scnEditor.instance.levelData.levelEvents.Remove((LevelEvent)raw);
                throw new NotImplementedException();
            });

        var editorBackend = new DelegateEditorBackend(
            events: eventBackend,
            currentEditor: () =>
            {
                // return scnEditor.instance;
                throw new NotImplementedException();
            },
            currentLevel: () =>
            {
                // return scnEditor.instance.levelData;
                throw new NotImplementedException();
            },
            cloneLevel: () =>
            {
                // return scnEditor.instance.levelData.Copy();
                throw new NotImplementedException();
            },
            restoreLevel: snapshot =>
            {
                // Restore the snapshot using the same safe path as MultiTileEditor.
                throw new NotImplementedException();
            },
            captureSelection: () =>
            {
                // Return selected floor indices as a copied array.
                return new int[0];
            },
            restoreSelection: floors =>
            {
                // Restore floor selection after path/decoration rebuild.
                throw new NotImplementedException();
            },
            refresh: flags =>
            {
                // Keep the verified order here.
                // if ((flags & EditorRefreshOptions.RemakePath) != 0) ...
                // if ((flags & EditorRefreshOptions.ApplyEventsToFloors) != 0) ...
                // if ((flags & EditorRefreshOptions.UpdateDecorationObjects) != 0) ...
                throw new NotImplementedException();
            });

        Editor.Configure(editorBackend);
    }
}
