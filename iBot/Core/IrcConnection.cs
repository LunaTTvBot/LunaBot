using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using IBot.Core.Settings;
using IBot.Events.Args.Connections;
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
        private static readonly string[] MessageSeparators = {"\r\n"};
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ConnectionType, IrcConnection> ConType =
            new Dictionary<ConnectionType, IrcConnection>();

        private readonly TwitchCaps[] _caps;

        private readonly List<string> _channelList = new List<string>();
        private readonly string _nick;
        private readonly string _password;
        private readonly int _port;
        private readonly ConcurrentQueue<string> _prioritySendQueue = new ConcurrentQueue<string>();
        private readonly int _rateLimit;
        private readonly bool _secure;

        private readonly ConcurrentQueue<string> _sendQueue = new ConcurrentQueue<string>();
        private readonly string _url;
        private readonly string _user;
        private readonly ConnectionType _type;

        private TcpClient _client;
        private Thread _readerThread;
        private Thread _senderThread;
        private SslStream _sslstream;
        private NetworkStream _stream;
        private bool _work;

        internal IrcConnection(string user, string password, string nick, string url, int port, ConnectionType conType,
            TwitchCaps[] caps, bool secure, int rateLimit)
        {
            Logger.Debug("IrcConnection created");

            _user = user;
            _password = password;
            _nick = nick;
            _url = url;
            _port = port;
            _caps = caps;
            _secure = secure;
            _rateLimit = rateLimit;
            _type = conType;

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
        public event EventHandler<ConnectionUnexpectedCloseEventArgs> UnexpectedCloseEvent;

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Logger.Error($"Certificate error: {sslPolicyErrors}");

            return false;
        }

        public IEnumerable<string> GetChannels()
        {
            return new List<string>(_channelList);
        }

        public bool Connect()
        {
            if (_client == null)
            {
                try
                {
                    _client = new TcpClient(_url, _port);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    UnexpectedClose(Resources.IrcConnection.tcp_client_error);
                    return false;
                }

                _sslstream = null;
                _stream = null;
            }

            if (!_client.Connected) return false;

            if (_stream == null)
                _stream = _client.GetStream();

            if (_secure && _sslstream == null)
            {
                try
                {
                    var sslStream = new SslStream(
                        _client.GetStream(),
                        false,
                        ValidateServerCertificate,
                        null
                        );
                    sslStream.AuthenticateAsClient(_url);
                    _sslstream = sslStream;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    UnexpectedClose(Resources.IrcConnection.ssl_stream_error);
                    return false;
                }
            }

            _work = true;
            _readerThread = new Thread(Reader);
            _readerThread.Start();

            _senderThread = new Thread(Sender);
            _senderThread.Start();

            EnqueueMessage(@"USER " + _user, true);
            EnqueueMessage(@"PASS " + _password, true);
            EnqueueMessage(@"NICK " + _nick, true);

            if (_caps != null)
            {
                foreach (var cap in _caps)
                {
                    EnqueueMessage(@"CAP REQ :twitch.tv/" + cap.ToString().ToLower(), true);
                }
            }

            if (_channelList.Count != 0)
            {
                _channelList.ForEach(Join);
            }

            return true;
        }

        private void UnexpectedClose(string reason)
        {
            try
            {
                _work = false;

                _client?.Close();
                _stream?.Close();
                _sslstream?.Close();

                _client = null;
                _stream = null;
                _sslstream = null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            OnUnexpectedCloseEvent(new ConnectionUnexpectedCloseEventArgs(reason, _type));
        }

        public bool Close()
        {
            try
            {
                _work = false;

                _client?.Close();
                _stream?.Close();
                _sslstream?.Close();

                _client = null;
                _stream = null;
                _sslstream = null;

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public void Join(string channel)
        {
            EnqueueMessage(@"JOIN #" + channel, true);

            if (_channelList.FindIndex(c => c == channel) == 0)
                _channelList.Add(channel);
        }

        private void Write(string message)
        {
            message = message.Replace("\r\n", " ");

            var buffer = Encoding.UTF8.GetBytes(message + "\r\n");
            try
            {
                if (_secure)
                {
                    _sslstream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    _stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (ObjectDisposedException)
            {
                UnexpectedClose(Resources.IrcConnection.lost_connection);
            }
            catch (IOException)
            {
                UnexpectedClose(Resources.IrcConnection.lost_connection);
            }
        }

        private void EnqueueMessage(string message, bool hasPriority = false)
        {
            (hasPriority ? _prioritySendQueue : _sendQueue).Enqueue(message);
        }

        public static void Write(ConnectionType conType, string channel, string msg)
        {
            if (!ConType.ContainsKey(conType))
                return;

            ConType[conType].EnqueueMessage(string.Format(GlobalTwitchPatterns.WritePublicFormat, channel, msg));
        }

        public static void Write(ConnectionType conType, AnswerType aType, string target, string msg)
        {
            if (!ConType.ContainsKey(conType))
                return;

            switch (aType)
            {
                case AnswerType.Private:
                    Write(ConnectionType.BotCon, SettingsManager.GetSettings<ConnectionSettings>().ChannelList.First(), $"/w {target} {msg}");
                    break;
                case AnswerType.Public:
                    Write(ConnectionType.BotCon, target, msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aType), aType, null);
            }
        }

        private void Sender()
        {
            while (_client.Connected)
            {
                if (!_work)
                    return;

                string message;
                if (!_prioritySendQueue.IsEmpty && _prioritySendQueue.TryDequeue(out message))
                {
                    Write(message);
                }
                else if (!_sendQueue.IsEmpty && _sendQueue.TryDequeue(out message))
                {
                    Write(message);

                    Thread.Sleep(_rateLimit);
                }

                Thread.Sleep(1);
            }
        }

        private void Reader()
        {
            var buffer = new byte[1024];
            var msg = new StringBuilder();

            var badMessagesReceived = 0;

            Stream stream;
            if (_secure)
            {
                stream = _sslstream;
            }
            else
            {
                stream = _stream;
            }

            while (_client.Connected)
            {
                Thread.Sleep(1);

                if (!_work)
                    return;

                if (!_stream.DataAvailable) continue;

                Array.Clear(buffer, 0, buffer.Length);

                stream.Read(buffer, 0, buffer.Length);
                var data = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                msg.Append(data);

                var msgstr = msg.ToString();
                if (!msgstr.EndsWith("\r\n"))
                {
                    var all0 = buffer.All(b => b == 0);

                    if (all0)
                    {
                        if (++badMessagesReceived <= 2)
                            continue;

                        UnexpectedClose(Resources.IrcConnection.bad_messages);
                        break;
                    }

                    var idx = msgstr.LastIndexOf("\r\n", StringComparison.Ordinal);
                    if (idx != -1)
                    {
                        idx += 2;
                        msg.Remove(0, idx);
                        msgstr = msgstr.Substring(0, idx);
                    }
                    else
                        continue;
                }
                else
                    msg.Clear();

                var messages = msgstr.Split(MessageSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (var message in messages)
                {
                    if (message.StartsWith("PING"))
                    {
                        EnqueueMessage(message.Replace("PING", "PONG"), true);
                        Logger.Debug("PING received -> Send PONG!");
                    }

                    OnRaiseMessageEvent(new MessageEventArgs(message));
                }
            }
        }

        protected virtual void OnRaiseMessageEvent(MessageEventArgs e) => RaiseMessageEvent?.Invoke(this, e);
        protected virtual void OnUnexpectedCloseEvent(ConnectionUnexpectedCloseEventArgs e) => UnexpectedCloseEvent?.Invoke(this, e);
    }
}