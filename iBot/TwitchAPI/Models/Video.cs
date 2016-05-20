using System;
using Newtonsoft.Json;

namespace IBot.TwitchAPI.Models
{
    internal class Video
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("broadcast_id")]
        public long BroadcastId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("tag_list")]
        public string TagList { get; set; }

        [JsonProperty("recorded_at")]
        public DateTime RecordedAt { get; set; }

        [JsonProperty("game")]
        public object Game { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("broadcast_type")]
        public string BroadcastType { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }
    }
}