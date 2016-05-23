using System;

namespace IBot.Facades.Events.Args
{
    public class UserJoinEventArgs
    {
        public UserJoinEventArgs(Core.User user, DateTime time)
        {
            User = user;
            Time = time;
        }

        public Core.User User { get; }
        public DateTime Time { get; }
    }
}
