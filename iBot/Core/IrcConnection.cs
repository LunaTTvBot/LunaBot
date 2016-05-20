using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using IBot.Events.Args.Users;
using NLog;

namespace IBot.Core
{
    internal enum ConnectionType
    {
        BotCon,
        ChatCon
    }

    internal enum AnswerType
    {
        Public,
        Private
    }

    internal class IrcConnection
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ConnectionType, IrcConnection> ConType =
            new Dictionary<ConnectionType, IrcConnection>();

        private readonly TwitchCaps[] _caps;

        private readonly List<string> _channelList = new List<string>();
        private readonly string _nick;
        private readonly string _password;
        private readonly int _port;
        private readonly int _rateLimit;
        private readonly bool _secure;
        private readonly string _url;
        private readonly string _user;

        private TcpClient _client;
        private ConcurrentQueue<string> _prioritySendQueue = new ConcurrentQueue<string>();
        private StreamReader _reader;

        private ConcurrentQueue<string> _sendQueue = new ConcurrentQueue<string>();
        private SslStream _sslstream;
        private NetworkStream _stream;
        private Thread _thread;
        private bool _work;
        private StreamWriter _writer;

        internal IrcConnection(string user, string password, string nick, string url, int port, ConnectionType conType,
            TwitchCaps[] caps, bool secure, int rateLimit)
        {
            _logger.Debug("IrcConnection created");

            _user = user;
            _password = password;
            _nick = nick;
            _url = url;
            _port = port;
            _caps = caps;
            _secure = secure;
            _rateLimit = rateLimit;

            try
            {
                ConType.Add(conType, this);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    "This type of connection already exists. Use IrcConnectionManager.GetConnection(ConnectionType) to get the instance.");
            }
        }

        public event EventHandler<MessageEventArgs> RaiseMessageEvent;

        public bool Connect()
        {
            if (_client == null)
            {
                try
                {
                    _client = new TcpClient(_url, _port);
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    Close();
                    return false;
                }

                _sslstream = null;
                _stream = null;
                _reader = null;
                _writer = null;
            }

            if (!_client.Connected) return false;

            if (_stream == null)
                _stream = _client.GetStream();

            if(_secure && _sslstream == null) {
                try 
                {
                    var sslstream = new SslStream(_stream);
                    sslstream.AuthenticateAsClient(_url);
                    _sslstream = sslstream;
                } 
                catch(ObjectDisposedException) 
                {
                    Close();
                    return false;
                }
            }

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

            _work = true;
            _thread = new Thread(ReadConnection);
            _thread.Start();

            return true;
        }

        public bool Close()
        {
            try
            {
                _work = false;

                _client?.Close();
                _stream?.Close();
                _reader?.Close();
                _writer?.Close();

                _client = null;
                _stream = null;
                _reader = null;
                _writer = null;

                return true;
            }
            catch (Exception e)
            {
                _logger.Warn(e);
                return false;
            }
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

        public static void Write(ConnectionType conType, AnswerType aType, string target, string msg)
        {
            if (!ConType.ContainsKey(conType))
                return;

            switch (aType)
            {
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
                if (!_work)
                    return;

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