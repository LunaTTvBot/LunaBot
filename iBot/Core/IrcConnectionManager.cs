using System;
using System.Collections.Generic;
using System.Linq;
using IBot.Events.Args.Connections;
using IBot.Events.Args.Users;
using NLog;

namespace IBot.Core
{
    internal enum TwitchCaps 
    {
        Membership,
        Commands,
        Tags
    }

    internal static class IrcConnectionManager
    {
        private static readonly Dictionary<ConnectionType, IrcConnection> Connections =
            new Dictionary<ConnectionType, IrcConnection>();

        private static readonly Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>>
            ConnectionMessageHandlers =
                new Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>>();

        private static readonly Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>>
            ConnectionConnectedHandlers =
                new Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>>();

        private static readonly Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>>
            ConnectionDisconnectedHandlers =
                new Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IrcConnection GetConnection(ConnectionType type) => Connections.ContainsKey(type)
            ? Connections[type]
            : null;

        public static bool RegisterConnection(string user, string password, string nickname, string url, TwitchCaps[] caps, bool secure, int port,
            ConnectionType type)
        {
            try
            {
                var connection = new IrcConnection(user, password, nickname, url, port, type, caps, secure, 1500);

                Connections[type] = connection;

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }

        public static void RemoveMessageHandler(ConnectionType type, EventHandler<MessageEventArgs> handler)
        {
            if (!ConnectionMessageHandlers.ContainsKey(type))
                return;

            if (!ConnectionMessageHandlers[type].Contains(handler))
                return;

            ConnectionMessageHandlers[type].Remove(handler);

            if (Connections.ContainsKey(type))
            {
                Connections[type].RaiseMessageEvent -= handler;
            }
        }

        public static void RegisterMessageHandler(ConnectionType type, EventHandler<MessageEventArgs> handler)
        {
            if (!ConnectionMessageHandlers.ContainsKey(type))
                ConnectionMessageHandlers.Add(type, new List<EventHandler<MessageEventArgs>>());

            // don't register methods twice
            if (ConnectionMessageHandlers[type].Contains(handler))
                return;

            ConnectionMessageHandlers[type].Add(handler);

            if (Connections.ContainsKey(type))
            {
                Connections[type].RaiseMessageEvent += handler;
            }
        }

        public static void RemoveOnConnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler)
        {
            if (!ConnectionConnectedHandlers.ContainsKey(type))
                return;

            if (ConnectionConnectedHandlers[type].Contains(handler))
            {
                ConnectionConnectedHandlers[type].Remove(handler);
            }
        }

        public static void RegisterOnConnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler)
        {
            if (!ConnectionConnectedHandlers.ContainsKey(type))
                ConnectionConnectedHandlers.Add(type, new List<EventHandler<ConnectionEventArgs>>());

            // don't register methods twice
            if (ConnectionConnectedHandlers[type].Contains(handler))
                return;

            ConnectionConnectedHandlers[type].Add(handler);
        }

        public static void RemoveOnDisconnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler) {
            if(!ConnectionDisconnectedHandlers.ContainsKey(type))
                return;

            if(ConnectionDisconnectedHandlers[type].Contains(handler)) {
                ConnectionDisconnectedHandlers[type].Remove(handler);
            }
        }

        public static void RegisterOnDisconnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler) {
            if(!ConnectionDisconnectedHandlers.ContainsKey(type))
                ConnectionDisconnectedHandlers.Add(type, new List<EventHandler<ConnectionEventArgs>>());

            // don't register methods twice
            if(ConnectionDisconnectedHandlers[type].Contains(handler))
                return;

            ConnectionDisconnectedHandlers[type].Add(handler);
        }

        public static bool ReconnectAll()
        {
            DisconnectAll();
            return ConnectAll();
        }

        public static bool ConnectAll()
        {
            return Connections.All(kvp =>
            {
                var type = kvp.Key;
                var con = kvp.Value;

                if (ConnectionMessageHandlers.ContainsKey(type))
                    ConnectionMessageHandlers[type].ForEach(e => con.RaiseMessageEvent += e);

                if (!con.Connect())
                    return false;

                if (ConnectionConnectedHandlers.ContainsKey(type))
                    ConnectionConnectedHandlers[type].ForEach(e => e.Invoke(null, new ConnectionEventArgs(con)));

                return true;
            });
        }

        public static void DisconnectAll()
        {
            foreach (var kvp in Connections)
            {
                var type = kvp.Key;
                var connection = kvp.Value;
                ConnectionMessageHandlers[type].ForEach(e => connection.RaiseMessageEvent -= e);
                connection.Close();

                if(ConnectionDisconnectedHandlers.ContainsKey(type))
                    ConnectionDisconnectedHandlers[type].ForEach(e => e.Invoke(null, new ConnectionEventArgs(connection)));
            }
        }
    }
}