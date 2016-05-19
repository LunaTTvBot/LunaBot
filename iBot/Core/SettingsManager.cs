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
            var name = typeof(T).ToString();

            if (LoadedSettings.ContainsKey(name) && LoadedSettings[name] is T)
                return (T) LoadedSettings[name];

            if (SettingsBase<T>.TryLoad(name, out setting))
            {
                LoadedSettings.Add(name, setting);
                return setting;
            }

            LoadedSettings.Add(name, setting);
            setting = SettingsBase<T>.LoadLocal(name);
            var set = setting as SettingsBase<T>;
            set?.Save(name);            

            return setting;
        }
    }
}