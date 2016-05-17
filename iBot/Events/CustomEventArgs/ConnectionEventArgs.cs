namespace IBot.Events.CustomEventArgs
{
    public class ConnectionEventArgs
    {
        public ConnectionType Type { get; }

        public ConnectionEventArgs(ConnectionType type)
        {
            Type = type;
        }
    }
}