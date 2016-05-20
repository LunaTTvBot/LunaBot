using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Ingest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("_default")]
        public bool Default { get; set; }

        [JsonProperty("_id")]
        public int Id { get; set; }

        [JsonProperty("url_template")]
        public string UrlTemplate { get; set; }

        [JsonProperty("availability")]
        public float Availability { get; set; }
    }
}