using System.Collections.Generic;

namespace IBot.Events.CustomEventArgs
{
    internal class UserListEventArgs
    {
        public UserListEventArgs(List<string> userList, string user, string channel)
        {
            UserList = userList;
            User = user;
            Channel = channel;
        }

        public List<string> UserList { get; }
        public string User { get; }
        public string Channel { get; }
    }
}