using System;

namespace IBot.Events.Args.UserList
{
    internal class UserJoinEventArgs
    {
        public UserJoinEventArgs(User user, Channel channel, DateTime time)
        {
            JoinedUser = user;
            JoinedChannel = channel;
            JoinTime = time;
        }

        public User JoinedUser { get; }
        public Channel JoinedChannel { get; }
        public DateTime JoinTime { get; }
    }
}
