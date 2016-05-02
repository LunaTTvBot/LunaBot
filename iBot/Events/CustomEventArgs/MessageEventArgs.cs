using System;

namespace IRCConnectionTest.Events.CustomEventArgs
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