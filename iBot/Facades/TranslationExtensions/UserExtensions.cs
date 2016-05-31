namespace IBot.Facades.TranslationExtensions
{
    internal static class UserExtensions
    {
        public static IBot.Facades.Core.User ToFacadeUser(this IBot.Models.User user)
        {
            return new IBot.Facades.Core.User(user.Username, user.ChannelName ?? user.Channel.Name);
        }

        public static IBot.Models.User ToCoreUser(this IBot.Facades.Core.User user)
        {
            return new IBot.Models.User(user.Name) { ChannelName = user.Channel };
        }
    }
}
