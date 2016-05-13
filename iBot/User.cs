namespace IBot
{
    internal class User : IExtendable
    {
        public User(string username)
        {
            Username = username;
        }

        public virtual Channel Channel { get; set; }

        public virtual string ChannelName { get; set; }

        public virtual string Username { get; set; }

        public virtual string Id => $"{ChannelName ?? Channel.Name}#{Username}";

        public virtual string ClassName => GetType().FullName;
    }
}
