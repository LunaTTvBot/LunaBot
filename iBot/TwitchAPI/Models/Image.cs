using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    public class Image
    {
        [JsonProperty("emoticon_set")]
        public int? EmoticonSet { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}