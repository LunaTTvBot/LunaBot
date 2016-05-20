using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Links
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("follows")]
        public string Follows { get; set; }

        [JsonProperty("commercial")]
        public string Commercial { get; set; }

        [JsonProperty("stream_key")]
        public string StreamKey { get; set; }

        [JsonProperty("chat")]
        public string Chat { get; set; }

        [JsonProperty("features")]
        public string Features { get; set; }

        [JsonProperty("subscriptions")]
        public string Subscriptions { get; set; }

        [JsonProperty("editors")]
        public string Editors { get; set; }

        [JsonProperty("teams")]
        public string Teams { get; set; }

        [JsonProperty("videos")]
        public string Videos { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("users")]
        public string Users { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("channels")]
        public string Channels { get; set; }

        [JsonProperty("streams")]
        public string Streams { get; set; }

        [JsonProperty("ingests")]
        public string Ingests { get; set; }

        [JsonProperty("search")]
        public string Search { get; set; }
    }
}