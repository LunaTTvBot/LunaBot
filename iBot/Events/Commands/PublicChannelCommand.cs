using System;
using System.Text.RegularExpressions;
using IBot.Events.Args.Users;

namespace IBot.Events.Commands
{
    internal class PublicChannelCommand : CommandBase
    {
        public Action<PublicChannelCommand, Match, UserPublicMessageEventArgs> Action { get; set; }
    }
}