using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IRCConnectionTest.Misc;
using SQLite.CodeFirst;

namespace IRCConnectionTest
{
    internal class User
    {
        public User() {}

        public User(string username)
        {
            Username = username;
        }

        [Key]
        public int Id { get; set; }

        public string ChannelName { get; set; }

        [ForeignKey("ChannelName")]
        public virtual Channel Channel { get; set; }

        public string Username { get; set; }

        public static IEnumerable<User> GetAll()
        {
            return DatabaseContext.Get().Users;
        }
    }
}
