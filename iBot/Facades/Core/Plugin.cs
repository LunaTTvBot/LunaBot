using IBot.Plugins;

namespace IBot.Facades.Core
{
    public class Plugin
    {
        private IPlugin BasePlugin { get; }

        internal Plugin(IPlugin plugin)
        {
            BasePlugin = plugin;
        }

        public string PluginName => BasePlugin.PluginName;

        public void Init() => BasePlugin.Init();
    }
}
