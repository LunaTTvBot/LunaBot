using System;
using System.Text.RegularExpressions;
using IBot.Events.Args.Users;

namespace IBot.Events.Commands
{
    internal class WhisperCommand
    {
        public string Name { get; set; }

        public string RegEx { get; set; }

        public Action<WhisperCommand, Match, UserWhisperMessageEventArgs> Action { get; set; }
    }
}