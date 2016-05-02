using System;
using System.Text.RegularExpressions;

namespace IBot.Events.Commands
{
    internal class GlobalCommand
    {
        public string Name { get; set; }

        public string RegEx { get; set; }

        public Action<GlobalCommand, Match, string> Action { get; set; }
    }
}