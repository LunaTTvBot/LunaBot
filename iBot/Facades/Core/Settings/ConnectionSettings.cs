using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace IBot.Facades.Core.Settings
{
    public class ConnectionSettings : SettingsBase
    {
        [JsonProperty("channel_list")]
        [Description("settings_description_" + nameof(ChannelList))]
        public List<string> ChannelList { get; set; } = new List<string>();

        [JsonProperty("bot_nickname")]
        [Description("settings_description_" + nameof(BotNickname))]
        public string BotNickname { get; set; } = "";

        [JsonProperty("bot_apikey")]
        [Description("settings_description_" + nameof(BotTwitchApiKey))]
        public string BotTwitchApiKey { internal get; set; } = "";

        [JsonProperty("owner_nickname")]
        [Description("settings_description_" + nameof(OwnerNickname))]
        public string OwnerNickname { get; set; } = "";

        [JsonProperty("owner_apikey")]
        [Description("settings_description_" + nameof(OwnerTwitchApiKey))]
        public string OwnerTwitchApiKey { internal get; set; } = "";
    }
}
