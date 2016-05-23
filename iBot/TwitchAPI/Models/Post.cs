using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Post
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("emotes")]
        public object[] Emotes { get; set; }

        [JsonProperty("reactions")]
        public Reactions Reactions { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}