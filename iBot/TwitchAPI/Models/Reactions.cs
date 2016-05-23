using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Reactions
    {
        [JsonProperty("endorse")]
        public Endorse Endorse { get; set; }
    }
}