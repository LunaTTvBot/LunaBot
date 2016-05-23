namespace IBot.Facades.Core
{
    public class User
    {
        public User(string name, string channel)
        {
            Name = name;
            Channel = channel;
        }

        public virtual string Channel { get; internal set; }

        public virtual string Name { get; internal set; }

        public virtual string Id => $"{Channel}#{Name}";
    }
}
