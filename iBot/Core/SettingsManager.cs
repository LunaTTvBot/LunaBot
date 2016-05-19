using System.Collections.Generic;
using IBot.Core.Settings;

namespace IBot.Core
{
    internal class SettingsManager
    {
        private static readonly Dictionary<string, object> LoadedSettings = new Dictionary<string, object>();

        public static T GetSettings<T>() where T : new()
        {
            T setting;
            var name = typeof(T).Name;

            if (LoadedSettings.ContainsKey(name) && LoadedSettings[name] is T)
                return (T) LoadedSettings[name];

            if (SettingsBase<T>.TryLoad(name, out setting))
            {
                LoadedSettings.Add(name, setting);
                return setting;
            }

            LoadedSettings.Add(name, setting);
            setting = SettingsBase<T>.LoadLocal(name);
            SettingsBase<T>.Save(setting, name);

            return setting;
        }

        public static bool SetSettings<T>(T settings) where T : new()
        {
            var name = typeof(T).Name;
            SettingsBase<T>.Save(settings, name);
            if(!LoadedSettings.ContainsKey(name))
                LoadedSettings.Add(name, settings);

            return SettingsBase<T>.Save(settings, name);
        }
    }
}