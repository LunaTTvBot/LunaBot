using System;
using System.Linq;
using IBot.Events.Args.Connections;
using IBot.Facades.Events.Args;
using CoreConnectionType = IBot.Core.ConnectionType;
using CoreManager = IBot.Core.IrcConnectionManager;
using CoreTwitchCaps = IBot.Core.TwitchCaps;
using FacadeConnectionType = IBot.Facades.Core.ConnectionType;
using FacadeTwitchCaps = IBot.Facades.Core.TwitchCaps;

namespace IBot.Facades.Core
{
    public static class ConnectionManager
    {
        public static event EventHandler<ConnectionChangedEventArgs> OnBotConnected;
        public static event EventHandler<ConnectionChangedEventArgs> OnChatConnected;
        public static event EventHandler<ConnectionChangedEventArgs> OnBotDisconnected;
        public static event EventHandler<ConnectionChangedEventArgs> OnChatDisconnected;

        static ConnectionManager()
        {
            CoreManager.RegisterOnConnectedHandler(CoreConnectionType.BotCon, OnBotConnectedHandler);
            CoreManager.RegisterOnConnectedHandler(CoreConnectionType.ChatCon, OnChatConnectedHandler);

            CoreManager.RegisterOnDisconnectedHandler(CoreConnectionType.BotCon, OnBotDisconnectedHandler);
            CoreManager.RegisterOnDisconnectedHandler(CoreConnectionType.ChatCon, OnChatDisconnectedHandler);
        }

        public static bool ConnectAll() => CoreManager.ConnectAll();

        public static void DisconnectAll() => CoreManager.DisconnectAll();

        public static bool RegisterConnection(string user,
                                              string password,
                                              string nickname,
                                              string url,
                                              FacadeTwitchCaps[] caps,
                                              bool secure,
                                              int port,
                                              FacadeConnectionType type)
            => CoreManager.RegisterConnection(user, password, nickname, url, caps.Select(c => (CoreTwitchCaps) c).ToArray(), secure, port, (CoreConnectionType) type);

        private static void OnBotConnectedHandler(object sender, ConnectionEventArgs connectionEventArgs)
        {
            OnBotConnected?.Invoke(null, new ConnectionChangedEventArgs(FacadeConnectionType.BotCon, ConnectionChangeType.Connected));
        }

        private static void OnChatConnectedHandler(object sender, ConnectionEventArgs connectionEventArgs)
        {
            OnChatConnected?.Invoke(null, new ConnectionChangedEventArgs(FacadeConnectionType.ChatCon, ConnectionChangeType.Connected));
        }

        private static void OnBotDisconnectedHandler(object sender, ConnectionEventArgs connectionEventArgs)
        {
            OnBotDisconnected?.Invoke(null, new ConnectionChangedEventArgs(FacadeConnectionType.BotCon, ConnectionChangeType.Disconnected));
        }

        private static void OnChatDisconnectedHandler(object sender, ConnectionEventArgs e)
        {
            OnChatDisconnected?.Invoke(null, new ConnectionChangedEventArgs(FacadeConnectionType.ChatCon, ConnectionChangeType.Disconnected));
        }
    }
}
