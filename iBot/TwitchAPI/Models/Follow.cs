using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Follow
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("notifications")]
        public bool Notifications { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}