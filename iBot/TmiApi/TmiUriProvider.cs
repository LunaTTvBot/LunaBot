namespace IBot.TmiApi
{
    internal static class TmiUriProvider
    {
        public const string TmiBaseUri = "http://tmi.twitch.tv/";
        private const string ChannelChattersUri = "group/user/{0}/chatters";

        public static string GetChannelChattersUri(string channel)
        {
            return string.Format(ChannelChattersUri, channel);
        }
    }
}