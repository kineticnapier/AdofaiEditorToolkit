using System;
using System.Collections.Generic;
using global::ADOFAI;

namespace ADOFAI.EditorToolkit.Game
{
    /// <summary>ADOFAI 3.x stock editorへの標準接続部。</summary>
    public sealed class ADOFAIEditorBackend : IEditorBackend
    {
        private readonly ADOFAIEventBackend _events;

        public ADOFAIEditorBackend()
        {
            _events = new ADOFAIEventBackend(RequireEditor);
        }

        public object CurrentEditor { get { return RequireEditor(); } }
        public object CurrentLevel { get { return RequireEditor().levelData; } }
        public IEventBackend Events { get { return _events; } }

        public object CloneLevel()
        {
            LevelData level = RequireEditor().levelData;
            if (level == null) throw new InvalidOperationException("The stock editor has no LevelData.");
            return level.Copy();
        }

        public void RestoreLevel(object snapshot)
        {
            LevelData level = snapshot as LevelData;
            if (level == null) throw new ArgumentException("The snapshot is not ADOFAI LevelData.", nameof(snapshot));

            scnEditor editor = RequireEditor();
            if (editor.customLevel == null)
                throw new InvalidOperationException("The stock editor has no custom level instance.");
            editor.customLevel.levelData = level.Copy();
        }

        public IReadOnlyList<int> CaptureSelection()
        {
            scnEditor editor = RequireEditor();
            var selected = new List<int>();
            if (editor.floors == null || editor.selectedFloors == null) return selected.AsReadOnly();

            for (int s = 0; s < editor.selectedFloors.Count; s++)
            {
                object selectedFloor = editor.selectedFloors[s];
                for (int i = 0; i < editor.floors.Count; i++)
                {
                    if (!ReferenceEquals(editor.floors[i], selectedFloor)) continue;
                    if (!selected.Contains(i)) selected.Add(i);
                    break;
                }
            }
            return selected.AsReadOnly();
        }

        public void RestoreSelection(IReadOnlyList<int> selectedFloors)
        {
            scnEditor editor = RequireEditor();
            editor.DeselectFloors(false);
            if (selectedFloors == null || editor.floors == null) return;

            for (int i = 0; i < selectedFloors.Count; i++)
            {
                int floor = selectedFloors[i];
                if (floor >= 0 && floor < editor.floors.Count)
                    editor.SelectFloor(editor.floors[floor], true);
            }
        }

        public void Refresh(EditorRefreshOptions options)
        {
            scnEditor editor = RequireEditor();

            if ((options & EditorRefreshOptions.RemakePath) != 0)
            {
                editor.DeselectFloors(false);
                editor.RemakePath(true, true);
            }
            if ((options & EditorRefreshOptions.ApplyEventsToFloors) != 0)
                editor.ApplyEventsToFloors();
            if ((options & EditorRefreshOptions.UpdateDecorationObjects) != 0)
            {
                editor.DeselectAllDecorations();
                editor.UpdateDecorationObjects();
            }
        }

        public static void ConfigureToolkit()
        {
            global::ADOFAI.EditorToolkit.Editor.Configure(new ADOFAIEditorBackend());
        }

        private static scnEditor RequireEditor()
        {
            scnEditor editor = ADOBase.editor;
            if (editor == null) throw new InvalidOperationException("ADOFAI stock editor is not active.");
            return editor;
        }
    }
}
