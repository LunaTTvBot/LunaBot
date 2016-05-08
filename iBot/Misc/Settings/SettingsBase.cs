using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace IBot.Misc.Settings
{
    public class SettingsBase<T> where T : new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iBot");

        static SettingsBase()
        {
            if (!Directory.Exists(SettingsPath))
                Directory.CreateDirectory(SettingsPath);
        }

        public static T Load(string settingsFileName)
        {
            var p = Path.Combine(SettingsPath, settingsFileName);
            if (!File.Exists(p))
                throw new FileNotFoundException($"File {settingsFileName} not found");                

            using(var file = File.Open(p, FileMode.Open, FileAccess.Read)) {
                try {
                    var serializer = new JsonSerializer();
                    using(var sr = new StreamReader(file))
                    using(var jsonTextReader = new JsonTextReader(sr)) {
                        return serializer.Deserialize<T>(jsonTextReader);
                    }
                } catch(Exception) {
                    return new T();
                }
            }
        }

        public static T LoadLocal(string settingsFileName) {
            if(!File.Exists(settingsFileName))
            {
                var f = File.Create(settingsFileName);
                f.Close();
            }                

            using(var file = File.Open(settingsFileName, FileMode.Open, FileAccess.Read)) {
                try {
                    var serializer = new JsonSerializer();
                    using(var sr = new StreamReader(file))
                    using(var jsonTextReader = new JsonTextReader(sr)) {
                        var d = serializer.Deserialize<T>(jsonTextReader);
                        if(d == null) 
                            throw new ArgumentNullException();

                        return d;
                    }
                } catch(Exception) {
                    return new T();
                }
            }
        }

        public static bool TryLoad(string settingsFileName, out T settings) {
            var p = Path.Combine(SettingsPath, settingsFileName);

            if (!File.Exists(p)) {
                settings = default(T);
                return false;
            }

            try {
                settings = Load(p);
                return true;
            } catch(Exception) {
                settings = default(T);
                return false;
            }
        }

        public bool Save(string settingsFileName) {
            var p = Path.Combine(SettingsPath, settingsFileName);
            try {
                
                if(File.Exists(p))
                    File.Delete(p);

                var json = JsonConvert.SerializeObject(this);

                using(var stream = File.Open(p, FileMode.CreateNew)) {
                    using(var writer = new StreamWriter(stream, new UTF8Encoding(false))) {
                        writer.Write(json.Trim());
                    }
                }

                return true;
            } catch(Exception) {
                return false;
            }
        }
    }
}