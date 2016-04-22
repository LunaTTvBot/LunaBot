using System;

namespace IRCConnectionTest.Events.ComstumEventArgs
{
    internal class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string s)
        {
            Message = s;
        }

        public string Message { get; }
    }
}