using System.Collections.Generic;
using System.Linq;
using IBot.Facades.TranslationExtensions;
using CoreManager = IBot.Core.PluginManager;

namespace IBot.Facades.Core
{
    public class PluginManager
    {
        public IEnumerable<Plugin> GetPlugins() => CoreManager.GetLoadedPlugins().Select(p => p.ToPlugin());

        public bool IsLoaded(string name) => CoreManager.IsPluginLoaded(name);
    }
}
