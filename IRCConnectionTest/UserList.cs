using System.Collections.Generic;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.ComstumEventArgs;

namespace IRCConnectionTest
{
    internal static class UserList
    {
        private static readonly Dictionary<string, HashSet<string>> UsrList = new Dictionary<string, HashSet<string>>();

        static UserList()
        {
            ChannelEventManager.UserListEvent += ChannelEventManagerOnUserListEvent;
            ChannelEventManager.UserJoinEvent += ChannelEventManagerOnUserJoinEvent;
            ChannelEventManager.UserPartEvent += ChannelEventManagerOnUserPartEvent;
            UserEventManager.UserPublicMessageEvent += UserEventManagerOnUserPublicMessageEvent;
        }

        private static void UserEventManagerOnUserPublicMessageEvent(object sender, UserPublicMessageEventArgs eArgs)
            => AddToSet(eArgs.Channel, eArgs.UserName);

        private static void ChannelEventManagerOnUserJoinEvent(object sender, UserEventArgs eArgs)
            => AddToSet(eArgs.Channel, eArgs.UserName);

        private static void ChannelEventManagerOnUserListEvent(object sender, UserListEventArgs eArgs)
            => eArgs.UserList.ForEach(s => AddToSet(eArgs.Channel, s));

        private static void ChannelEventManagerOnUserPartEvent(object sender, UserEventArgs eArgs)
        {
            if (!UsrList.ContainsKey(eArgs.Channel)) return;
            if (!UsrList[eArgs.Channel].Contains(eArgs.UserName)) return;
            UsrList[eArgs.Channel].Remove(eArgs.UserName);
        }

        private static void AddToSet(string channel, string userName)
        {
            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<string>());

            if (!UsrList[channel].Contains(userName))
                UsrList[channel].Add(userName);
        }

        public static HashSet<string> GetUserList(string channel)
        {
            if (!UsrList.ContainsKey(channel))
                UsrList.Add(channel, new HashSet<string>());

            return UsrList[channel];
        }
    }
}