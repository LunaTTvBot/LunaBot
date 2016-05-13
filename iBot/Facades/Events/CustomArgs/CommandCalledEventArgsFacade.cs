namespace IBot.Facades.Events.CustomArgs
{
    public class CommandCalledEventArgsFacade
    {
        public CommandCalledEventArgsFacade(string user, string commandType, string commandName, string msg)
        {
            User = user;
            CommandType = commandType;
            CommandName = commandName;
            Msg = msg;
        }

        public string User { get; }

        public string CommandType { get; }
        public string CommandName { get; }
        public string Msg { get; set; }
    }
}