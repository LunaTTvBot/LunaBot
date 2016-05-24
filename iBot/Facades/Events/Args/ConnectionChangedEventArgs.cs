using System;
using IBot.Facades.Core;

namespace IBot.Facades.Events.Args
{
    public class ConnectionChangedEventArgs : EventArgs
    {
        public ConnectionChangeType ChangeType { get; internal set; }
        public ConnectionType ConnectionType { get; internal set; }

        public ConnectionChangedEventArgs(ConnectionType connectionType, ConnectionChangeType changeType)
        {
            ConnectionType = connectionType;
            ChangeType = changeType;
        }
    }
}