namespace IBot.Misc
{
    internal static class GlobalTwitchPatterns
    {
        public const string TwitchUserNamePattern = @"[a-zA-Z0-9][\w]{2,24}";
        public const string TwitchChannelNamePattern = @"[a-zA-Z0-9][\w]{2,24}";
        public const string TwitchHostNamePattern = @"tmi.twitch.tv";
        public const string JtvPattern = @"jtv";
        public const string WritePublicFormat = "PRIVMSG #{0} :{1}";
        public const string WritePrivateFormat = "/w {0} {1}";

        /// <summary>
        /// Group 1: UserName
        /// </summary>
        public const string TwichUserBasePattern =
            ":(" + TwitchUserNamePattern + ")" + 
            "!" + TwitchUserNamePattern + 
            "@" + TwitchUserNamePattern +
            "." + TwitchHostNamePattern + @"\s";
    }
}