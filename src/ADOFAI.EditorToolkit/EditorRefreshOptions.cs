using System;

namespace ADOFAI.EditorToolkit
{
    [Flags]
    public enum EditorRefreshOptions
    {
        None = 0,
        RemakePath = 1 << 0,
        ApplyEventsToFloors = 1 << 1,
        UpdateDecorationObjects = 1 << 2,
        RestoreSelection = 1 << 3,
        All = RemakePath | ApplyEventsToFloors | UpdateDecorationObjects | RestoreSelection
    }
}
