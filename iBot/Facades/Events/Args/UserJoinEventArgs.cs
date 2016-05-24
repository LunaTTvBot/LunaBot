using System;
using FacadeUser = IBot.Facades.Core.User;

namespace IBot.Facades.Events.Args
{
    public class UserJoinEventArgs
    {
        public UserJoinEventArgs(FacadeUser user, DateTime time)
        {
            User = user;
            Time = time;
        }

        public FacadeUser User { get; }
        public DateTime Time { get; }
    }
}
