using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IBot
{
    internal class DbChannel
    {
        private readonly Channel _channel;

        public DbChannel() : this(new Channel()) {}

        public DbChannel(Channel channel)
        {
            _channel = channel;
            DbUsers = DbUser.From(_channel.Users).ToList();
            DbUsers.ForEach(u => u.ChannelName = _channel.Name);
        }

        [Key]
        public string Name
        {
            get { return _channel.Name; }
            set { _channel.Name = value; }
        }
        
        public List<DbUser> DbUsers { get; set; }
    }
}
