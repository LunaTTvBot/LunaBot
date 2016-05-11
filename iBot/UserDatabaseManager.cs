using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using IBot.Events.CustomEventArgs.UserList;
using NLog;

namespace IBot
{
    internal class UserDatabaseManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static UserDatabaseManager _instance;

        private List<DbChannel> _channels;
        private ConcurrentQueue<KeyValuePair<User, DateTime>> _userQueue;
        private Thread _persistenceThread;
        private bool _continueWork;

        private UserDatabaseManager()
        {
            _instance = this;
            _channels = new List<DbChannel>();
            _userQueue = new ConcurrentQueue<KeyValuePair<User, DateTime>>();
            _persistenceThread = new Thread(PersistUsers);
            _continueWork = true;
            UserList.UserJoined += AddUserToHistory;
            _persistenceThread.Start();

            var x = new System.Timers.Timer(1000)
            {
                AutoReset = true,
                Interval = 1000.0,
            };
            x.Elapsed += (sender, args) => _logger.Info("queue length: {0}", _userQueue.Count);
            x.Start();
        }

        private void AddUserToHistory(object sender, UserJoinEventArgs args) => StoreUser(args.JoinedUser, args.JoinTime);

        private void StoreUser(User user) => StoreUser(user, DateTime.Now);

        private void StoreUser(User user, DateTime time)
        {
            if (!_userQueue.Any(kvp => kvp.Key.Username == user.Username
                                       && kvp.Key.Channel.Name == user.Channel.Name))
            {
                _userQueue.Enqueue(new KeyValuePair<User, DateTime>(user, time));
                _logger.Trace("enqueue: {0}", _userQueue.Count);
            }
        }

        private void PersistUsers()
        {
            var db = DatabaseContext.Get();

            while (_continueWork)
            {
                KeyValuePair<User, DateTime> pair;

                if (!_userQueue.TryDequeue(out pair))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.1d));
                    continue;
                }

                var user = pair.Key;
                var time = pair.Value;

                try
                {
                    lock (db)
                    {
                        var historyChannel = _channels.FirstOrDefault(c => c.Name == (user.Channel.Name ?? user.ChannelName));
                        if (historyChannel == null)
                        {
                            historyChannel = db
                                .HistoryChannels
                                .Include("DbUsers")
                                .FirstOrDefault(c => c.Name == (user.Channel.Name ?? user.ChannelName));

                            if (historyChannel == null)
                            {
                                _logger.Debug("channel '{0}' added to history", user.Channel.Name ?? user.ChannelName);
                                historyChannel = new DbChannel(user.Channel ?? new Channel(user.ChannelName));
                                db.HistoryChannels.Add(historyChannel);
                            }

                            _channels.Add(historyChannel);
                        }

                        if (historyChannel.DbUsers.All(u => u.Username != user.Username))
                        {
                            _logger.Debug("user {0}#{1}' added to history", historyChannel.Name, user.Username);
                            var dbUser = user.ToDbUser();
                            dbUser.DbChannel = historyChannel;
                            dbUser.ChannelName = historyChannel.Name;
                            historyChannel.DbUsers.Add(dbUser);
                        }

                        db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }

                _logger.Trace("dequeue: {0} ", _userQueue.Count);
            }
        }

        public static UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());

        /// <summary>
        /// Initialise is called so the Instance is created
        /// </summary>
        public static void Initialise() => _instance = (_instance ?? new UserDatabaseManager());
    }
}
