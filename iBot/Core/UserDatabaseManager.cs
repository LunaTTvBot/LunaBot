using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using IBot.Database;
using IBot.Events.Args.UserList;
using IBot.Models;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot.Core
{
    internal class UserDatabaseManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static UserDatabaseManager _instance;

        private readonly HashSet<string> _storedUsers;
        private readonly ConcurrentQueue<KeyValuePair<User, DateTime>> _userQueue;
        private bool _continueWork;
        private Timer _dbSinkTimer;
        private Thread _persistenceThread;

        private UserDatabaseManager()
        {
            _instance = this;
            _storedUsers = new HashSet<string>();
            _userQueue = new ConcurrentQueue<KeyValuePair<User, DateTime>>();

            Start();
        }

        /// <summary>
        /// Singleton Pattern
        /// </summary>
        public static UserDatabaseManager Instance => _instance ?? (_instance = new UserDatabaseManager());

        /// <summary>
        /// Retrieve all current users from the <seealso cref="DatabaseContext"/>, to improve performance.
        /// </summary>
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

        /// <summary>
        /// Event for <seealso cref="_dbSinkTimer"/>, periodically saves changes made to the <seealso cref="DatabaseContext"/>.
        /// </summary>
        private void OnDbSinkTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            var db = DatabaseContext.Get();
            lock (db)
            {
                db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Event handler for <seealso cref="UserList.UserJoined"/>.
        /// </summary>
        private void AddUserToHistory(object sender, UserJoinEventArgs args) => AddUserToQueue(args.JoinedUser, args.JoinTime);

        /// <summary>
        /// Add the User to the <seealso cref="_userQueue"/> to be processed by the <seealso cref="_persistenceThread"/>.
        /// </summary>
        /// <param name="user">A valid <seealso cref="User"/> object</param>
        private void AddUserToQueue(User user) => AddUserToQueue(user, DateTime.Now);

        /// <summary>
        /// Add the User to the <seealso cref="_userQueue"/> to be processed by the <seealso cref="_persistenceThread"/>.
        /// </summary>
        /// <param name="user">A valid <seealso cref="User"/> object</param>
        /// <param name="time">Optional desired time the <seealso cref="User"/> was first encountered</param>
        private void AddUserToQueue(User user, DateTime time)
        {
            if (!_storedUsers.Contains(GetUniqueId(user))
                && !_userQueue.Any(kvp => kvp.Key.Username == user.Username
                                          && kvp.Key.Channel.Name == user.Channel.Name))
            {
                _userQueue.Enqueue(new KeyValuePair<User, DateTime>(user, time));
            }
        }

        /// <summary>
        /// Get the Unique Identifier for the given User.
        /// </summary>
        /// <param name="user">A <seealso cref="DbUser"/> object with a set Username and ChannelName or Channel</param>
        /// <returns>$"{user.ChannelName ?? user.Channel.Name}#{user.Username}"</returns>
        private string GetUniqueId(DbUser user) => $"{user.ChannelName ?? user.DbChannel.Name}#{user.Username}";

        /// <summary>
        /// Get the Unique Identifier for the given User.
        /// </summary>
        /// <param name="user">A <seealso cref="User"/> object with a set Username and ChannelName or Channel</param>
        /// <returns>$"{user.ChannelName ?? user.Channel.Name}#{user.Username}"</returns>
        private string GetUniqueId(User user) => $"{user.ChannelName ?? user.Channel.Name}#{user.Username}";

        /// <summary>
        /// Stop listening for Users.
        /// </summary>
        public void Stop()
        {
            _continueWork = false;
            _dbSinkTimer.Stop();

            _persistenceThread = null;
            _dbSinkTimer = null;

            UserList.UserJoined -= AddUserToHistory;
        }

        /// <summary>
        /// <para>Start listening for Joining Users and Persist them to the DB.</para>
        /// <para>Only required after stopping the instance with <seealso cref="Stop"/></para>
        /// </summary>
        public void Start()
        {
            _storedUsers.Clear();
            InitializeStoredUsers();

            _continueWork = true;

            _persistenceThread = new Thread(PersistUsers);

            _dbSinkTimer = new Timer(1000) { AutoReset = true };
            _dbSinkTimer.Elapsed += OnDbSinkTimerOnElapsed;

            _dbSinkTimer.Start();
            _persistenceThread.Start();

            UserList.UserJoined += AddUserToHistory;
        }

        /// <summary>
        /// Body-Function of <seealso cref="_persistenceThread"/>, used to save Users in the <seealso cref="_userQueue"/> to the DB.
        /// </summary>
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

                        user.Set("FirstRecorded", time.Ticks);

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

        /// <summary>
        ///     Initialise is called so the Instance is created
        /// </summary>
        public static void Initialise() => _instance = _instance ?? new UserDatabaseManager();
    }
}
