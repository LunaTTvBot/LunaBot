using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events.Args.User
{
    public class PublicMessageEventArgs
    {
        public PublicMessageEventArgs(string userName, string channel, string message, UserMessageTags tags)
        {
            UserName = userName;
            Channel = channel;
            Message = message;
            Tags = tags;
        }

        public string UserName { get; }
        public string Channel { get; }
        public string Message { get; }
        public UserMessageTags Tags { get; }
    }
}