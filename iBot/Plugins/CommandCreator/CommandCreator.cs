using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IBot.Core;
using IBot.Events;
using IBot.Events.Args.Users;
using IBot.Events.Commands;
using IBot.Facades.Core.Settings;
using IBot.Plugins.Poll.EventArgs;
using NLog;
using CommandCreatorLocale = IBot.Resources.Plugins.CommandCreator.commandcreator;

namespace IBot.Plugins.CommandCreator
{
    internal class CommandCreator : IPlugin
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private const string CommandCreatePattern = @"^\s([a-zA-Z][a-zA-Z0-9_-]*)\s(.{2,})$";

        private static readonly Regex CommandCreateRegEx = new Regex(CommandCreatePattern);

        public string PluginName => CommandCreatorLocale.commandcreator_plugin_name;

        public void BindEvents()
        {
            _logger.Debug("{0} {1}", PluginName, CommandCreatorLocale.commandcreator_plugin_loaded);

            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                RegEx = "!addCom(.*)",
                Name = PluginName,
                Action = CommandAction
            });
        }

        public void Init()
        {
            _logger.Trace($"{PluginName} {CommandCreatorLocale.commandcreator_plugin_loaded}");
        }

    }
}
