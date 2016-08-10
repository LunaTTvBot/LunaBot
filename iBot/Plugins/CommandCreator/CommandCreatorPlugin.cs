using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IBot.Core;
using IBot.Events;
using IBot.Events.Args.Users;
using IBot.Events.Commands;
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
         * 
         * Created Commands
         * -----
         * Params:
         * _TITLE_      String (Title of a created command)
         * 
         * commands:
         * !(_TITLE_)                           - Activates an created command
         */

        private const string CommandCreateTitlePattern = @"^\screate\s([\w]{2,})\s(.*)$";
        private const string CommandCreatePattern = @"^\s([a-zA-Z][a-zA-Z0-9_-]*)\s(.{2,})$";
        private const string CommandTextPattern = @"([^|]+)\|?";
        private const string CommandListPattern = @"^\slist";
        private const string CommandActionsPattern = @"^\s(delete)[:\s]([0-9]+)";
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex CommandCreateTitleRegEx = new Regex(CommandCreateTitlePattern);
        private static readonly Regex CommandTextRegEx = new Regex(CommandTextPattern);
        private static readonly Regex CommandListRegEx = new Regex(CommandListPattern);
        private static readonly Regex CommandActionsRegEx = new Regex(CommandActionsPattern);

        private static readonly List<CommandCreator> CommandsStack = new List<CommandCreator>();

        public string PluginName => CommandCreatorLocale.commandcreator_plugin_name;

        public void Init()
        {
            _logger.Debug("{0} {1}", PluginName, CommandCreatorLocale.commandcreator_plugin_loaded);
            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                RegEx = "!command(.*)",
                Name = PluginName,
                Action = CommandAction
            });

        }

        public static event EventHandler<CommandCreatedEventArgs> CommandCreatedEvent;
        private static void OnCommandCreatedEvent(CommandCreatedEventArgs e) => CommandCreatedEvent?.Invoke(null, e);

        public static event EventHandler<CommandChangedEventArgs> CommandDeletedEvent;
        private static void OnCommandDeletedEvent(CommandChangedEventArgs e) => CommandDeletedEvent?.Invoke(null, e);

        public static List<CommandCreator> GetCommandList() => CommandsStack;

        public static CommandCreator CreateCommand(string title, string[] cmdtexts)
        {
            var list = new List<CommandText>();
            foreach (var cmdtext in cmdtexts)
            {
                list.Add(new CommandText(list.Count + 1, cmdtext));
            }
            var idx = 1;
            if(CommandsStack.Count > 0)
            {
                var lIdx = CommandsStack[CommandsStack.Count - 1].Id;
            }

            var p = new CommandCreator(title, list, idx);
            CommandsStack.Add(p);

            OnCommandCreatedEvent(new CommandCreatedEventArgs(p));
            return p;
        }

        public static CommandCreator DeleteCommand(int id)
        {
            var p = CommandsStack.Find(command => command.Id == id);

            if (p == null)
                return null;

            CommandsStack.Remove(p);

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
            if (CommandsStack.Count > 0)
            {
                var lIdx = CommandsStack[CommandsStack.Count - 1].Id;
                idx = lIdx + 1;
            }

            var cmdText = list.Select((t, i) => new CommandText(i + 1, t)).ToList();

            var p = new CommandCreator(m.Groups[1].Value, cmdText, idx);
            CommandsStack.Add(p);

            SendMessage(string.Format(CommandCreatorLocale.commandcreator_created, p.Id), answerType, answerTarget);
            OnCommandCreatedEvent(new CommandCreatedEventArgs(p));

            return true;
        }


        private static bool HandleCommandListCommand(string commandParams, AnswerType answerType, string answerTarget)
        {
            var m = CommandListRegEx.Match(commandParams);
            if (!m.Success) return false;

            if (CommandsStack.Count == 0)
            {
                SendMessage(CommandCreatorLocale.commandcreator_no_commands, answerType, answerTarget);
                return true;
            }

            var stringB = new StringBuilder();
            CommandsStack.ForEach(command =>
            {
                stringB.Clear();

                stringB.Append($"#{command.Id}");
                if (command.Title != "")
                    stringB.Append($" - {command.Title}");

                command.CmdTexts.ForEach(text =>
                {
                    var idx = command.CmdTexts.IndexOf(text);
                    if (idx != 0)
                        stringB.Append(", ");
                    stringB.Append($"{text.Id}:{text.Name}");
                });

                SendMessage(stringB.ToString(), answerType, answerTarget);
            });

            return true;
        }

        private static void HandleCommandDelete(CommandCreator c, AnswerType answerType, string answerTarget)
        {
            CommandsStack.Remove(c);
            SendMessage(string.Format(CommandCreatorLocale.commandcreator_command_removed, c.Id), answerType, answerTarget);
            OnCommandDeletedEvent(new CommandChangedEventArgs(c));
        }

        private static void HandleCommandActions(string commandParams, AnswerType answerType, string answerTarget)
        {
            var m = CommandActionsRegEx.Match(commandParams);
            if (!m.Success) return;

            var cIdx = Convert.ToInt32(m.Groups[2].Value);
            if (!CommandsStack.Exists(command => command.Id == cIdx))
            {
                SendMessage(string.Format(CommandCreatorLocale.commandcreator_command_not_found, cIdx), answerType,
                    answerTarget);
                return;
            }

            var c = CommandsStack.Find(command => command.Id == cIdx);

            switch (m.Groups[1].Value)
            {
                case "delete":
                    HandleCommandDelete(c, answerType, answerTarget);
                    break;
                default:
                    SendMessage(string.Format(CommandCreatorLocale.commandcreator_wrong_command, cIdx), answerType,
                        answerTarget);
                    break;
            }
        }

        private static void CommandAction(PublicChannelCommand command, Match match, UserPublicMessageEventArgs eArgs)
        {
            var commandParams = match.Groups[1].Value;

            try
            {
                // !command
                // Always send private due to a lot of text.
                if (HandleCommandBaseCommand(commandParams, AnswerType.Private, eArgs.UserName))
                    return;

                HandleCommandCommands(commandParams, AnswerType.Public, eArgs.Channel);
            }
            catch (Exception)
            {
                SendMessage(CommandCreatorLocale.commandcreator_base_error, AnswerType.Public,
                    eArgs.Channel);
            }
        }

        private static void HandleCommandCommands(string pollParams, AnswerType aType, string target)
        {
            // !command create(_TITLE_) _TEXT_ 
            if (HandleCommandCreateTitleCommand(pollParams, aType, target))
                return;

            // !command list
            if (HandleCommandListCommand(pollParams, aType, target))
                return;

            // !command delete:_ID_
            HandleCommandActions(pollParams, aType, target);
        }

    }
}
