using IBot.Facades.Core.Settings;

namespace IBot.Facades.TranslationExtensions
{
    internal static class GeneralSettingsExtension
    {
        public static GeneralSettings ToFacadeSettings(this IBot.Core.Settings.GeneralSettings settings)
        {
            return new GeneralSettings
            {
                UserEmoteSpamInterval = settings.UserEmoteSpamInterval,
                UserEmoteSpamThreshold = settings.UserEmoteSpamThreshold,
                UserEmoteSpamMessagePercentage = settings.UserEmoteSpamMessagePercentage,
                UserEmoteSpamMessageThreshold = settings.UserEmoteSpamMessageThreshold,
                UserMessageSpamInterval = settings.UserMessageSpamInterval,
                UserMessageSpamThreshold = settings.UserMessageSpamThreshold,
            };
        }
    }
}
