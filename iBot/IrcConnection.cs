using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using IRCConnectionTest.Events.CustomEventArgs;
using IRCConnectionTest.Misc;

namespace IRCConnectionTest
{
    internal enum ConnectionType
    {
        BotCon,
        ChatCon
    }

    internal enum AnswerType {
        Public,
        Private
    }

    internal class IrcConnection
    {
        private static readonly Dictionary<ConnectionType, IrcConnection> ConType =
            new Dictionary<ConnectionType, IrcConnection>();

        private readonly List<string> _channelList = new List<string>();
        private readonly string _nick;
        private readonly string _password;
        private readonly int _port;
        private readonly string _url;

        private readonly string _user;
        private TcpClient _client;

        private StreamReader _reader;
        private NetworkStream _stream;

        private Thread _thread;
        private StreamWriter _writer;

        public IrcConnection(string user, string password, string nick, string url, int port, ConnectionType conType)
        {
            _user = user;
            _password = password;
            _nick = nick;
            _url = url;
            _port = port;

            try
            {
                ConType.Add(conType, this);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    "This type of connection already exists. Use GetIrcConnection(conType) to get the instance.");
            }
        }

        public event EventHandler<MessageEventArgs> RaiseMessageEvent;

        public static IrcConnection GetIrcConnection(ConnectionType conType)
        {
            return ConType[conType];
        }

        public bool Connect()
        {
            if (_client == null)
            {
                _client = new TcpClient(_url, _port);
                _stream = null;
                _reader = null;
                _writer = null;
            }

            if (!_client.Connected) return false;

            if (_stream == null)
                _stream = _client.GetStream();

            if (_reader == null && _stream.CanRead)
                _reader = new StreamReader(_stream);

            if (_writer == null && _stream.CanWrite)
                _writer = new StreamWriter(_stream);

            if (_writer == null || _reader == null) return false;

            Write(@"USER " + _user);
            Write(@"PASS " + _password);
            Write(@"NICK " + _nick);
            Write(@"CAP REQ :twitch.tv/membership");
            Write(@"CAP REQ :twitch.tv/commands");
            Write(@"CAP REQ :twitch.tv/tags");

            if (_channelList.Count != 0)
            {
                _channelList.ForEach(Join);
            }

            _thread = new Thread(ReadConnection);
            _thread.Start();

            return true;
        }

        public void Join(string channel)
        {
            Write(@"JOIN #" + channel);

            if (_channelList.FindIndex(c => c == channel) == 0)
                _channelList.Add(channel);
        }

        public void Write(string msg)
        {
            if (_client == null || !_client.Connected)
                Connect();

            if (_writer == null)
                return;

            _writer.WriteLine(msg);
            _writer.Flush();
        }

        public static void Write(ConnectionType conType, string channel, string msg)
        {
            if (!ConType.ContainsKey(conType))
                return;

            ConType[conType].Write(string.Format(GlobalTwitchPatterns.WritePublicFormat, channel, msg));
        }

        public static void Write(ConnectionType conType, AnswerType aType, string target, string msg) {
            if(!ConType.ContainsKey(conType))
                return;

            switch(aType) {
                case AnswerType.Private:
                    Write(ConnectionType.BotCon, App.BotChannelList.First(), $"/w {target} {msg}");
                    break;
                case AnswerType.Public:
                    Write(ConnectionType.BotCon, target, msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aType), aType, null);
            }
        }

        private void ReadConnection()
        {
            while (_client.Connected)
            {
                if (!_stream.DataAvailable) continue;

                var data = _reader.ReadLine();

                if (data == null) continue;

                // check for PING and PONG back
                if (data.StartsWith("PING"))
                {
                    Write("PONG :tmi.twitch.tv");
                    Trace.WriteLine("PING RECEIVED - PONG SENT!");
                }

                OnRaiseMessageEvent(new MessageEventArgs(data));
            }
        }

        public StreamReader GetReader()
        {
            return _reader;
        }

        public NetworkStream GetStream()
        {
            return _stream;
        }

        protected virtual void OnRaiseMessageEvent(MessageEventArgs e) => RaiseMessageEvent?.Invoke(this, e);
    }
}