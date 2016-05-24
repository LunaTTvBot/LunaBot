using System;
using FacadeUser = IBot.Facades.Core.User;

namespace IBot.Facades.Events.Args
{
    public class UserPartedEventArgs
    {
        public UserPartedEventArgs(FacadeUser user, DateTime time)
        {
            User = user;
            Time = time;
        }

        public FacadeUser User { get; }
        public DateTime Time { get; }
    }
}