using IBot.Facades.TranslationExtensions;
using CorePlugin = IBot.Plugins.UserPoints.UserPointPlugin;
using FacadeUser = IBot.Facades.Core.User;

namespace IBot.Facades.Plugins.Points
{
    public class UserPoints
    {
        public long GetPoints(FacadeUser user) => CorePlugin.GetPoints(user.ToCoreUser());

        public bool UserHasPoints(FacadeUser user, long amount) => CorePlugin.UserHasPoints(user.ToCoreUser(), amount);

        public void AddPoints(FacadeUser user, long amount) => CorePlugin.AddPoints(user.ToCoreUser(), amount);

        public bool RemovePoints(FacadeUser user, long amount) => CorePlugin.RemovePoints(user.ToCoreUser(), amount);
    }
}
