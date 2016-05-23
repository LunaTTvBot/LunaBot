using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Endorse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("user_ids")]
        public int[] UserIds { get; set; }
    }
}