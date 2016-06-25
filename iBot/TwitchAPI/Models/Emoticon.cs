using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    public class Emoticon
    {
        [JsonProperty("regex")]
        public string Regex { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }
    }
}
