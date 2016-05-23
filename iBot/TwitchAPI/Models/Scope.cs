using System;
using System.ComponentModel;

namespace IBot.TwitchAPI.Models
{
    [Flags]
    public enum Scope : Int32
    {
        [ScopeDefinition("user_read")]
        [Description("Read access to non-public user information, such as email address.")]
        UserRead = 1 << 0,

        [ScopeDefinition("user_blocks_edit")]
        [Description("Ability to ignore or unignore on behalf of a user.")]
        UserBlocksEdit = 1 << 1,

        [ScopeDefinition("user_blocks_read")]
        [Description("Read access to a user's list of ignored users.")]
        UserBlocksRead = 1 << 2,

        [ScopeDefinition("user_follows_edit")]
        [Description("Access to manage a user's followed channels.")]
        UserFollowsEdit = 1 << 3,

        [ScopeDefinition("channel_read")]
        [Description("Read access to non-public channel information, including email address and stream key.")]
        ChannelRead = 1 << 4,

        [ScopeDefinition("channel_editor")]
        [Description("Write access to channel metadata (game, status, etc).")]
        ChannelEditor = 1 << 5,

        [ScopeDefinition("channel_commercial")]
        [Description("Access to trigger commercials on channel.")]
        ChannelCommercial = 1 << 6,

        [ScopeDefinition("channel_stream")]
        [Description("Ability to reset a channel's stream key.")]
        ChannelStream = 1 << 7,

        [ScopeDefinition("channel_subscriptions")]
        [Description("Read access to all subscribers to your channel.")]
        ChannelSubscriptions = 1 << 8,

        [ScopeDefinition("user_subscriptions")]
        [Description("Read access to subscriptions of a user.")]
        UserSubscriptions = 1 << 9,

        [ScopeDefinition("channel_check_subscription")]
        [Description("Read access to check if a user is subscribed to your channel.")]
        ChannelCheckSubscription = 1 << 10,

        [ScopeDefinition("chat_login")]
        [Description("Ability to log into chat and send messages.")]
        ChatLogin = 1 << 11,

        [ScopeDefinition("channel_feed_read")]
        [Description("Ability to view to a channel feed.")]
        ChannelFeedEdit = 1 << 13,

        [ScopeDefinition("channel_feed_edit")]
        [Description("Ability to add posts and reactions to a channel feed.")]
        ChannelFeedRead = 1 << 12,
    }
}
