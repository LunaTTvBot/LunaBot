using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events.Args.User
{
    public class PrivateMessageEventArgs
    {
        public PrivateMessageEventArgs(string userName, string toUserName, string message,
            UserMessageTags userMessageTags)
        {
            UserName = userName;
            ToUserName = toUserName;
            Message = message;
            UserMessageTags = userMessageTags;
        }

        public string UserName { get; }
        public string ToUserName { get; }
        public string Message { get; }
        public UserMessageTags UserMessageTags { get; }
    }
}