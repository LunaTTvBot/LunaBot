using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Newtonsoft.Json;

namespace IBot.Core
{
    public class AppSettings
    {
        [JsonProperty("owner_nickname")]
        public string OwnerNickname { get; set; }

        [JsonProperty("owner_username")]
        public string OwnerUsername { get; set; }

        [JsonProperty("owner_apikey")]
        public string OwnerTwitchApiKey { get; set; }

        [JsonProperty("bot_nickname")]
        public string BotNickname { get; set; }

        [JsonProperty("bot_username")]
        public string BotUsername { get; set; }

        [JsonProperty("bot_apikey")]
        public string BotTwitchApiKey { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("channel_list")]
        public List<string> ChannelList { get; set; }

        public static AppSettings Load(string settingsFileName)
        {
            var isoStore = GetIsoStorage();

            if (!isoStore.FileExists(settingsFileName))
                throw new FileNotFoundException($"File {settingsFileName} not found");

            using (var file = isoStore.OpenFile(settingsFileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var serializer = new JsonSerializer();
                    using(var sr = new StreamReader(file))
                    using(var jsonTextReader = new JsonTextReader(sr)) {
                        return serializer.Deserialize<AppSettings>(jsonTextReader);
                    }
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
                throw new FileNotFoundException($"File {settingsFileName} not found");

            using (var file = File.Open(settingsFileName, FileMode.Open, FileAccess.Read))
            {
                try {
                    var serializer = new JsonSerializer();
                    using(var sr = new StreamReader(file))
                    using(var jsonTextReader = new JsonTextReader(sr)) {
                        return serializer.Deserialize<AppSettings>(jsonTextReader);
                    }
                } catch(Exception) {
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