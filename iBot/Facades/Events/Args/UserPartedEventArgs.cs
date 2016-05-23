using System;
using IBot.Facades.Core;

namespace IBot.Facades.Events.Args
{
    public class UserPartedEventArgs
    {
        public UserPartedEventArgs(User user, DateTime time)
        {
            User = user;
            Time = time;
        }

        public User User { get; }
        public DateTime Time { get; }
    }
}
