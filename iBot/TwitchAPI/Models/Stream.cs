using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Stream
    {
        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("viewers")]
        public int Viewers { get; set; }

        [JsonProperty("average_fps")]
        public float AverageFps { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("video_height")]
        public int VideoHeight { get; set; }

        [JsonProperty("is_playlist")]
        public bool IsPlaylist { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("_id")]
        public long Id { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("preview")]
        public Preview Preview { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }
}