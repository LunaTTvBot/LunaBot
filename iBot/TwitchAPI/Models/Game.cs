using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Game
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("box")]
        public Box Box { get; set; }

        [JsonProperty("logo")]
        public Logo Logo { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("_id")]
        public int Id { get; set; }

        [JsonProperty("giantbomb_id")]
        public int GiantbombId { get; set; }
    }
}