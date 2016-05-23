using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events.Args.User
{
    public class UserStateEventArgs
    {
        public UserStateEventArgs(string channel, UserStateTags userStateTags)
        {
            Channel = channel;
            UserStateTags = userStateTags;
        }

        public string Channel { get; set; }
        public UserStateTags UserStateTags { get; set; }
    }
}