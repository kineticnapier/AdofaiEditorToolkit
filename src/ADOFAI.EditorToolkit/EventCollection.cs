namespace ADOFAI.EditorToolkit
{
    public enum EventCollection
    {
        Actions = 0,
        Decorations = 1,
        All = 2,

        /// <summary>
        /// Create時にイベントmetadataから追加先を決める。Queryには使用できない。
        /// </summary>
        Auto = 3
    }
}
