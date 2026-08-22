using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    /// <summary>実行中のstock editorを操作するための入口。</summary>
    public static class Editor
    {
        private static readonly object Sync = new object();
        private static IEditorBackend _backend;
        private static EventService _events;
        private static bool _transactionActive;

        public static bool IsConfigured
        {
            get { lock (Sync) return _backend != null; }
        }

        public static object Current { get { return Backend.CurrentEditor; } }
        public static object Level { get { return Backend.CurrentLevel; } }
        public static IReadOnlyList<int> Selection { get { return Backend.CaptureSelection(); } }
        public static EventService Events { get { EnsureConfigured(); return _events; } }

        internal static IEditorBackend Backend
        {
            get { lock (Sync) return _backend ?? throw new EditorNotConfiguredException(); }
        }

        public static void Configure(IEditorBackend backend)
        {
            if (backend == null) throw new ArgumentNullException(nameof(backend));

            lock (Sync)
            {
                if (_transactionActive)
                    throw new InvalidOperationException("The editor backend cannot be replaced during a transaction.");

                _backend = backend;
                _events = new EventService(backend.Events);
            }
        }

        public static EditorSnapshot Snapshot()
        {
            var backend = Backend;
            return new EditorSnapshot(backend, backend.CloneLevel(), backend.CaptureSelection());
        }

        public static void Restore(EditorSnapshot snapshot, EditorRefreshOptions refresh = EditorRefreshOptions.All)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var backend = Backend;
            if (!ReferenceEquals(snapshot.Owner, backend))
                throw new InvalidOperationException("The snapshot belongs to a different editor backend.");

            backend.RestoreLevel(snapshot.Level);
            backend.Refresh(refresh & ~EditorRefreshOptions.RestoreSelection);
            if ((refresh & EditorRefreshOptions.RestoreSelection) != 0)
                backend.RestoreSelection(snapshot.SelectedFloors);
        }

        public static EditorTransaction BeginTransaction(
            EditorRefreshOptions commitRefresh = EditorRefreshOptions.All,
            EditorRefreshOptions rollbackRefresh = EditorRefreshOptions.All)
        {
            var backend = Backend;
            lock (Sync)
            {
                if (_transactionActive)
                    throw new InvalidOperationException("Nested editor transactions are not supported.");
                _transactionActive = true;
            }

            try
            {
                return new EditorTransaction(backend, Snapshot(), commitRefresh, rollbackRefresh);
            }
            catch
            {
                EndTransaction();
                throw;
            }
        }

        public static void Refresh(EditorRefreshOptions options = EditorRefreshOptions.All)
        {
            var backend = Backend;
            var selection = backend.CaptureSelection();
            backend.Refresh(options & ~EditorRefreshOptions.RestoreSelection);
            if ((options & EditorRefreshOptions.RestoreSelection) != 0)
                backend.RestoreSelection(selection);
        }

        internal static void EndTransaction()
        {
            lock (Sync) _transactionActive = false;
        }

        private static void EnsureConfigured()
        {
            var ignored = Backend;
        }
    }
}
