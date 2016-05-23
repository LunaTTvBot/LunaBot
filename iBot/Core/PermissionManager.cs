using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using IBot.Core.Settings;
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

            UserList.UserJoined += OnUserJoined;
            UserList.UserParted += OnUserParted;
            IrcConnectionManager.RegisterMessageHandler(ConnectionType.BotCon, OnMessageReceived);

            InitializeUsers();
            StartApiTimer();
        }

        public static Rights GetRights(User u) => GetRights(u.Username);

        public static Rights GetRights(string username)
        {
            return UserRights.ContainsKey(username)
                       ? UserRights[username]
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
                Interval = 10 * 1000,
            };
            _apiRightsTimer.Elapsed += UpdateApiRights;
            _apiRightsTimer.Start();
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
                            if (UserRights.ContainsKey(subscriber))
                                UserRights[subscriber] |= GetEffectiveRights(Rights.Subscriber);
                            else
                                UserRights.Add(subscriber, GetEffectiveRights(Rights.Subscriber));
                        }
                    }

                    var followers = Twitch.GetChannelFollowers(channel);

                    lock (UserRights)
                    {
                        foreach (var follower in followers)
                        {
                            if (UserRights.ContainsKey(follower))
                                UserRights[follower] |= GetEffectiveRights(Rights.Follower);
                            else
                                UserRights.Add(follower, GetEffectiveRights(Rights.Follower));
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

        public static void Initialise()
        {
            Start();
        }

        public static void Start() {}

        private static string GetUniqueIdentifier(User u) => $"{u.ChannelName ?? u.Channel.Name}#{u.Username}";

        private static void InitializeUsers()
        {
            SettingsManager
                .GetSettings<ConnectionSettings>()
                .ChannelList
                .AsParallel()
                .SelectMany(UserList.GetUserList)
                .AsParallel()
                .ForAll(AddUser);
        }

        private static void AddUser(User user)
        {
            var identifier = GetUniqueIdentifier(user);
            var ownerName = SettingsManager.GetSettings<ConnectionSettings>().OwnerUsername;

            if (UserRights.ContainsKey(identifier))
                return;

            UserRights.Add(identifier, user.Username == ownerName
                                           ? GetEffectiveRights(Rights.Owner)
                                           : GetEffectiveRights(Rights.Viewer));
        }

        private static void RemoveUser(User user)
        {
            var identifier = GetUniqueIdentifier(user);

            if (UserRights.ContainsKey(identifier))
                UserRights.Remove(identifier);
        }

        private static void AnalyzeMessage(string message)
        {
            // TODO: obviously, implement the real method
            if (message.IndexOf("grant", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                Logger.Debug("found a grant");
            }
        }

        private static void OnUserJoined(object sender, UserJoinEventArgs eventArgs) => AddUser(eventArgs.JoinedUser);

        private static void OnUserParted(object sender, UserPartedEventArgs eventArgs) => RemoveUser(eventArgs.PartedUser);

        private static void OnMessageReceived(object sender, MessageEventArgs eventArgs) => AnalyzeMessage(eventArgs.Message);

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
