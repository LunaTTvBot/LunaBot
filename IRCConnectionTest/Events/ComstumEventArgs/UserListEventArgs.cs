using System.Collections.Generic;

namespace IRCConnectionTest.Events.ComstumEventArgs
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