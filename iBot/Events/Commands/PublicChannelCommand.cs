using System;
using System.Text.RegularExpressions;
using IBot.Events.Args.Users;

namespace IBot.Events.Commands
{
    internal class PublicChannelCommand
    {
        public string Name { get; set; }

        public string RegEx { get; set; }

        public Action<PublicChannelCommand, Match, UserPublicMessageEventArgs> Action { get; set; }
    }
}