using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Linq;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.ComstumEventArgs;
using IRCConnectionTest.Misc;
using IRCConnectionTest.TmiApi.ChannelChattersEndpoint;
using Timer = System.Timers.Timer;

namespace IRCConnectionTest
{
    internal static class UserList
    {
        private static readonly HashSet<Channel> Channels = new HashSet<Channel>();
        private static readonly HashSet<Channel> ApiChannels = new HashSet<Channel>();
        private static Timer _myTimer;

        static UserList()
        {
            var myThread = new Thread(Start);
            myThread.Start();
        }

        private static void Start(object o)
        {

            ChannelEventManager.UserListEvent += ChannelEventManagerOnUserListEvent;
            ChannelEventManager.UserJoinEvent += ChannelEventManagerOnUserJoinEvent;
            ChannelEventManager.UserPartEvent += ChannelEventManagerOnUserPartEvent;
            UserEventManager.UserPublicMessageEvent += UserEventManagerOnUserPublicMessageEvent;

            if (_myTimer == null)
            {
                _myTimer = new Timer(60 * 1000 * 1); // 1 minute
                _myTimer.Elapsed += MyTimerOnElapsed;
                _myTimer.AutoReset = true;
            }

            _myTimer.Enabled = true;

            // get chatters count from tmi
            foreach (var channelName in App.BotChannelList)
            {
                var channel = Channel.Get(channelName);

                if (!Channels.Contains(channel))
                    Channels.Add(channel);

                var chatters = TmiApi.TmiApi.GetChannelChatters(channel.Name);
                if (chatters.Count > 400)
                {
                    /**
                     * If there are more then 400 chatters we request chatters from tmi in 1 minute interval
                     */
                    UseApi(chatters, channel);
                }
            }
        }

        private static void UseApi(ChannelChatters chatters, Channel channel)
        {
            Logger.Write($"UserList use API#{channel.Name} ({chatters.Count})");

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

            Logger.Write($"-- UserList switch to API {channel.Users.Count}");
        }

        private static void SwitchToEvents(Channel channel)
        {
            if (ApiChannels.Contains(channel))
                ApiChannels.Remove(channel);
            else
                return;

            Logger.Write($"-- UserList switch to Events {channel.Users.Count}");
        }

        private static void HandleChattersListFromApi(ChannelChatters chatters, Channel channel)
        {
            chatters.Chatters.Viewers.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.GlobalMods.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Admins.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Moderators.ForEach(chatter => AddToSetFromApi(channel, chatter));
            chatters.Chatters.Staff.ForEach(chatter => AddToSetFromApi(channel, chatter));

            Logger.Write($"-- UserList#{channel} UPDATED! -> {chatters.Count}");

            CheckCount(channel);
        }

        private static void MyTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var channel in ApiChannels)
            {
                var chatters = TmiApi.TmiApi.GetChannelChatters(channel.Name);
                HandleChattersListFromApi(chatters, channel);
            }
        }

        private static void CheckCount(Channel channel)
        {
            if (channel.Users.Count > 400)
                SwitchToApi(channel);
            else
                SwitchToEvents(channel);
        }

        private static void UserEventManagerOnUserPublicMessageEvent(object sender, UserPublicMessageEventArgs eArgs)
            => AddToSet(GetChannel(eArgs.Channel), eArgs.UserName);

        private static void ChannelEventManagerOnUserJoinEvent(object sender, UserEventArgs eArgs)
            => AddToSet(GetChannel(eArgs.Channel), eArgs.UserName);

        private static void ChannelEventManagerOnUserListEvent(object sender, UserListEventArgs eArgs)
            => eArgs.UserList.ForEach(s => AddToSet(GetChannel(eArgs.Channel), s));

        private static void ChannelEventManagerOnUserPartEvent(object sender, UserEventArgs eArgs)
        {
            if (ApiChannels.Any(c=>c.Name == eArgs.Channel))
                return;

            if (Channels.Any(c => c.Name == eArgs.Channel))
                return;

            var channel = Channels.First(c => c.Name == eArgs.Channel);
            var user = channel.Users.FirstOrDefault(u => u.Username == eArgs.UserName);
            
            // user not found
            if (user == null)
                return;

            channel.Users.Remove(user);

            Logger.Write($"-- UserList#{eArgs.Channel} UPDATED! -> {channel.Users.Count}");
        }

        private static void AddToSetFromApi(Channel channel, string userName)
        {
            if (!Channels.Contains(channel))
                Channels.Add(channel);

            lock (channel)
            {
                if (channel.Users.Any(c => c.Username == userName))
                    return;

                var user = new User(userName);
                channel.Users.Add(user);
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
                    var user = new User(userName);
                    channel.Users.Add(user);
                }
                else
                    return;
            }

            Logger.Write($"-- UserList#{channel} UPDATED! -> {channel.Users.Count}");
            CheckCount(channel);
        }

        private static Channel GetChannel(string channelName)
        {
            if (!Channels.Any(c => c.Name == channelName))
                Channels.Add(Channel.Get(channelName));

            return Channels.First(c => c.Name == channelName);
        }

        public static IEnumerable<User> GetUserList(string channelName) 
            => new List<User>(GetChannel(channelName).Users);

        public static ISet<User> GetUserListAsSet(string channelName) 
            => new HashSet<User>(GetChannel(channelName).Users);
    }
}
