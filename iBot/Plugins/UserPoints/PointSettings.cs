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
    }
}
