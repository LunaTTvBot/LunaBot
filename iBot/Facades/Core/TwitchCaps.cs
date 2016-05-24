namespace IBot.Facades.Core
{
    public enum TwitchCaps : short
    {
        Membership = 1 << 0,
        Commands = 1 << 1,
        Tags = 1 << 2,
    }
}