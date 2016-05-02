namespace IRCConnectionTest.Plugins.Poll
{
    internal class PollOption
    {
        public PollOption(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}