using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using IRCConnectionTest.Events.CustomEventArgs;

namespace IRCConnectionTest.Events
{
    internal static class CommandManager
    {
        private static readonly List<ChannelCommand> CommandList = new List<ChannelCommand>();
        private static readonly Dictionary<string, Regex> RegExStack = new Dictionary<string, Regex>();

        static CommandManager()
        {
            var myThread = new Thread(Start);
            myThread.Start();
        }

        private static void Start()
        {
            IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += ConnectionOnRaiseMessageEvent;
        }

        private static void ConnectionOnRaiseMessageEvent(object sender, MessageEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                CommandList.ForEach(command =>
                {
                    if (!RegExStack.ContainsKey(command.Name))
                        RegExStack.Add(command.Name, new Regex(command.RegEx));

                    var m = RegExStack[command.Name].Match(e.Message);
                    if (m.Success)
                    {
                        ThreadPool.QueueUserWorkItem(__ => { command.Action(command, m, e.Message); });
                    }
                });
            });
        }

        public static void RegisterCommand(ChannelCommand command)
        {
            CommandList.Add(command);
        }
    }

    internal class ChannelCommand
    {
        public string Name { get; set; }
        public string RegEx { get; set; }
        public Action<ChannelCommand, Match, string> Action { get; set; }
    }
}