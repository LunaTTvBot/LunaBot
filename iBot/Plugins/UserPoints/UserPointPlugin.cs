using System.Collections.Generic;
using System.Timers;
using IBot.Core;
using IBot.Core.Settings;
using IBot.Models;
using Timer = System.Timers.Timer;

namespace IBot.Plugins.UserPoints
{
    internal class UserPointPlugin : IPlugin
    {
        private readonly Dictionary<string, long> _pointDictionary;
        private Timer _pointAwardTimer;

        public UserPointPlugin()
        {
            _pointDictionary = new Dictionary<string, long>();
        }

        private void PointAwardTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var channel in SettingsManager.GetSettings<ConnectionSettings>().ChannelList)
            {
                foreach (var user in UserList.GetUserList(channel))
                {
                    AddAmount(user, 1);
                }
            }
        }

        private void InitialiseTimer()
        {
            if (_pointAwardTimer != null)
            {
                if (_pointAwardTimer.Enabled)
                    _pointAwardTimer.Stop();

                _pointAwardTimer = null;
            }

            _pointAwardTimer = new Timer { AutoReset = true };
            _pointAwardTimer.Elapsed += PointAwardTimerOnElapsed;
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

        public string PluginName => "User Point Plugin";

        public void Init()
        {
            Start();
        }

        public double CheckAmount(User user) => CheckAmount(user.Id);

        public double CheckAmount(string username, string channel) => CheckAmount($"{channel}#{username}");

        public double CheckAmount(string userId) => _pointDictionary.ContainsKey(userId)
                                                        ? _pointDictionary[userId]
                                                        : 0.0d;

        public bool UserHasAmount(User user, long amount) => UserHasAmount(user.Id, amount);

        public bool UserHasAmount(string username, string channel, long amount) => UserHasAmount($"{channel}#{username}", amount);

        public bool UserHasAmount(string userId, long amount) => _pointDictionary.ContainsKey(userId) && (_pointDictionary[userId] >= amount);

        public void AddAmount(User user, long amount) => AddAmount(user.Id, amount);

        public void AddAmount(string username, string channel, long amount) => AddAmount($"{channel}#{username}", amount);

        public void AddAmount(string userId, long amount)
        {
            if (!_pointDictionary.ContainsKey(userId))
                _pointDictionary.Add(userId, 0);

            _pointDictionary[userId] += amount;
        }

        public bool RemoveAmount(User user, long amount) => RemoveAmount(user.Id, amount);

        public bool RemoveAmount(string username, string channel, long amount) => RemoveAmount($"{channel}#{username}", amount);

        public bool RemoveAmount(string userId, long amount)
        {
            if (!_pointDictionary.ContainsKey(userId))
                _pointDictionary.Add(userId, 0);

            if (!UserHasAmount(userId, amount))
                return false;

            _pointDictionary[userId] -= amount;
            return true;
        }
    }
}
