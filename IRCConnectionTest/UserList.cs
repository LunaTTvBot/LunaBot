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
        private static readonly Dictionary<string, HashSet<User>> UsrList = new Dictionary<string, HashSet<User>>();
        private static Timer _myTimer;
        private static readonly HashSet<string> ApiChannels = new HashSet<string>();

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
            foreach (var channel in App.BotChannelList)
            {
                if (!UsrList.ContainsKey(channel))
                    UsrList.Add(channel, new HashSet<User>());

                var chatters = TmiApi.TmiApi.GetChannelChatters(channel);
                if (chatters.Count > 400)
                {
                    /**
                     * If there are more then 400 chatters we request chatters from tmi in 1 minute interval
                     */
                    UseApi(chatters, channel);
                }
            }
        }

        private static void UseApi(ChannelChatters chatters, string channel)
        {
            Logger.Write($"UserList use API#{channel} ({chatters.Count})");

            if (!ApiChannels.Contains(channel))
                ApiChannels.Add(channel);

            HandleChattersListFromApi(chatters, channel);
        }

        private static void SwitchToApi(string channel)
        {
            if (!ApiChannels.Contains(channel))
                ApiChannels.Add(channel);
            else
                return;

            Logger.Write($"-- UserList switch to API {UsrList[channel].Count}");
        }

        private static void SwitchToEvents(string channel)
        {
            if (ApiChannels.Contains(channel))
                ApiChannels.Remove(channel);
            else
                return;

            Logger.Write($"-- UserList switch to Events {UsrList[channel].Count}");
        }

        private static void HandleChattersListFromApi(ChannelChatters chatters, string channel)
        {
            UsrList[channel].Clear();

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
                var chatters = TmiApi.TmiApi.GetChannelChatters(channel);
                HandleChattersListFromApi(chatters, channel);
            }
        }

        private static void CheckCount(string channel)
        {
            if (UsrList[channel].Count > 400)
                SwitchToApi(channel);
            else
                SwitchToEvents(channel);
        }

        private static void UserEventManagerOnUserPublicMessageEvent(object sender, UserPublicMessageEventArgs eArgs)
            => AddToSet(eArgs.Channel, eArgs.UserName);

        private static void ChannelEventManagerOnUserJoinEvent(object sender, UserEventArgs eArgs)
            => AddToSet(eArgs.Channel, eArgs.UserName);

        private static void ChannelEventManagerOnUserListEvent(object sender, UserListEventArgs eArgs)
            => eArgs.UserList.ForEach(s => AddToSet(eArgs.Channel, s));

        private static void ChannelEventManagerOnUserPartEvent(object sender, UserEventArgs eArgs)
        {
            if (ApiChannels.Contains(eArgs.Channel)
                || !UsrList.ContainsKey(eArgs.Channel))
                return;

            var user = UsrList[eArgs.Channel].FirstOrDefault(u => u.Username == eArgs.UserName);

            // user not found
            if (user == null)
                return;

            UsrList[eArgs.Channel].Remove(user);
            DatabaseContext.Get().Users.Remove(user);

            Logger.Write($"-- UserList#{eArgs.Channel} UPDATED! -> {UsrList[eArgs.Channel].Count}");
        }

        private static void AddUser(string channel, User user)
        {
            UsrList[channel].Add(user);
            var db = DatabaseContext.Get();

            lock (db.Users)
            {
                db.Users.Add(user);
            }
        }

        private static void AddToSetFromApi(string channel, string userName)
        {
            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<User>());

            if (!UsrList[channel].Any(u => u.Username == userName))
                AddUser(channel, new User { Username = userName });
        }

        private static void AddToSet(string channel, string userName)
        {
            if (ApiChannels.Contains(channel))
                return;

            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<User>());

            if (!UsrList[channel].Any(u => u.Username == userName))
                AddUser(channel, new User { Username = userName });
            else
                return;

            Logger.Write($"-- UserList#{channel} UPDATED! -> {UsrList[channel].Count}");
            CheckCount(channel);
        }

        public static IEnumerable<User> GetUserList(string channel)
        {
            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<User>());

            return new HashSet<User>(UsrList[channel]);
        }

        public static ISet<User> GetUserListAsSet(string channel)
        {
            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<User>());

            return new HashSet<User>(UsrList[channel]);
        }
    }
}
