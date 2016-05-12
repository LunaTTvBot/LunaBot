using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using IBot.Events.CustomEventArgs.UserList;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot
{
    internal class UserDatabaseManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static UserDatabaseManager _instance;

        private HashSet<string> _storedUsers;
        private ConcurrentQueue<KeyValuePair<User, DateTime>> _userQueue;
        private Thread _persistenceThread;
        private Timer _dbSinkTimer;
        private bool _continueWork;

        private UserDatabaseManager()
        {
            _instance = this;
            _storedUsers = new HashSet<string>();
            _userQueue = new ConcurrentQueue<KeyValuePair<User, DateTime>>();
            _persistenceThread = new Thread(PersistUsers);
            _continueWork = true;
            InitializeStoredUsers();

            _dbSinkTimer = new Timer(1000) { AutoReset = true };
            _dbSinkTimer.Elapsed += OnDbSinkTimerOnElapsed;

            _persistenceThread.Start();
            _dbSinkTimer.Start();

            UserList.UserJoined += AddUserToHistory;
        }

        private void InitializeStoredUsers()
        {
            var db = DatabaseContext.Get();

            lock (db)
            {
                foreach (var user in db.HistoryUsers)
                {
                    _storedUsers.Add(GetUniqueId(user));
                }
            }
        }

        private void OnDbSinkTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            var db = DatabaseContext.Get();
            lock (db)
            {
                db.SaveChangesAsync();
            }
        }

        private void AddUserToHistory(object sender, UserJoinEventArgs args) => StoreUser(args.JoinedUser, args.JoinTime);

        private void StoreUser(User user) => StoreUser(user, DateTime.Now);

        private void StoreUser(User user, DateTime time)
        {
            if (!_storedUsers.Contains(GetUniqueId(user))
                && !_userQueue.Any(kvp => kvp.Key.Username == user.Username
                                          && kvp.Key.Channel.Name == user.Channel.Name))
            {
                _userQueue.Enqueue(new KeyValuePair<User, DateTime>(user, time));
            }
        }

        private string GetUniqueId(DbUser user) => $"{user.ChannelName ?? user.DbChannel.Name}#{user.Username}";

        private string GetUniqueId(User user) => $"{user.ChannelName ?? user.Channel.Name}#{user.Username}";

        private void Stop()
        {
            _continueWork = false;
            _persistenceThread = null;
            _dbSinkTimer.Stop();
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
                    if (_storedUsers.Contains(GetUniqueId(user)))
                    {
                        continue;
                    }

                    lock (db)
                    {
                        var historyChannel = db
                            .HistoryChannels
                            .FirstOrDefault(c => c.Name == (user.Channel.Name ?? user.ChannelName));

                        if (historyChannel == null)
                        {
                            historyChannel = new DbChannel(new Channel(user.ChannelName ?? user.Channel.Name));
                            db.HistoryChannels.Add(historyChannel);
                            db.SaveChanges();

                            _logger.Trace("channel '{0}' added to history", user.Channel.Name ?? user.ChannelName);
                        }

                        var dbUser = user.ToDbUser();
                        dbUser.DbChannel = historyChannel;
                        dbUser.ChannelName = historyChannel.Name;
                        historyChannel.DbUsers.Add(dbUser);

                        _storedUsers.Add(GetUniqueId(user));

                        _logger.Trace("user {0}#{1}' added to history", historyChannel.Name, user.Username);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }

            // when the thread stops, save one last time
            db.SaveChanges();
        }

        public static UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());

        /// <summary>
        /// Initialise is called so the Instance is created
        /// </summary>
        public static void Initialise() => _instance = (_instance ?? new UserDatabaseManager());
    }
}
