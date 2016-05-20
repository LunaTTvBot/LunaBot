using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Authorization
    {
        [JsonProperty("scopes")]
        public string[] Scopes { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}