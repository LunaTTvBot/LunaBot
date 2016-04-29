using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using IRCConnectionTest.Events.ComstumEventArgs;

namespace IRCConnectionTest.Events
{
    internal static class CommandManager
    {
        private static readonly List<GlobalCommand> GlobalCommandList = new List<GlobalCommand>();
        private static readonly List<PublicChannelCommand> PublicCommandList = new List<PublicChannelCommand>();
        private static readonly List<WhisperCommand> WhisperCommandList = new List<WhisperCommand>();
        private static readonly Dictionary<string, Regex> RegExStack = new Dictionary<string, Regex>();

        static CommandManager()
        {
            var myThread = new Thread(Start);
            myThread.Start();
        }

        private static void Start()
        {
            IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += ConnectionOnRaiseMessageEvent;
            UserEventManager.UserPublicMessageEvent += UserEventManagerOnUserPublicMessageEvent;
            UserEventManager.UserWhisperMessageEvent += UserEventManagerOnUserWhisperMessageEvent;
        }

        private static void UserEventManagerOnUserWhisperMessageEvent(object sender,
            UserWhisperMessageEventArgs eArgs)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                WhisperCommandList.ForEach(command =>
                {
                    if (!RegExStack.ContainsKey(command.Name))
                        RegExStack.Add(command.Name, new Regex(command.RegEx));

                    var m = RegExStack[command.Name].Match(eArgs.Message);
                    if (m.Success)
                    {
                        ThreadPool.QueueUserWorkItem(__ => { command.Action(command, m, eArgs); });
                    }
                });
            });
        }

        private static void UserEventManagerOnUserPublicMessageEvent(object sender,
            UserPublicMessageEventArgs eArgs)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                PublicCommandList.ForEach(command =>
                {
                    if (!RegExStack.ContainsKey(command.Name))
                        RegExStack.Add(command.Name, new Regex(command.RegEx));

                    var m = RegExStack[command.Name].Match(eArgs.Message);
                    if (m.Success)
                    {
                        ThreadPool.QueueUserWorkItem(__ => { command.Action(command, m, eArgs); });
                    }
                });
            });
        }

        private static void ConnectionOnRaiseMessageEvent(object sender, MessageEventArgs eArgs)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                GlobalCommandList.ForEach(command =>
                {
                    if (!RegExStack.ContainsKey(command.Name))
                        RegExStack.Add(command.Name, new Regex(command.RegEx));

                    var m = RegExStack[command.Name].Match(eArgs.Message);
                    if (m.Success)
                    {
                        ThreadPool.QueueUserWorkItem(__ => { command.Action(command, m, eArgs.Message); });
                    }
                });
            });
        }

        public static void RegisterGlobalCommand(GlobalCommand command)
        {
            GlobalCommandList.Add(command);
        }

        public static void RegisterPublicChannelCommand(PublicChannelCommand command)
        {
            PublicCommandList.Add(command);
        }

        public static void RegisterWhisperCommand(WhisperCommand command)
        {
            WhisperCommandList.Add(command);
        }
    }

    internal class WhisperCommand
    {
        public string Name { get; set; }
        public string RegEx { get; set; }
        public Action<WhisperCommand, Match, UserWhisperMessageEventArgs> Action { get; set; }
    }

    internal class PublicChannelCommand
    {
        public string Name { get; set; }
        public string RegEx { get; set; }
        public Action<PublicChannelCommand, Match, UserPublicMessageEventArgs> Action { get; set; }
    }

    internal class GlobalCommand
    {
        public string Name { get; set; }
        public string RegEx { get; set; }
        public Action<GlobalCommand, Match, string> Action { get; set; }
    }
}