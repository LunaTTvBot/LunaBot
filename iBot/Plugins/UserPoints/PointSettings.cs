using Newtonsoft.Json;

namespace IBot.Plugins.UserPoints
{
    internal class PointSettings
    {
        [JsonProperty("start_on_application_startup")]
        public bool StartOnApplicationStartup { get; set; } = false;

        [JsonProperty("name_singular")]
        public string PointNameSingular { get; set; } = "Point";

        [JsonProperty("name_plural")]
        public string PointNamePlural { get; set; } = "Points";

        [JsonProperty("award_interval_seconds")]
        public long PointAwardIntervalSeconds { get; set; } = 60;

        [JsonProperty("award_interval_amount")]
        public long PointsAwardedPerInterval { get; set; } = 1;

        [JsonProperty("multiplier_owner")]
        public long PointsMultiplierOwner { get; set; } = 4;

        [JsonProperty("multiplier_moderator")]
        public long PointsMultiplierMod { get; set; } = 2;

        [JsonProperty("multiplier_subscriber")]
        public long PointsMultiplierSub { get; set; } = 1;

        [JsonProperty("multiplier_follower")]
        public long PointsMultiplierFollower { get; set; } = 1;

        [JsonProperty("multiplier_viewer")]
        public long PointsMultiplierViewer { get; set; } = 1;

        [JsonProperty("command_regex")]
        public string CommandRegex { get; set; } = "Points";
    }
}
