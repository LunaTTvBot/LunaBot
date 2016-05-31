using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using IBot.Core;
using IBot.Core.Settings;
using IBot.Models;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot.Plugins.UserPoints
{
    internal class UserPointPlugin : IPlugin
    {
        private const string PointPropertyName = "UserPoints_Value";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _transactionLock = new object();
        private readonly Dictionary<User, long> _pointDictionary;
        private Timer _pointAwardTimer;

        public UserPointPlugin()
        {
            _pointDictionary = new Dictionary<User, long>();
        }

        public string PluginName => "User Point Plugin";

        public void Init()
        {
            Start();
        }

        private void InitialiseTimer()
        {
            if (_pointAwardTimer != null)
            {
                if (_pointAwardTimer.Enabled)
                    _pointAwardTimer.Stop();

                _pointAwardTimer = null;
            }

            _pointAwardTimer = new Timer
            {
                AutoReset = false,
                Interval = SettingsManager.GetSettings<PointSettings>().PointAwardIntervalSeconds * 1000
            };
            _pointAwardTimer.Elapsed += PointAwardTimerOnElapsed;
        }

        private void PointAwardTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var channel in SettingsManager.GetSettings<ConnectionSettings>().ChannelList)
                foreach (var user in UserList.GetUserList(channel))
                    AddPoints(user, SettingsManager.GetSettings<PointSettings>().PointsAwardedPerInterval);

            _pointAwardTimer.Enabled = true;
        }

        public void Start()
        {
            InitialiseTimer();
            _pointAwardTimer?.Start();
        }

        public void Stop()
        {
            if (_pointAwardTimer == null)
                return;

            if (_pointAwardTimer.Enabled)
                _pointAwardTimer.Stop();
            _pointAwardTimer = null;
        }

        private bool ChangeAmount(User user, long change, bool checkBalanceBeforeAction)
        {
            try
            {
                if (checkBalanceBeforeAction && !UserHasPoints(user, change))
                    return false;

                lock (_transactionLock)
                {
                    if (!_pointDictionary.ContainsKey(user))
                        AddUser(user);

                    var newValue = _pointDictionary[user] += change;
                    user.Set(PointPropertyName, newValue);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }

        private void AddUser(User user)
        {
            lock (_transactionLock)
            {
                if (_pointDictionary.ContainsKey(user))
                    return;

                _pointDictionary.Add(user, user.Get<long>(PointPropertyName));
            }
        }

        public long GetPoints(User user)
        {
            try
            {
                return user.Get<long>(PointPropertyName);
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return default(long);
            }
        }

        public bool UserHasPoints(User user, long amount)
        {
            try
            {
                return user.Get<long>(PointPropertyName) >= amount;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }

        public void AddPoints(User user, long amount) => ChangeAmount(user, Math.Abs(amount), false);

        public bool RemovePoints(User user, long amount) => ChangeAmount(user, amount * -1, true);
    }
}
