using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IBot
{
    internal class DbUser
    {
        private readonly User _user;

        public DbUser(User user)
        {
            _user = user;
            DbChannel = new DbChannel();
            DbChannel.Name = _user.ChannelName ?? _user.Channel?.Name;
        }

        [Key]
        public string Username
        {
            get { return _user.Username; }
            set { _user.Username = value; }
        }
        
        public string ChannelName
        {
            get { return _user.ChannelName; }
            set { _user.ChannelName = value; }
        }

        [ForeignKey("ChannelName")]
        public DbChannel DbChannel { get; set; }

        public static IEnumerable<DbUser> From(IEnumerable<User> users) => users.Select(user => new DbUser(user)).ToList();
    }

    internal static class UserExtensions
    {
        public static DbUser ToDbUser(this User user) => new DbUser(user);
    }
}
