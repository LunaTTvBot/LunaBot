using System.Collections.Generic;
using Newtonsoft.Json;

namespace IBot.Misc.Settings
{
    public class ConnectionSettings : SettingsBase<ConnectionSettings>
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; } = "";

        [JsonProperty("port")]
        public int Port { get; set; } = 80;

        [JsonProperty("apikey")]
        public string TwitchApiKey { get; set; } = "";

        [JsonProperty("url")]
        public string Url { get; set; } = "irc.chat.twitch.tv";

        [JsonProperty("username")]
        public string Username { get; set; } = "";

        [JsonProperty("channel_list")]
        public List<string> ChannelList { get; set; } = new List<string>();
    }
}