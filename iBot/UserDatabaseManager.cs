using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using IBot.Events.CustomEventArgs.UserList;

namespace IBot
{
    internal class UserDatabaseManager
    {
        private static UserDatabaseManager _instance;

        private List<DbChannel> _channels;

        private UserDatabaseManager()
        {
            _channels = new List<DbChannel>();
            _instance = this;
            UserList.UserJoined += AddUserToHistory;
        }

        private void AddUserToHistory(object sender, UserJoinEventArgs args) => StoreUser(args.JoinedUser, args.JoinTime);

        private void StoreUser(User user) => StoreUser(user, DateTime.Now);

        private void StoreUser(User user, DateTime time)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var db = DatabaseContext.Get();

                try
                {
                    lock (db)
                    {
                        var historyChannel = db.HistoryChannels.Include("DbUsers").FirstOrDefault(c => c.Name == (user.Channel.Name ?? user.ChannelName));

                        if (historyChannel == null)
                        {
                            historyChannel = new DbChannel(user.Channel ?? new Channel(user.ChannelName));
                            db.HistoryChannels.Add(historyChannel);
                        }

                        if (!historyChannel.DbUsers.Any(u => u.Username == user.Username))
                        {
                            var dbUser = user.ToDbUser();
                            dbUser.DbChannel = historyChannel;
                            dbUser.ChannelName = historyChannel.Name;
                            historyChannel.DbUsers.Add(dbUser);
                        }

                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ;
                }
            });
        }

        public static UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());

        /// <summary>
        /// Initialise is called so the Instance is created
        /// </summary>
        public static void Initialise() => _instance = (_instance ?? new UserDatabaseManager());
    }
}
