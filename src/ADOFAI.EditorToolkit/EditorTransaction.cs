using System;

namespace ADOFAI.EditorToolkit
{
    public sealed class EditorTransaction : IDisposable
    {
        private readonly IEditorBackend _backend;
        private readonly EditorSnapshot _snapshot;
        private readonly EditorRefreshOptions _commitRefresh;
        private readonly EditorRefreshOptions _rollbackRefresh;
        private bool _completed;

        internal EditorTransaction(
            IEditorBackend backend,
            EditorSnapshot snapshot,
            EditorRefreshOptions commitRefresh,
            EditorRefreshOptions rollbackRefresh)
        {
            _backend = backend;
            _snapshot = snapshot;
            _commitRefresh = commitRefresh;
            _rollbackRefresh = rollbackRefresh;
        }

        public void Commit()
        {
            EnsureActive();
            try
            {
                var selection = _backend.CaptureSelection();
                _backend.Refresh(_commitRefresh & ~EditorRefreshOptions.RestoreSelection);
                if ((_commitRefresh & EditorRefreshOptions.RestoreSelection) != 0)
                    _backend.RestoreSelection(selection);
                Complete();
            }
            catch (Exception refreshError)
            {
                try
                {
                    RestoreSnapshot(_rollbackRefresh);
                }
                catch (Exception rollbackError)
                {
                    Complete();
                    var transactionError = new EditorTransactionException(
                        "Commit failed and rollback also failed. See InnerException and Data[\"RollbackException\"].",
                        refreshError);
                    transactionError.Data["RollbackException"] = rollbackError;
                    throw transactionError;
                }

                Complete();
                throw new EditorTransactionException("Commit refresh failed; the original level was restored.", refreshError);
            }
        }

        public void Rollback()
        {
            EnsureActive();
            try
            {
                RestoreSnapshot(_rollbackRefresh);
            }
            finally
            {
                Complete();
            }
        }

        public void Dispose()
        {
            if (!_completed) Rollback();
        }

        private void RestoreSnapshot(EditorRefreshOptions refresh)
        {
            _backend.RestoreLevel(_snapshot.Level);
            _backend.Refresh(refresh & ~EditorRefreshOptions.RestoreSelection);
            if ((refresh & EditorRefreshOptions.RestoreSelection) != 0)
                _backend.RestoreSelection(_snapshot.SelectedFloors);
        }

        private void EnsureActive()
        {
            if (_completed) throw new InvalidOperationException("The transaction has already completed.");
        }

        private void Complete()
        {
            if (_completed) return;
            _completed = true;
            Editor.EndTransaction();
        }
    }
}
