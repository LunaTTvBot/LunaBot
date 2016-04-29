using System;

namespace IRCConnectionTest.Events.CustomEventArgs
{
    internal class UserEventArgs : EventArgs
    {
        public UserEventArgs(string userName, string channel, UserEventType type)
        {
            UserName = userName;
            Channel = channel;
            Type = type;
        }

        public string UserName { get; }
        public string Channel { get; }
        public UserEventType Type { get; }
    }

    internal enum UserEventType
    {
        Join,
        Part
    }
}