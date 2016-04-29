namespace IRCConnectionTest
{
    internal class User
    {
        public User(string username)
        {
            Username = username;
        }

        public virtual Channel Channel { get; set; }

        public virtual string ChannelName { get; set; }

        public virtual string Username { get; set; }
    }
}
