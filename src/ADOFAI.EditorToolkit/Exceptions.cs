using System;

namespace ADOFAI.EditorToolkit
{
    public class EditorToolkitException : Exception
    {
        public EditorToolkitException(string message) : base(message) { }
        public EditorToolkitException(string message, Exception innerException) : base(message, innerException) { }
    }

    public sealed class EditorNotConfiguredException : EditorToolkitException
    {
        public EditorNotConfiguredException()
            : base("Editor.Configure(IEditorBackend) must be called before using the toolkit.") { }
    }

    public sealed class EventPropertyException : EditorToolkitException
    {
        public EventPropertyException(string message) : base(message) { }
        public EventPropertyException(string message, Exception innerException) : base(message, innerException) { }
    }

    public sealed class EditorTransactionException : EditorToolkitException
    {
        public EditorTransactionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
