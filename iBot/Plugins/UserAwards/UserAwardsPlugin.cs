using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using IBot.Core;
using IBot.Events;
using IBot.Events.Args.Users;
using IBot.Models;
using Timer = System.Timers.Timer;

namespace IBot.Plugins.UserAwards
{
    internal class UserAwardsPlugin : IPlugin
    {
        private static readonly Dictionary<string, List<Award>> UserAwards = new Dictionary<string, List<Award>>();

        public string PluginName => "User Awards Plugin";

        public void Init() => Start();

        public void Start()
        {
            var settings = SettingsManager.GetSettings<AwardSettings>();

            RegisterEvents();
        }

        public void Stop()
        {
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            UserList.UserListUpdated += OnUserListUpdated;
            UserEventManager.UserSpamEvent += OnUserSpam;
            UserEventManager.UserSpamEndEvent += OnUserSpamEnd;
        }

        private void UnregisterEvents()
        {
            UserList.UserListUpdated -= OnUserListUpdated;
            UserEventManager.UserSpamEvent -= OnUserSpam;
            UserEventManager.UserSpamEndEvent -= OnUserSpamEnd;
        }

        private void OnUserSpam(object sender, UserEventArgs eventArgs) => AddAward(eventArgs.UserName, new Award(AwardType.ChatterSpammer));

        private void OnUserSpamEnd(object sender, UserEventArgs eventArgs) => RemoveAward(eventArgs.UserName, AwardType.ChatterSpammer);

        private void OnUserListUpdated(object sender, EventArgs eventArgs)
        {
            var list = UserList.GetUserList(SettingsManager.GetOwnerChannel());

            foreach (var user in list)
            {
                AddAward(username: user.Username,
                         award: new Award(AwardType.JoinedChannel),
                         incrementIfAvailable: false);

                if (PermissionManager.GetRights(user).HasFlag(Rights.Follower))
                    AddAward(username: user.Username,
                             award: new Award(AwardType.Follows),
                             incrementIfAvailable: false);

                if (PermissionManager.GetRights(user).HasFlag(Rights.Subscriber))
                    AddAward(username: user.Username,
                             award: new Award(AwardType.Subscribed),
                             incrementIfAvailable: false);
            }
        }

        private void AddAward(string username, Award award, bool incrementIfAvailable = true, double incrementValue = 1.0d)
        {
            if (!UserAwards.ContainsKey(username))
                UserAwards.Add(username, new List<Award>());

            lock (UserAwards[username])
            {
                if (UserAwards[username].Any(a => a.Type == award.Type))
                {
                    if (incrementIfAvailable)
                    {
                        UserAwards[username].First(a => a.Type == award.Type).Add(incrementValue);
                    }

                    return;
                }

                UserAwards[username].Add(award);
            }
        }

        private void RemoveAward(string username, AwardType type)
        {
            if (!UserAwards.ContainsKey(username))
                UserAwards.Add(username, new List<Award>());

            lock (UserAwards[username])
            {
                UserAwards[username].RemoveAll(a => a.Type == type);
            }
        }

        public static IEnumerable<Award> GetAwards(User user) => GetAwards(user.Username);

        public static IEnumerable<Award> GetAwards(string username)
        {
            if (UserAwards.ContainsKey(username))
            {
                var retVal = new Award[UserAwards[username].Count];
                UserAwards[username].CopyTo(retVal);

                return retVal.ToList();
            }

            return new List<Award>();
        }

        public static double GetUserPoints(string username) => UserAwards[username].Sum(award => award.TotalValue);
    }
}
