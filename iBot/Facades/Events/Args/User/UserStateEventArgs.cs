using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events.Args.User
{
    public class UserStateEventArgs
    {
        public UserStateEventArgs(string channel, UserStateTags tags)
        {
            Channel = channel;
            Tags = tags;
        }

        public string Channel { get; set; }
        public UserStateTags Tags { get; set; }
    }
}