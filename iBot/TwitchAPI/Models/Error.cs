using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string Type { get; set; }
    }
}
