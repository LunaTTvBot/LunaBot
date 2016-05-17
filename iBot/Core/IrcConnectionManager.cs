using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBot.Events.Args.Users;
using NLog;

namespace IBot.Core
{
    internal static class IrcConnectionManager
    {
        private static Dictionary<ConnectionType, IrcConnection> _connections =
            new Dictionary<ConnectionType, IrcConnection>();

        private static Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>> _connectionHandlers =
            new Dictionary<ConnectionType, List<EventHandler<MessageEventArgs>>>();

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

        public static void RemoveHandler(ConnectionType type, EventHandler<MessageEventArgs> handler)
        {
            if (!_connectionHandlers.ContainsKey(type))
                return;

            // don't register methods twice
            if (_connectionHandlers[type].Contains(handler))
            {
                _connectionHandlers[type].Remove(handler);

                if (_connections.ContainsKey(type))
                {
                    _connections[type].RaiseMessageEvent -= handler;
                }
            }
        }

        public static void RegisterHandler(ConnectionType type, EventHandler<MessageEventArgs> handler)
        {
            if (!_connectionHandlers.ContainsKey(type))
                _connectionHandlers.Add(type, new List<EventHandler<MessageEventArgs>>());

            // don't register methods twice
            if (_connectionHandlers[type].Contains(handler))
                return;

            _connectionHandlers[type].Add(handler);

            if (_connections.ContainsKey(type))
            {
                _connections[type].RaiseMessageEvent += handler;
            }
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
                _connectionHandlers[type].ForEach(e => con.RaiseMessageEvent += e);
                var success = con.Connect();
                // wait a little so that this connection can initialize properly
                if (success)
                    System.Threading.Thread.Sleep(1000);

                return success;
            });
        }

        public static void DisconnectAll()
        {
            foreach (var kvp in _connections)
            {
                var type = kvp.Key;
                var connection = kvp.Value;
                _connectionHandlers[type].ForEach(e => connection.RaiseMessageEvent -= e);
                connection.Close();
            }
        }
    }
}
