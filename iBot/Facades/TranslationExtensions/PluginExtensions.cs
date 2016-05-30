using IBot.Facades.Core;
using IBot.Plugins;

namespace IBot.Facades.TranslationExtensions
{
    public static class PluginExtensions
    {
        internal static Plugin ToPlugin(this IPlugin plugin) => new Plugin(plugin);
    }
}
