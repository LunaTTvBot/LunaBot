using System;
using System.Collections.Generic;
using System.Linq;
using IBot.Events.Args.Connections;
using IBot.Events.Args.Users;
using NLog;

namespace IBot.Core
{
    internal static class IrcConnectionManager
    {
        private static Dictionary<ConnectionType, IrcConnection> _connections =
            new Dictionary<ConnectionType, IrcConnection>();

        private static Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>> _connectionMessageHandlers =
            new Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>>();

        private static Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>> _connectionConnectedHandlers =
            new Dictionary<ConnectionType, List<EventHandler<ConnectionEventArgs>>>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IrcConnection GetConnection(ConnectionType type) => _connections.ContainsKey(type)
                                                                              ? _connections[type]
                                                                              : null;

        public static bool RegisterConnection(string user, string password, string nickname, string url, int port, ConnectionType type)
        {
            try
            {
                var connection = new IrcConnection(user, password, nickname, url, port, type);

                _connections[type] = connection;

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
            if (!_connectionMessageHandlers.ContainsKey(type))
                return;

            if (!_connectionMessageHandlers[type].Contains(handler))
                return;

            _connectionMessageHandlers[type].Remove(handler);

            if (_connections.ContainsKey(type))
            {
                _connections[type].RaiseMessageEvent -= handler;
            }
        }

        public static void RegisterMessageHandler(ConnectionType type, EventHandler<MessageEventArgs> handler)
        {
            if (!_connectionMessageHandlers.ContainsKey(type))
                _connectionMessageHandlers.Add(type, new List<EventHandler<MessageEventArgs>>());

            // don't register methods twice
            if (_connectionMessageHandlers[type].Contains(handler))
                return;

            _connectionMessageHandlers[type].Add(handler);

            if (_connections.ContainsKey(type))
            {
                _connections[type].RaiseMessageEvent += handler;
            }
        }

        public static void RemoveOnConnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler)
        {
            if (!_connectionConnectedHandlers.ContainsKey(type))
                return;

            if (_connectionConnectedHandlers[type].Contains(handler))
            {
                _connectionConnectedHandlers[type].Remove(handler);
            }
        }

        public static void RegisterOnConnectedHandler(ConnectionType type, EventHandler<ConnectionEventArgs> handler)
        {
            if (!_connectionConnectedHandlers.ContainsKey(type))
                _connectionConnectedHandlers.Add(type, new List<EventHandler<ConnectionEventArgs>>());

            // don't register methods twice
            if (_connectionConnectedHandlers[type].Contains(handler))
                return;

            _connectionConnectedHandlers[type].Add(handler);
        }

        public static bool ReconnectAll()
        {
            DisconnectAll();
            return ConnectAll();
        }

        public static bool ConnectAll()
        {
            return _connections.All(kvp =>
            {
                var type = kvp.Key;
                var con = kvp.Value;

                if (_connectionMessageHandlers.ContainsKey(type))
                    _connectionMessageHandlers[type].ForEach(e => con.RaiseMessageEvent += e);

                if (!con.Connect())
                    return false;

                if (_connectionConnectedHandlers.ContainsKey(type))
                    _connectionConnectedHandlers[type].ForEach(e => e.Invoke(null, new ConnectionEventArgs(con)));

                return true;
            });
        }

        public static void DisconnectAll()
        {
            foreach (var kvp in _connections)
            {
                var type = kvp.Key;
                var connection = kvp.Value;
                _connectionMessageHandlers[type].ForEach(e => connection.RaiseMessageEvent -= e);
                connection.Close();
            }
        }
    }
}
