using System.Collections.Generic;
using Newtonsoft.Json;

namespace IBot.Core.Settings
{
    internal class ConnectionSettings
    {
        [JsonProperty("owner_nickname")]
        public string OwnerNickname { get; set; } = "";

        [JsonProperty("owner_username")]
        public string OwnerUsername { get; set; } = "";

        [JsonProperty("owner_apikey")]
        public string OwnerTwitchApiKey { get; set; } = "";

        [JsonProperty("bot_nickname")]
        public string BotNickname { get; set; } = "";

        [JsonProperty("bot_username")]
        public string BotUsername { get; set; } = "";

        [JsonProperty("bot_apikey")]
        public string BotTwitchApiKey { get; set; } = "";

        [JsonProperty("port")]
        public int Port { get; set; } = 80;

        [JsonProperty("url")]
        public string Url { get; set; } = "irc.chat.twitch.tv";

        [JsonProperty("application_id")]
        public string ApplicationId { get; set; } = "610a6pmvpmv3nmfgpsu8qgkuze09o9n";

        [JsonProperty("channel_list")]
        public List<string> ChannelList { get; set; } = new List<string>();
    }
}