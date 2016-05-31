using System;
using System.Text.RegularExpressions;

namespace IBot.Events.Commands
{
    internal class GlobalCommand : CommandBase
    {
        public Action<GlobalCommand, Match, string> Action { get; set; }
    }
}
