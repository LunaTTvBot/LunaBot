using System.Collections.Generic;

namespace IBot.Plugins.CommandCreator
{
    internal class CommandCreator
    {
        public CommandCreator(string title, List<CommandText> texts, int id)
        {
            Id = id;
            Title = title;
            Texts = texts;
        }

        public string Title { get; }

        public int Id { get; }

        public List<CommandText> Texts { get; }
    }
}
