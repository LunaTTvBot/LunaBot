using IBot.Events.Misc;

namespace IBot.Events.Args.Users
{
    internal class UserWhisperMessageEventArgs
    {
        public UserWhisperMessageEventArgs(UserMessageTags tags, string userName, string toUserName, string message)
        {
            Tags = tags;
            UserName = userName;
            ToUserName = toUserName;
            Message = message;
        }

        public UserMessageTags Tags { get; }
        public string UserName { get; }
        public string ToUserName { get; set; }
        public string Message { get; }
    }
}