using System.Configuration;
using Newtonsoft.Json;

namespace IBot.Plugins.UserAwards
{
    public class AwardSettings : SettingsBase
    {
        [AwardType(AwardType.JoinedChannel)]
        [JsonProperty(PropertyName = "value_joined_channel")]
        public double JoinedChannelValue { get; set; } = 5;

        [AwardType(AwardType.Follows)]
        [JsonProperty(PropertyName = "value_follows")]
        public double FollowsValue { get; set; } = 5;

        [AwardType(AwardType.Subscribed)]
        [JsonProperty(PropertyName = "value_subscribed")]
        public double SubscribedValue { get; set; } = 10;

        [AwardType(AwardType.AccountAge)]
        [JsonProperty(PropertyName = "value_account_age")]
        public double AccountAgeValue { get; set; } = 0.2;

        [AwardType(AwardType.Chatter)]
        [JsonProperty(PropertyName = "value_chatter")]
        public double ChatterValue { get; set; } = 5;

        [AwardType(AwardType.ChatterCommander)]
        [JsonProperty(PropertyName = "value_chatter_commands")]
        public double ChatterCommandsValue { get; set; } = -1;

        [AwardType(AwardType.ChatterSpammer)]
        [JsonProperty(PropertyName = "value_chatter_spammer")]
        public double ChatterSpammerValue { get; set; } = -10;

        [AwardType(AwardType.ChatterEmoji)]
        [JsonProperty(PropertyName = "value_chatter_emoji")]
        public double ChatterEmojiValue { get; set; } = -10;

        [AwardType(AwardType.OftenCensored)]
        [JsonProperty(PropertyName = "value_often_censored")]
        public double OftenCensoredValue { get; set; } = -10;
    }
}
