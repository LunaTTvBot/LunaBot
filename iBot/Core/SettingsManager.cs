using System.Collections.Generic;
using IBot.Core.Settings;

namespace IBot.Core
{
    internal class SettingsManager
    {
        private static readonly Dictionary<string, object> LoadedSettings = new Dictionary<string, object>();
        private static readonly object SyncLock = new object();

        public static T GetSettings<T>() where T : new()
        {
            T setting;
            var name = typeof(T).Name;

            lock (SyncLock)
            {
                if (LoadedSettings.ContainsKey(name) && LoadedSettings[name] is T)
                    return (T) LoadedSettings[name];

                if (SettingsHelper<T>.TryLoad(name, out setting))
                {
                    LoadedSettings.Add(name, setting);
                    return setting;
                }

                setting = SettingsHelper<T>.LoadLocal(name);
                SettingsHelper<T>.Save(setting, name);
                LoadedSettings.Add(name, setting);
            }

            return setting;
        }

        public static bool SetSettings<T>(T settings) where T : new()
        {
            var name = typeof(T).Name;

            lock (SyncLock)
            {
                SettingsHelper<T>.Save(settings, name);
                if (!LoadedSettings.ContainsKey(name))
                    LoadedSettings.Add(name, settings);
            }

            return SettingsHelper<T>.Save(settings, name);
        }
    }
}