using IBot.Facades.Core.Settings;

namespace IBot.Facades.TranslationExtensions
{
    internal static class ConnectionSettingExtensions
    {
        public static ConnectionSettings ToFacadeSettings(this IBot.Core.Settings.ConnectionSettings settings)
        {
            return new ConnectionSettings
            {
                OwnerNickname = settings.OwnerNickname,
                OwnerTwitchApiKey = settings.OwnerTwitchApiKey,
                BotNickname = settings.BotNickname,
                BotTwitchApiKey = settings.BotTwitchApiKey,
                ChannelList = settings.ChannelList
            };
        }
    }
}