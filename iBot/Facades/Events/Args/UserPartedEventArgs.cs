using System;

namespace IBot.Facades.Events.Args
{
    public class UserPartedEventArgs
    {
        public UserPartedEventArgs(Core.User user, DateTime time)
        {
            User = user;
            Time = time;
        }

        public Core.User User { get; }
        public DateTime Time { get; }
    }
}
