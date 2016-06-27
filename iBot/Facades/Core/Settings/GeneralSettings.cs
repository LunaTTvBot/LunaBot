using System.ComponentModel;
using Newtonsoft.Json;

namespace IBot.Facades.Core.Settings
{
    public class GeneralSettings : SettingsBase
    {
        [JsonProperty("user_message_spam_threshold")]
        [Description("settings_description_" + nameof(UserMessageSpamThreshold))]
        public int UserMessageSpamThreshold { get; set; }

        [JsonProperty("user_message_spam_interval")]
        [Description("settings_description_" + nameof(UserMessageSpamInterval))]
        public int UserMessageSpamInterval { get; set; }

        [JsonProperty("user_emote_spam_threshold")]
        [Description("settings_description_" + nameof(UserEmoteSpamThreshold))]
        public int UserEmoteSpamThreshold { get; set; }

        [JsonProperty("user_emote_spam_interval")]
        [Description("settings_description_" + nameof(UserEmoteSpamInterval))]
        public int UserEmoteSpamInterval { get; set; }

        [JsonProperty("user_emote_spam_message_percentage")]
        [Description("settings_description_" + nameof(UserEmoteSpamMessagePercentage))]
        public double UserEmoteSpamMessagePercentage { get; set; }

        [JsonProperty("user_emote_spam_message_threshold")]
        [Description("settings_description_" + nameof(UserEmoteSpamMessageThreshold))]
        public int UserEmoteSpamMessageThreshold { get; set; }
    }
}
