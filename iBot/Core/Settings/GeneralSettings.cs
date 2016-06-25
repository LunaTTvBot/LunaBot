using System.Configuration;
using Newtonsoft.Json;

namespace IBot.Core.Settings
{
    internal class GeneralSettings : SettingsBase
    {
        [JsonProperty("user_message_spam_threshold")]
        public int UserMessageSpamThreshold { get; set; } = 6;

        [JsonProperty("user_message_spam_interval")]
        public int UserMessageSpamInterval { get; set; } = 10;

        [JsonProperty("user_emote_spam_threshold")]
        public int UserEmoteSpamThreshold { get; set; } = 6;

        [JsonProperty("user_emote_spam_interval")]
        public int UserEmoteSpamInterval { get; set; } = 10;

        [JsonProperty("user_emote_spam_message_percentage")]
        public double UserEmoteSpamMessagePercentage { get; set; } = 35.0d;

        [JsonProperty("user_emote_spam_message_threshold")]
        public int UserEmoteSpamMessageThreshold { get; set; } = 6;
    }
}
