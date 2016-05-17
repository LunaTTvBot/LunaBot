using System;

namespace IBot.Events.Args.Users
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