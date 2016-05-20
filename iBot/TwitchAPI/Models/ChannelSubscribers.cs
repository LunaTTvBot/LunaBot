using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class ChannelSubscribers
    {
        [JsonProperty("follows")]
        public Follow[] Follows { get; set; }

        [JsonProperty("_total")]
        public int Total { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("_cursor")]
        public string Cursor { get; set; }
    }
}