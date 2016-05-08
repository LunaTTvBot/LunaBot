using System;
using System.Diagnostics;
using IBot.Events.CustomEventArgs;

namespace IBot.Misc
{
    public class ConnectionManager
    {
        private static IrcConnection _botConnection;
        // private static IrcConnection ChatterConnection;

        public static event EventHandler<ConnectionEventArgs> BotConnectedEvent;
        public static event EventHandler<ConnectionEventArgs> BotDisconnectedEvent;

        public static void ConnectToBotAccount()
        {
            var settings = SettingsManager.GetConnectionSettings();

            if(_botConnection == null)
            {
                _botConnection = new IrcConnection(
                    settings.Username,
                    settings.TwitchApiKey,
                    settings.Nickname,
                    settings.Url,
                    settings.Port,
                    ConnectionType.BotCon
                );

                _botConnection.RaiseMessageEvent += (sender, args) => Trace.WriteLine(args.Message);
            }

            if (!_botConnection.Connect()) return;
            
            settings.ChannelList.ForEach(channel => _botConnection.Join(channel));
            OnBotConnectedEvent(new ConnectionEventArgs(ConnectionType.BotCon));
        }

        public static void SettingsChanged()
        {
            _botConnection = null;
        }

        public static void DisconnectFromBotAccount()
        {
            if (_botConnection == null || !_botConnection.Connected)
                return;

            _botConnection.Disconnect();
            while (_botConnection.Connected) ;

            OnBotDisconnectedEvent(new ConnectionEventArgs(ConnectionType.BotCon));
        }

        public static bool IsBotConnected()
        {
            return _botConnection.Connected;
        }

        protected static void OnBotConnectedEvent(ConnectionEventArgs e)
        {
            BotConnectedEvent?.Invoke(null, e);
        }

        protected static void OnBotDisconnectedEvent(ConnectionEventArgs e)
        {
            BotDisconnectedEvent?.Invoke(null, e);
        }
    }
}