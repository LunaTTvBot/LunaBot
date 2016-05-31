using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using IBot.Core;
using IBot.Models;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot.Plugins.UserPoints
{
    internal class UserPointPlugin : IPlugin
    {
        private const string PointPropertyName = "UserPoints_Value";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly object TransactionLock = new object();
        private static readonly Dictionary<string, long> PointDictionary = new Dictionary<string, long>();
        private static Timer _pointAwardTimer;

        public string PluginName => "User Point Plugin";

        public void Init()
        {
            Start();
        }

        private static void InitialiseTimer()
        {
            if (_pointAwardTimer != null)
            {
                if (_pointAwardTimer.Enabled)
                    _pointAwardTimer.Stop();

                _pointAwardTimer = null;
            }

            _pointAwardTimer = new Timer
            {
                AutoReset = true,
                Interval = SettingsManager.GetSettings<PointSettings>().PointAwardIntervalSeconds * 1000
            };
            _pointAwardTimer.Elapsed += PointAwardTimerOnElapsed;
        }

        private static void PointAwardTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var user in UserList.GetUserList(SettingsManager.GetOwnerChannel()))
                AddPoints(user, SettingsManager.GetSettings<PointSettings>().PointsAwardedPerInterval);
        }

        public static void Start()
        {
            InitialiseTimer();
            _pointAwardTimer?.Start();
        }

        public static void Stop()
        {
            if (_pointAwardTimer == null)
                return;

            if (_pointAwardTimer.Enabled)
                _pointAwardTimer.Stop();
            _pointAwardTimer = null;
        }

        private static bool ChangeAmount(User user, long change, bool checkBalanceBeforeAction)
        {
            try
            {
                if (checkBalanceBeforeAction && !UserHasPoints(user, change))
                    return false;

                lock (TransactionLock)
                {
                    if (!PointDictionary.ContainsKey(user.Id))
                        AddUser(user);

                    var newValue = PointDictionary[user.Id] += change;
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

        private static void AddUser(User user)
        {
            lock (TransactionLock)
            {
                if (PointDictionary.ContainsKey(user.Id))
                    return;

                PointDictionary.Add(user.Id, user.Get<long>(PointPropertyName));
            }
        }

        public static long GetPoints(User user)
        {
            try
            {
                lock (PointDictionary)
                {
                    var savedUser = PointDictionary.FirstOrDefault(u => u.Key == user.Id);

                    return savedUser.Key != null
                               ? savedUser.Value
                               : user.Get<long>(PointPropertyName);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return default(long);
            }
        }

        public static bool UserHasPoints(User user, long amount)
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

        public static void AddPoints(User user, long amount)
        {
            ChangeAmount(user, Math.Abs(amount), false);
            Logger.Trace("user {0} received {1} points", user.Id, amount);
        }

        public static bool RemovePoints(User user, long amount)
        {
            var success = ChangeAmount(user, amount * -1, true);
            Logger.Trace("user {0} lost {1} points", user.Id, amount);

            return success;
        }
    }
}
