using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IBot.Plugins;

namespace IBot.Misc
{
    internal static class PluginManager
    {
        private static readonly List<IPlugin> Plugins = new List<IPlugin>();

        static PluginManager()
        {
            var consoleAssembly = Assembly.GetExecutingAssembly();
            var pluginTypes = GetTypesByInterface<IPlugin>(consoleAssembly);

            foreach(var pluginType in pluginTypes) {
                var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                if (plugin == null) continue;

                Plugins.Add(plugin);
                plugin.Init();
            }
        }

        //public static IPlugin GetPlugin(string name)
        //{
        //    return Plugins.Find(p => p.PluginName == name);
        //}

        /// <summary>
        /// Will call BindEvents Method of all classes that implement the IPlugin interface
        /// </summary>
        public static void BindEvents() 
        {
            Plugins.ForEach(p => p.BindEvents());
        }

        private static IEnumerable<Type> GetTypesByInterface<T>(Assembly assembly) {
            if(!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            return assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(T).Name) != null)
                .ToList();
        }
    }
}