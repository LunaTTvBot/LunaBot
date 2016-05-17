﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using IBot.Events;
using IBot.Events.CustomEventArgs;
using IBot.Events.CustomEventArgs.UserList;
using IBot.Misc;
using IBot.TmiApi.ChannelChattersEndpoint;
using NLog;
using Timer = System.Timers.Timer;

namespace IBot
{
    internal static class UserList
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HashSet<Channel> Channels = new HashSet<Channel>();
        private static readonly HashSet<Channel> ApiChannels = new HashSet<Channel>();
        private static Timer _myTimer;

        static UserList()
        {
            var myThread = new Thread(Start);
            myThread.Start();
        }

        public static event EventHandler UserListUpdated;
        public static event EventHandler<UserJoinEventArgs> UserJoined;
        public static event EventHandler<UserPartedEventArgs> UserParted;

        public static void OnUserListUpdated() => UserListUpdated?.Invoke(null, new EventArgs());

        public static void OnUserJoined(UserJoinEventArgs eventArgs) => UserJoined?.Invoke(null, eventArgs);

        public static void OnUserParted(UserPartedEventArgs eventArgs) => UserParted?.Invoke(null, eventArgs);

        private static void Start(object o)
        {
            ConnectionManager.BotConnectedEvent += (s, e) =>
            {
                ChannelEventManager.UserListEvent += AddListUsersToSet;
                ChannelEventManager.UserJoinEvent += AddJoinedUsersToSet;
                ChannelEventManager.UserPartEvent += RemovePartingUsersFromSet;
                UserEventManager.UserPublicMessageEvent += AddChattingUsersToSet;

                if (_myTimer == null)
                {
                    _myTimer = new Timer(60*1000*1); // 1 minute
                    _myTimer.Elapsed += UpdateRegisteredChannelsFromApi;
                    _myTimer.AutoReset = true;
                }

                _myTimer.Enabled = true;

                // get chatters count from tmi
                foreach (var channelName in SettingsManager.GetConnectionSettings().ChannelList)
                {
                    var channel = new Channel(channelName);

                    if (!Channels.Contains(channel))
                        Channels.Add(channel);

                    var chatters = TmiApi.TmiApi.GetChannelChatters(channel.Name);
                    if (chatters?.Count > 400)
                    {
                        /**
                         * If there are more then 400 chatters we request chatters from tmi in 1 minute interval
                         */
                        UseApi(chatters, channel);
                    }
                }
            };

            ConnectionManager.BotDisconnectedEvent += (s, e) =>
            {
                _myTimer.Stop();
                _myTimer = null;
            };
        }

        private static void UseApi(ChannelChatters chatters, Channel channel)
        {
            _logger.Debug("UserList use API#{0} ({1})", channel.Name, chatters.Count);

            if (!ApiChannels.Contains(channel))
                ApiChannels.Add(channel);

            HandleChattersListFromApi(chatters, channel);
        }

        private static void SwitchToApi(Channel channel)
        {
            if (!ApiChannels.Contains(channel))
                ApiChannels.Add(channel);
            else
                return;

            _logger.Debug("UserList switch to API {0}", channel.Users.Count);
        }

        private static void SwitchToEvents(Channel channel)
        {
            if (ApiChannels.Contains(channel))
                ApiChannels.Remove(channel);
            else
                return;

            _logger.Debug("UserList switch to Events {0}", channel.Users.Count);
        }

        private static void HandleChattersListFromApi(ChannelChatters chatters, Channel channel)
        {
            channel.Users.Clear();

            chatters.Chatters.Viewers.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.GlobalMods.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Admins.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Moderators.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Staff.ForEach(chatter => AddToSetFromApi(channel, chatter));

            _logger.Debug("UserList#{0} UPDATED! -> {1}", channel.Name, chatters.Count);

            CheckCount(channel);
        }

        private static void UpdateRegisteredChannelsFromApi(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (ApiChannels)
            {
                foreach (var channel in ApiChannels)
                {
                    var chatters = TmiApi.TmiApi.GetChannelChatters(channel.Name);
                    HandleChattersListFromApi(chatters, channel);
                }
            }
        }

        private static void CheckCount(Channel channel)
        {
            if (channel.Users.Count > 400)
                SwitchToApi(channel);
            else
                SwitchToEvents(channel);
        }

        private static void AddChattingUsersToSet(object sender, UserPublicMessageEventArgs eArgs)
            => AddToSet(GetChannel(eArgs.Channel), eArgs.UserName);

        private static void AddJoinedUsersToSet(object sender, UserEventArgs eArgs)
            => AddToSet(GetChannel(eArgs.Channel), eArgs.UserName);

        private static void AddListUsersToSet(object sender, UserListEventArgs eArgs)
            => eArgs.UserList.ForEach(s => AddToSet(GetChannel(eArgs.Channel), s));

        private static void RemovePartingUsersFromSet(object sender, UserEventArgs eArgs)
        {
            if (ApiChannels.Any(c => c.Name == eArgs.Channel))
                return;

            if (Channels.All(c => c.Name != eArgs.Channel))
                return;

            var channel = Channels.First(c => c.Name == eArgs.Channel);
            var user = channel.Users.FirstOrDefault(u => u.Username == eArgs.UserName);

            // user not found
            if (user == null)
                return;

            channel.Users.Remove(user);
            OnUserParted(new UserPartedEventArgs(user, channel, DateTime.Now));
            OnUserListUpdated();

            _logger.Debug("UserList#{0} UPDATED! -> {1}", eArgs.Channel, channel.Users.Count);
        }

        private static void AddToSetFromApi(Channel channel, string userName)
        {
            if (!Channels.Contains(channel))
                Channels.Add(channel);

            lock (channel)
            {
                if (channel.Users.Any(c => c.Username == userName))
                    return;

                var user = new User(userName)
                {
                    Channel = channel
                };

                channel.Users.Add(user);
                OnUserJoined(new UserJoinEventArgs(user, channel, DateTime.Now));
                OnUserListUpdated();
            }
        }

        private static void AddToSet(Channel channel, string userName)
        {
            if (ApiChannels.Contains(channel))
                return;

            if (!Channels.Contains(channel))
                Channels.Add(channel);

            lock (channel)
            {
                if (channel.Users.Any(c => c.Username == userName))
                {
                    var user = new User(userName)
                    {
                        Channel = channel
                    };

                    channel.Users.Add(user);
                    OnUserJoined(new UserJoinEventArgs(user, channel, DateTime.Now));
                    OnUserListUpdated();
                }
                else
                    return;
            }

            _logger.Debug("UserList#{0} UPDATED! -> {1}", channel.Name, channel.Users.Count);
            CheckCount(channel);
        }

        private static Channel GetChannel(string channelName)
        {
            if (Channels.All(c => c.Name != channelName))
                Channels.Add(new Channel(channelName));

            return Channels.First(c => c.Name == channelName);
        }

        public static IEnumerable<User> GetUserList(string channelName)
            => new List<User>(GetChannel(channelName).Users);

        public static ISet<User> GetUserListAsSet(string channelName)
            => new HashSet<User>(GetChannel(channelName).Users);
    }
}