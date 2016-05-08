using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using IBot.Events.Commands;
using IBot.Events.CustomEventArgs;
using IBot.Misc;

namespace IBot.Events
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
            ConnectionManager.BotConnectedEvent += (s, e) =>
            {
                UserEventManager.UserPublicMessageEvent += CheckAndRaisePublicCommands;
                UserEventManager.UserWhisperMessageEvent += CheckAndRaiseWhisperCommands;
                IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += CheckAndRaiseGlobalCommands;
            };

            ConnectionManager.BotDisconnectedEvent += (s, e) =>
            {
                GlobalCommandList.Clear();
                PublicCommandList.Clear();
                WhisperCommandList.Clear();
                IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent -= CheckAndRaiseGlobalCommands;
            };
        }

        private static void CheckAndRaiseWhisperCommands(object sender, UserWhisperMessageEventArgs eArgs)
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

        private static void CheckAndRaisePublicCommands(object sender, UserPublicMessageEventArgs eArgs)
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

        private static void CheckAndRaiseGlobalCommands(object sender, MessageEventArgs eArgs)
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
}
