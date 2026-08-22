using System;
using System.Collections.Generic;

namespace ADOFAI.EditorToolkit
{
    public sealed class EditorSnapshot
    {
        private readonly int[] _selectedFloors;

        internal EditorSnapshot(IEditorBackend owner, object level, IReadOnlyList<int> selectedFloors)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Level = level ?? throw new ArgumentNullException(nameof(level));

            if (selectedFloors == null)
            {
                _selectedFloors = new int[0];
            }
            else
            {
                _selectedFloors = new int[selectedFloors.Count];
                for (var i = 0; i < selectedFloors.Count; i++)
                    _selectedFloors[i] = selectedFloors[i];
            }
        }

        internal IEditorBackend Owner { get; }
        internal object Level { get; }

        public IReadOnlyList<int> SelectedFloors
        {
            get { return Array.AsReadOnly(_selectedFloors); }
        }
    }
}
