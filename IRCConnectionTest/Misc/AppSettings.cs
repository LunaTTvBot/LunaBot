using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IRCConnectionTest.Misc
{
    public class AppSettings
    {
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("apikey")]
        public string TwitchApiKey { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        public static AppSettings Load(string settingsFileName)
        {
            var isoStore = GetIsoStorage();

            if (!isoStore.FileExists(settingsFileName))
                throw new FileNotFoundException(String.Format("File {0} not found", settingsFileName));

            using (var file = isoStore.OpenFile(settingsFileName, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[file.Length];
                file.Read(buffer, 0, buffer.Length);
                var json = Encoding.UTF8.GetString(buffer);

                try
                {
                    return JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch (Exception)
                {
                    return new AppSettings();
                }
            }
        }

        public static AppSettings LoadLocal(string settingsFileName)
        {
            if (!File.Exists(settingsFileName))
                throw new FileNotFoundException(String.Format("File {0} not found", settingsFileName));

            using (var file = File.Open(settingsFileName, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[file.Length];
                file.Read(buffer, 0, buffer.Length);
                var json = Encoding.UTF8.GetString(buffer);

                try
                {
                    return JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch (Exception)
                {
                    return new AppSettings();
                }
            }
        }

        public static bool TryLoad(string settingsFileName, out AppSettings settings)
        {
            if (!IsoStoreFileExists(settingsFileName))
            {
                settings = null;
                return false;
            }

            try
            {
                settings = Load(settingsFileName);
                return true;
            }
            catch (Exception)
            {
                settings = null;
                return false;
            }
        }

        public bool Save(string settingsFileName)
        {
            try
            {
                var isoStore = GetIsoStorage();

                if (isoStore.FileExists(settingsFileName))
                    isoStore.DeleteFile(settingsFileName);

                var json = JsonConvert.SerializeObject(this);

                using (var stream = isoStore.OpenFile(settingsFileName, FileMode.CreateNew))
                {
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    {
                        writer.Write(json.Trim());
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static IsolatedStorageFile GetIsoStorage()
            => IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        private static bool IsoStoreFileExists(string filename)
            => GetIsoStorage().FileExists(filename);
    }
}