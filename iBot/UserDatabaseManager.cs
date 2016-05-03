using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IBot.Events.CustomEventArgs.UserList;

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
            UserList.UserJoined += AddUserToHistory;
        }

        private void AddUserToHistory(object sender, UserJoinEventArgs args)
        {
            Trace.WriteLine($"{args.JoinedUser.Channel.Name}#{args.JoinedUser.Username} at {args.JoinTime.ToLongTimeString()}");
            StoreUser(args.JoinedUser, args.JoinTime);
        }

        private void StoreUser(User user) => StoreUser(user, DateTime.Now);

        private void StoreUser(User user, DateTime time)
        {
            var db = DatabaseContext.Get();

            lock (db)
            {
                var historyChannel = db.HistoryChannels.FirstOrDefault(c => c.Name == (user.Channel.Name ?? user.ChannelName));

                if (historyChannel == null)
                {
                    historyChannel = new DbChannel(user.Channel ?? new Channel(user.ChannelName));
                    db.HistoryChannels.Add(historyChannel);
                }

                if (!historyChannel.DbUsers.Any(u => u.Username == user.Username))
                {
                    historyChannel.DbUsers.Add(user.ToDbUser());
                }

                db.SaveChanges();
            }
        }

        public static UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());

        /// <summary>
        /// Initialise is called so the Instance is created
        /// </summary>
        public static void Initialise() => _instance = (_instance ?? new UserDatabaseManager());
    }
}
