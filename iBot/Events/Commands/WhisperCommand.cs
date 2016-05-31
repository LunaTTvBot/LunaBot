using System;
using System.Text.RegularExpressions;
using IBot.Events.Args.Users;

namespace IBot.Events.Commands
{
    internal class WhisperCommand : CommandBase
    {
        public Action<WhisperCommand, Match, UserWhisperMessageEventArgs> Action { get; set; }
    }
}
