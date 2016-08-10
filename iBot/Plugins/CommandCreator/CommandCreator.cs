using System.Collections.Generic;

namespace IBot.Plugins.CommandCreator
{
    internal class CommandCreator
    {
        public CommandCreator(string title, List<CommandText> cmdtexts, int id)
        {
            Id = id;
            Title = title;
            CmdTexts = cmdtexts;
        }

        public string Title { get; }

        public int Id { get; }

        public List<CommandText> CmdTexts { get; }
    }
}
