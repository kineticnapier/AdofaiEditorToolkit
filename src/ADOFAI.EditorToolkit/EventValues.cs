using System;

namespace ADOFAI.EditorToolkit
{
    public static class EventValues
    {
        public static ComponentValue Vector2(float x, float y)
        {
            return new ComponentValue(new[] { "x", "y" }, new object[] { x, y });
        }

        public static ComponentValue Vector3(float x, float y, float z)
        {
            return new ComponentValue(new[] { "x", "y", "z" }, new object[] { x, y, z });
        }

        public static ComponentValue Color(float r, float g, float b, float a = 1f)
        {
            return new ComponentValue(new[] { "r", "g", "b", "a" }, new object[] { r, g, b, a });
        }
    }

    public sealed class ComponentValue
    {
        internal ComponentValue(string[] names, object[] values)
        {
            Names = names ?? throw new ArgumentNullException(nameof(names));
            Values = values ?? throw new ArgumentNullException(nameof(values));
            if (names.Length != values.Length)
                throw new ArgumentException("Component names and values must have equal lengths.");
        }

        internal string[] Names { get; }
        internal object[] Values { get; }
    }
}
