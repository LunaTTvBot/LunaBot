using System;

namespace IBot.Facades.Events.Args.Channel
{
    public class JoinPartEventArgs
    {
        public JoinPartEventArgs(string channel, string user, DateTime time)
        {
            Channel = channel;
            User = user;
            Time = time;
        }

        public string Channel { get; }
        public string User { get; }
        public DateTime Time { get; }
    }
}