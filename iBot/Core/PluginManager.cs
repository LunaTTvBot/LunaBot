using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IBot.Plugins;
using NLog;

namespace IBot.Core
{
    internal class PluginManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<IPlugin> Plugins = new List<IPlugin>();

        static PluginManager() {
            var consoleAssembly = Assembly.GetExecutingAssembly();
            var pluginTypes = GetTypesByInterface<IPlugin>(consoleAssembly);

            foreach(var pluginType in pluginTypes) {
                try
                {
                    var constructor = pluginType.GetConstructor(Type.EmptyTypes);
                    if (constructor == null) continue;

                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    if(plugin == null)
                        continue;

                    Plugins.Add(plugin);
                    plugin.Init();
                }
                catch(Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        public static bool IsPluginLoaded(string name) => Plugins.Find(p => p.PluginName == name) != null;

        public static List<IPlugin> GetLoadedPlugins() => new List<IPlugin>(Plugins);

        private static IEnumerable<Type> GetTypesByInterface<T>(Assembly assembly) {
            if(!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            return assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(T).Name) != null)
                .ToList();
        }
    }
}