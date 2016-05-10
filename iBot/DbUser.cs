using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IBot
{
    internal class DbUser
    {
        public DbUser() {}

        public DbUser(User user)
        {
            Username = user.Username;
            ChannelName = user.ChannelName;
            DbChannel = new DbChannel();
            DbChannel.Name = user.ChannelName ?? user.Channel?.Name;
            DbChannel.DbUsers.Add(this);
        }

        [Key]
        public string Username { get; set; }

        public string ChannelName { get; set; }

        [ForeignKey("ChannelName")]
        public DbChannel DbChannel { get; set; }

        public static IEnumerable<DbUser> From(IEnumerable<User> users) => users.Select(user => new DbUser(user)).ToList();
    }

    internal static class UserExtensions
    {
        public static DbUser ToDbUser(this User user) => new DbUser(user);
    }
}
