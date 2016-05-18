using System;
using IBot.Core;

namespace IBot.Events.Args.Connections
{
    internal class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(IrcConnection connection)
        {
            Connection = connection;
        }

        public IrcConnection Connection { get; }
    }
}
