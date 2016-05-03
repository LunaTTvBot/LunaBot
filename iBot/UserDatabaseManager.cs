using System.Collections.Generic;

namespace IBot
{
    internal class UserDatabaseManager
    {
        private static UserDatabaseManager _instance;

        private List<Channel> _channels;

        private UserDatabaseManager()
        {
            _channels = new List<Channel>();
            _instance = this;
        }

        public UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());
    }
}
