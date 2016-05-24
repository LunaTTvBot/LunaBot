using CoreManager = IBot.Core.PermissionManager;
using CoreUser = IBot.Models.User;
using CoreRights = IBot.Core.Rights;
using FacadeUser = IBot.Facades.Core.User;
using FacadeRights = IBot.Facades.Core.Rights;

namespace IBot.Facades.Core
{
    /**
     * IBot.Core.Rights and IBot.Facades.Core.Rights have the same internal layout, 
     * and as longs as that is the case we can simply cast them around
     */

    public static class PermissionManager
    {
        public static FacadeRights GetRights(string username) => (FacadeRights) CoreManager.GetRights(username);

        public static FacadeRights GetRights(FacadeUser u) => (FacadeRights) CoreManager.GetRights(u.Name);

        public static FacadeRights GetEffectiveRights(FacadeRights rights) => (FacadeRights) CoreManager.GetEffectiveRights((CoreRights) rights);
    }
}
