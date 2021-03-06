using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Token
    {
        [JsonProperty("authorization")]
        public Authorization Authorization { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("valid")]
        public bool Valid { get; set; }
    }
}