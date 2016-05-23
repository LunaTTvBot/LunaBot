using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class User
    {
        [JsonProperty("_id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}