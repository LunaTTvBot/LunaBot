using System;
using IBot.Events;
using IBot.Facades.Events.CustomArgs;

namespace IBot.Facades.Events
{
    public class CommandManagerFacade
    {
        static CommandManagerFacade()
        {
            CommandManager.CommandCalledEvent +=
                (sender, args) => OnCommandCalledEvent(new CommandCalledEventArgsFacade(args.User, args.CommandType, args.CommandName, args.Msg));
        }

        public static event EventHandler<CommandCalledEventArgsFacade> CommandCalledEvent;

        private static void OnCommandCalledEvent(CommandCalledEventArgsFacade e) => CommandCalledEvent?.Invoke(null, e);
    }
}