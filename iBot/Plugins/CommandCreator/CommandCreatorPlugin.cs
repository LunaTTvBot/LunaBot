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
using IBot.Plugins.CommandCreator.EventArgs;
using NLog;
using CommandCreatorLocale = IBot.Resources.Plugins.CommandCreator.CommandCreatorLocale;

namespace IBot.Plugins.CommandCreator
{
    internal class CommandCreatorPlugin : IPlugin
    {
        /**
     * CommandCreator Plugin by Lion Haupt
     * 
     * !command
     * -----
     * Params:
     * _TITLE_      string
     * _TEXT_       string
     * 
     * Commands:
     * !command                             - Description
     * !command create(_TITLE_) _Text_      - Creates a command with given _TEXT_
     * !command delete(_TITLE)              - Deletes command with _TITLE_
     * !command list                        - Lists all created commands
     */

        private const string CommandCreateTitlePattern = @"^\screate\((.*)\)\s(.*)";
        private const string CommandCreatePattern = @"^\s([a-zA-Z][a-zA-Z0-9_-]*)\s(.{2,})$";
        private const string CommandTextPattern = @"([^|]+)\|?";
        private const string CommandListPattern = @"^\slist";
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex CommandCreateTitleRegEx = new Regex(CommandCreateTitlePattern);
        private static readonly Regex CommandCreateRegEx = new Regex(CommandCreatePattern);
        private static readonly Regex CommandTextRegEx = new Regex(CommandTextPattern);
        private static readonly Regex CommandListRegEx = new Regex(CommandTextPattern);

        private static readonly List<CommandCreator> CommandStack = new List<CommandCreator>();

        public string PluginName => CommandCreatorLocale.commandcreator_plugin_name;

        public void Init()
        {
            _logger.Debug("{0} {1}", PluginName, CommandCreatorLocale.commandcreator_plugin_loaded);
            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                RegEx = "!command(.*)",
                Name = PluginName,
                //Action = CommandAction
            });
        }

        public static event EventHandler<CommandCreatedEventArgs> CommandCreatedEvent;
        private static void OnCommandCreatedEvent(CommandCreatedEventArgs e) => CommandCreatedEvent?.Invoke(null, e);

        public static event EventHandler<CommandChangedEventArgs> CommandDeletedEvent;
        private static void OnCommandDeletedEvent(CommandChangedEventArgs e) => CommandDeletedEvent?.Invoke(null, e);

        public static List<CommandCreator> GetCommandList() => CommandStack;

        public static CommandCreator CreateCommand(string title, string[] texts)
        {
            var list = new List<CommandText>();
            foreach (var text in texts)
            {
                list.Add(new CommandText(list.Count + 1, text));
            }
            var idx = 1;
            if(CommandStack.Count > 0)
            {
                var lIdx = CommandStack[CommandStack.Count - 1].Id;
            }

            var p = new CommandCreator(title, list, idx);
            CommandStack.Add(p);

            OnCommandCreatedEvent(new CommandCreatedEventArgs(p));
            return p;
        }

        public static CommandCreator DeleteCommand(int id)
        {
            var p = CommandStack.Find(command => command.Id == id);

            if (p == null)
                return null;

            CommandStack.Remove(p);

            OnCommandDeletedEvent(new CommandChangedEventArgs(p));
            return p;
        }

        private static void SendMessage(string msg, AnswerType aType, string target)
        {
            IrcConnection.Write(ConnectionType.BotCon, aType, target, msg);
        }

        private static bool HandleCommandBaseCommand(string commandParams, AnswerType answerT, string answerTarget)
        {
            if (commandParams != "") return false;

            SendMessage(CommandCreatorLocale.commandcreator_help, answerT, answerTarget);

            return true;
        }

        private static bool HandleCommandCreateTitleCommand(string commandParams, AnswerType answerType, string answerTarget)
        {
            var m = CommandCreateTitleRegEx.Match(commandParams);
            if (!m.Success) return false;

            var optM = CommandTextRegEx.Matches(m.Groups[2].Value);
            if (optM.Count <= 0) return false;

            var list = (from Match match in optM select match.Groups[1].Value).ToList();
            var idx = 1;
            if (CommandStack.Count > 0)
            {
                var lIdx = CommandStack[CommandStack.Count - 1].Id;
                idx = lIdx + 1;
            }

            var cmdText = list.Select((t, i) => new CommandText(i + 1, t)).ToList();

            var p = new CommandCreator(m.Groups[1].Value, cmdText, idx);
            CommandStack.Add(p);

            SendMessage(string.Format(CommandCreatorLocale.commandcreator_created, p.Id), answerType, answerTarget);
            OnCommandCreatedEvent(new CommandCreatedEventArgs(p));

            return true;
        }

        private static void CommandAction(PublicChannelCommand command, Match match, UserPublicMessageEventArgs eArgs)
        {

        }



    }
}
