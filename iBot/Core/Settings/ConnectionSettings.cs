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

        [JsonProperty("owner_ssl")]
        public bool OwnerSecure { get; set; } = true;

        [JsonProperty("bot_nickname")]
        public string BotNickname { get; set; } = "";

        [JsonProperty("bot_username")]
        public string BotUsername { get; set; } = "";

        [JsonProperty("bot_apikey")]
        public string BotTwitchApiKey { get; set; } = "";

        [JsonProperty("bot_ssl")]
        public bool BotSecure { get; set; } = false;

        [JsonProperty("port")]
        public int Port { get; set; } = 80;

        [JsonProperty("ssl_port")]
        public int SslPort { get; set; } = 443;

        [JsonProperty("url")]
        public string Url { get; set; } = "irc.chat.twitch.tv";

        [JsonProperty("channel_list")]
        public List<string> ChannelList { get; set; } = new List<string>();
    }
}