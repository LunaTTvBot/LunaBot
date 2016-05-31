using System.Text.RegularExpressions;

namespace IBot.Events.Commands
{
    public class CommandBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string RegEx { get; set; }

        public RegexOptions RegexOptions { get; set; }
    }
}
