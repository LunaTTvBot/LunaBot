using System;

namespace IRCConnectionTest
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