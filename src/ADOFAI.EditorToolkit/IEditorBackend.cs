using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    /// <summary>ADOFAIのバージョン固有APIとToolkit本体の境界。</summary>
    public interface IEditorBackend
    {
        object CurrentEditor { get; }
        object CurrentLevel { get; }
        IEventBackend Events { get; }

        object CloneLevel();
        void RestoreLevel(object snapshot);
        IReadOnlyList<int> CaptureSelection();
        void RestoreSelection(IReadOnlyList<int> selectedFloors);
        void Refresh(EditorRefreshOptions options);
    }
}
