using System;
using IBot.Core;
using IBot.Facades.Events.Args.Channel;

namespace IBot.Facades.Events
{
    public static class ChannelEvents
    {
        static ChannelEvents()
        {
            UserList.UserJoined +=
                (sender, args) => OnUserJoinEvent(new JoinPartEventArgs(args.JoinedChannel.Name, args.JoinedUser.Username, DateTime.Now));

            UserList.UserParted +=
                (sender, args) => OnUserPartEvent(new JoinPartEventArgs(args.PartedChannel.Name, args.PartedUser.Username, DateTime.Now));          
        }

        public static event EventHandler<JoinPartEventArgs> UserJoinEvent;
        private static void OnUserJoinEvent(JoinPartEventArgs e) => UserJoinEvent?.Invoke(typeof(ChannelEvents), e);

        public static event EventHandler<JoinPartEventArgs> UserPartEvent;
        private static void OnUserPartEvent(JoinPartEventArgs e) => UserPartEvent?.Invoke(typeof(ChannelEvents), e);
    }
}