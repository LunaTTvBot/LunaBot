using IRCConnectionTest.Events.Misc;

namespace IRCConnectionTest.Events.ComstumEventArgs
{
    internal class UserStateEventArgs
    {
        public UserStateEventArgs(UserStateTags tags, string channel)
        {
            Tags = tags;
            Channel = channel;
        }

        public UserStateTags Tags { get; }
        public string Channel { get; }
    }
}