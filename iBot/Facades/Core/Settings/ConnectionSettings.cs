using System.Collections.Generic;
using Newtonsoft.Json;

namespace IBot.Facades.Core.Settings
{
    public class ConnectionSettings
    {
        [JsonProperty("channel_list")]
        public List<string> ChannelList { get; set; } = new List<string>();

        [JsonProperty("bot_nickname")]
        public string BotNickname { get; set; } = "";

        [JsonProperty("bot_apikey")]
        public string BotTwitchApiKey { internal get; set; } = "";

        [JsonProperty("owner_nickname")]
        public string OwnerNickname { get; set; } = "";

        [JsonProperty("owner_apikey")]
        public string OwnerTwitchApiKey { internal get; set; } = "";
    }
}