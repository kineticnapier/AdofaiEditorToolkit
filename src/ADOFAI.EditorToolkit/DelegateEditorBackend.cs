using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    /// <summary>ADOFAI固有コードを数個のdelegateだけで接続するbackend。</summary>
    public sealed class DelegateEditorBackend : IEditorBackend
    {
        private readonly Func<object> _currentEditor;
        private readonly Func<object> _currentLevel;
        private readonly Func<object> _cloneLevel;
        private readonly Action<object> _restoreLevel;
        private readonly Func<IReadOnlyList<int>> _captureSelection;
        private readonly Action<IReadOnlyList<int>> _restoreSelection;
        private readonly Action<EditorRefreshOptions> _refresh;

        public DelegateEditorBackend(
            IEventBackend events,
            Func<object> currentEditor,
            Func<object> currentLevel,
            Func<object> cloneLevel,
            Action<object> restoreLevel,
            Func<IReadOnlyList<int>> captureSelection,
            Action<IReadOnlyList<int>> restoreSelection,
            Action<EditorRefreshOptions> refresh)
        {
            Events = events ?? throw new ArgumentNullException(nameof(events));
            _currentEditor = currentEditor ?? throw new ArgumentNullException(nameof(currentEditor));
            _currentLevel = currentLevel ?? throw new ArgumentNullException(nameof(currentLevel));
            _cloneLevel = cloneLevel ?? throw new ArgumentNullException(nameof(cloneLevel));
            _restoreLevel = restoreLevel ?? throw new ArgumentNullException(nameof(restoreLevel));
            _captureSelection = captureSelection ?? throw new ArgumentNullException(nameof(captureSelection));
            _restoreSelection = restoreSelection ?? throw new ArgumentNullException(nameof(restoreSelection));
            _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        }

        public object CurrentEditor { get { return _currentEditor(); } }
        public object CurrentLevel { get { return _currentLevel(); } }
        public IEventBackend Events { get; }
        public object CloneLevel() { return _cloneLevel(); }
        public void RestoreLevel(object snapshot) { _restoreLevel(snapshot); }
        public IReadOnlyList<int> CaptureSelection() { return _captureSelection(); }
        public void RestoreSelection(IReadOnlyList<int> selectedFloors) { _restoreSelection(selectedFloors); }
        public void Refresh(EditorRefreshOptions options) { _refresh(options); }
    }
}
