using System.Linq;
using IBot.Misc.Settings;

namespace IBot.Misc
{
    public class SettingsManager
    {
        private const string ConnectionSettigsFile = "ConnectionSettings.json";
        private static ConnectionSettings _connectionSettings;

        private static readonly object SyncLock = new object();

        public static ConnectionSettings GetConnectionSettings()
        {
            if (_connectionSettings != null) return _connectionSettings;

            lock (SyncLock)
            {                            
                if (ConnectionSettings.TryLoad(ConnectionSettigsFile, out _connectionSettings))
                    return _connectionSettings;

                _connectionSettings = ConnectionSettings.LoadLocal(ConnectionSettigsFile);
                _connectionSettings.Save(ConnectionSettigsFile);
            }

            return _connectionSettings;
        }

        public static void SaveConnectionSettings(string nickname, string apikey, string[] channelList)
        {
            GetConnectionSettings();

            _connectionSettings.Username = nickname;
            _connectionSettings.Nickname = nickname.ToLower();
            _connectionSettings.TwitchApiKey = apikey;
            _connectionSettings.ChannelList = channelList.Select(c => c).ToList();
            _connectionSettings.Save(ConnectionSettigsFile);

            _connectionSettings = null;
        }
    }
}