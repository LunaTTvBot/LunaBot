using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    public class Emoticon
    {
        [JsonProperty("regex")]
        public string Regex { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("emoticon_set")]
        public int EmoticonSet { get; set; }
    }
}