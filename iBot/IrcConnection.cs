using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace iBot
{
    internal class IrcConnection
    {
        private readonly int _port;
        private readonly string _url;

        private readonly string _user;
        private List<string> _channelList = new List<string>();
        private TcpClient _client;

        private readonly string _nick;
        private readonly string _password;

        private StreamReader _reader;
        private NetworkStream _stream;
        private StreamWriter _writer;

        public IrcConnection(string user, string password, string nick, string url, int port)
        {
            _user = user;
            _password = password;
            _nick = nick;
            _url = url;
            _port = port;
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

            Write("USER " + _user);
            Write("PASS " + _password);
            Write("NICK" + _nick);

            return true;
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

        public StreamReader GetReader()
        {
            return _reader;
        }

        public NetworkStream GetStream()
        {
            return _stream;
        }
    }
}