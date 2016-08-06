using System;
using System.ComponentModel;

namespace IBot.Plugins.UserAwards
{
    internal enum AwardType : Int32
    {
        [Description("Joined Channel")]
        JoinedChannel,

        [Description("Follows Channel")]
        Follows,

        [Description("Subscribed to Channel")]
        Subscribed,

        [Description("Account Age")]
        AccountAge,

        [Description("Chatter")]
        Chatter,

        [Description("Command abuser")]
        ChatterCommander,

        [Description("Spammer")]
        ChatterSpammer,

        [Description("Emojii Abuser")]
        ChatterEmoji,

        [Description("Often Censored")]
        OftenCensored,
    }
}
