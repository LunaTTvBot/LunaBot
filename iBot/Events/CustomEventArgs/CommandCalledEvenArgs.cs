using System;

namespace IBot.Events.CustomEventArgs
{
    internal class CommandCalledEvenArgs
    {
        public CommandCalledEvenArgs(string user, Type commandType, string commandName, string msg)
        {
            User = user;
            CommandName = commandName;
            Msg = msg;
            CommandType = commandType.Name;
        }

        public string User { get; }
        public string CommandName { get; set; }
        public string Msg { get; set; }
        public string CommandType { get; }
    }
}