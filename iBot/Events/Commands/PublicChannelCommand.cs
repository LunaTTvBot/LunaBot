using System;
using System.Text.RegularExpressions;
using IRCConnectionTest.Events.CustomEventArgs;

namespace IRCConnectionTest.Events.Commands
{
    internal class PublicChannelCommand
    {
        public string Name { get; set; }

        public string RegEx { get; set; }

        public Action<PublicChannelCommand, Match, UserPublicMessageEventArgs> Action { get; set; }
    }
}