namespace DDSReader
{
    public enum DDSMessageType
    {
        Info,

        Warning,

        Error
    }

    public class DDSMessage
    {
        public DDSMessage(DDSMessageType type, string message)
        {
            Message = message;
            Type = type;
        }

        public string Message { get; private set; }

        public DDSMessageType Type { get; private set; }
    }
}
