using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using IBot.Core.Settings;
using IBot.Events;
using IBot.Events.Args.UserList;
using IBot.Events.Args.Users;
using IBot.Models;
using IBot.TwitchAPI;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot.Core
{
    internal static class PermissionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, Rights> UserRights;
        private static Timer _apiRightsTimer;

        static PermissionManager()
        {
            UserRights = new Dictionary<string, Rights>();
            Start();
        }

        public static Rights GetRights(User u) => GetRights(u.Id);

        public static Rights GetRights(string username, string channel) => GetRights(GetUniqueIdentifier(channel, username));

        public static Rights GetRights(string userId)
        {
            if (userId == GetUniqueIdentifier(SettingsManager.GetOwnerChannel(), SettingsManager.GetOwnerChannel()))
                return GetEffectiveRights(Rights.Owner);

            return UserRights.ContainsKey(userId)
                       ? UserRights[userId]
                       : Rights.Viewer;
        }

        private static void StartApiTimer()
        {
            if (_apiRightsTimer != null)
            {
                _apiRightsTimer.Stop();
                _apiRightsTimer = null;
            }

            _apiRightsTimer = new Timer()
            {
                AutoReset = false,
                Interval = 60 * 1000 * 1,
            };
            _apiRightsTimer.Elapsed += UpdateApiRights;
            _apiRightsTimer.Start();
        }

        private static void StopApiTimer()
        {
            _apiRightsTimer?.Stop();
        }

        private static void UpdateApiRights(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                var channels = SettingsManager.GetSettings<ConnectionSettings>().ChannelList;

                foreach (var channel in channels)
                {
                    var subscribers = Twitch.GetChannelSubscribers(channel);

                    lock (UserRights)
                    {
                        foreach (var subscriber in subscribers)
                        {
                            var id = GetUniqueIdentifier(channel, subscriber);
                            if (UserRights.ContainsKey(id))
                                UserRights[id] |= GetEffectiveRights(Rights.Subscriber);
                            else
                                UserRights.Add(id, GetEffectiveRights(Rights.Subscriber));
                        }
                    }

                    var followers = Twitch.GetChannelFollowers(channel);

                    lock (UserRights)
                    {
                        foreach (var follower in followers)
                        {
                            var id = GetUniqueIdentifier(channel, follower);
                            if (UserRights.ContainsKey(id))
                                UserRights[id] |= GetEffectiveRights(Rights.Follower);
                            else
                                UserRights.Add(id, GetEffectiveRights(Rights.Follower));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e);
            }
            finally
            {
                _apiRightsTimer.Enabled = true;
            }
        }

        public static void Initialise() {}

        public static void Start()
        {
            UserList.UserJoined += OnUserJoined;
            UserList.UserParted += OnUserParted;
            ChannelEventManager.OperatorGrantedEvent += OnUserGrantedOperator;
            ChannelEventManager.OperatorRevokedEvent += OnUserRevokedOperator;

            InitializeUsers();

            StartApiTimer();
        }

        public static void Stop()
        {
            UserList.UserJoined -= OnUserJoined;
            UserList.UserParted -= OnUserParted;
            ChannelEventManager.OperatorGrantedEvent -= OnUserGrantedOperator;
            ChannelEventManager.OperatorRevokedEvent -= OnUserRevokedOperator;

            StopApiTimer();
        }

        private static string GetUniqueIdentifier(User u) => GetUniqueIdentifier(u.ChannelName ?? u.Channel.Name, u.Username);

        private static string GetUniqueIdentifier(string channel, string username) => $"{channel}#{username}";

        private static void InitializeUsers()
        {
            foreach (var user in SettingsManager
                .GetSettings<ConnectionSettings>()
                .ChannelList
                .SelectMany(UserList.GetUserList))
            {
                AddUser(user, Rights.Viewer);
            }
        }

        private static void AddUser(User user, Rights rights) => AddUser(user.ChannelName ?? user.Channel.Name, user.Username, rights);

        private static void AddUser(string channel, string username, Rights rights)
        {
            var identifier = GetUniqueIdentifier(channel, username);
            var ownerName = SettingsManager.GetSettings<ConnectionSettings>().OwnerUsername;

            if (UserRights.ContainsKey(identifier))
                return;

            UserRights.Add(identifier, username == ownerName
                                           ? GetEffectiveRights(Rights.Owner)
                                           : GetEffectiveRights(rights));
        }

        private static void RemoveUser(User user) => RemoveUser(user.ChannelName ?? user.Channel.Name, user.Username);

        private static void RemoveUser(string channel, string username)
        {
            var identifier = GetUniqueIdentifier(channel, username);

            if (UserRights.ContainsKey(identifier))
                UserRights.Remove(identifier);
        }

        private static void OnUserJoined(object sender, UserJoinEventArgs eventArgs) => AddUser(eventArgs.JoinedUser, Rights.Viewer);

        private static void OnUserParted(object sender, UserPartedEventArgs eventArgs) => RemoveUser(eventArgs.PartedUser);

        private static void OnUserRevokedOperator(object sender, OperatorModeEventArgs eventArgs) => AddUser(eventArgs.Channel, eventArgs.User, Rights.Moderator);

        private static void OnUserGrantedOperator(object sender, OperatorModeEventArgs eventArgs) => RemoveUser(eventArgs.Channel, eventArgs.User);

        public static Rights GetEffectiveRights(Rights baseRights)
        {
            switch (baseRights)
            {
                case Rights.Owner:
                    return Rights.Owner | Rights.Moderator | Rights.Subscriber | Rights.Follower | Rights.Viewer;
                case Rights.Moderator:
                    return Rights.Moderator | Rights.Subscriber | Rights.Follower | Rights.Viewer;
                case Rights.Subscriber:
                    return Rights.Subscriber | Rights.Follower | Rights.Viewer;
                case Rights.Follower:
                    return Rights.Follower | Rights.Viewer;
                case Rights.Viewer:
                    return Rights.Viewer;
                default:
                    return baseRights;
            }
        }
    }
}
