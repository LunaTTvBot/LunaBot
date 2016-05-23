using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Top
    {
        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("viewers")]
        public int Viewers { get; set; }

        [JsonProperty("channels")]
        public int Channels { get; set; }
    }
}