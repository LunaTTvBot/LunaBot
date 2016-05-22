using IBot.Core;

namespace IBot.Events.Args.Connections
{
    internal class ConnectionUnexpectedCloseEventArgs
    {
        public ConnectionUnexpectedCloseEventArgs(string message, ConnectionType type)
        {
            Message = message;
            Type = type;
        }

        public string Message { get; }
        public ConnectionType Type { get; }
    }
}