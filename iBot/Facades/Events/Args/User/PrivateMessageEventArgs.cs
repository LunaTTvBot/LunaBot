using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events.Args.User
{
    public class PrivateMessageEventArgs
    {
        public PrivateMessageEventArgs(string userName, string toUserName, string message,
            UserMessageTags tags)
        {
            UserName = userName;
            ToUserName = toUserName;
            Message = message;
            Tags = tags;
        }

        public string UserName { get; }
        public string ToUserName { get; }
        public string Message { get; }
        public UserMessageTags Tags { get; }
    }
}