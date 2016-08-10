namespace IBot.Plugins.CommandCreator
{
    internal class CommandText
    {
        public CommandText(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}
