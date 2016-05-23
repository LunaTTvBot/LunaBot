using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Block
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("_id")]
        public int Id { get; set; }
    }
}