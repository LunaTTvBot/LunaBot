using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IBot.Plugins;

namespace IBot.Core
{
    internal class PluginsManager
    {
        private static readonly List<IPlugin> Plugins = new List<IPlugin>();

        static PluginsManager() {
            var consoleAssembly = Assembly.GetExecutingAssembly();
            var pluginTypes = GetTypesByInterface<IPlugin>(consoleAssembly);

            foreach(var pluginType in pluginTypes) {
                var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                if(plugin == null)
                    continue;

                Plugins.Add(plugin);
                plugin.Init();
            }
        }

        public static bool IsPluginLoaded(string name) => Plugins.Find(p => p.PluginName == name) != null;        

        private static IEnumerable<Type> GetTypesByInterface<T>(Assembly assembly) {
            if(!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            return assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(T).Name) != null)
                .ToList();
        }
    }
}