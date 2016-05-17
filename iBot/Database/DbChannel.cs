using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IBot.Models;

namespace IBot.Database
{
    internal class DbChannel
    {
        public DbChannel() : this(new Channel()) {}

        public DbChannel(Channel channel)
        {
            Name = channel.Name;

            DbUsers = DbUser.From(channel.Users.ToArray()).ToList();
            DbUsers.ForEach(u =>
            {
                u.ChannelName = channel.Name;
                u.DbChannel = this;
            });
        }

        [Key]
        public string Name { get; set; }

        public List<DbUser> DbUsers { get; set; }
    }
}
