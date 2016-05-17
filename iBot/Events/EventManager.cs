using IBot.Events.CustomEventArgs;
using IBot.Misc;

namespace IBot.Events
{
    internal static class EventManager
    {
        static EventManager()
        {
            ConnectionManager.BotConnectedEvent += ConnectionManagerOnBotConnectedEvent;
            ConnectionManager.BotDisconnectedEvent += ConnectionManagerOnBotDisconnectedEvent;
        }

        private static void ConnectionManagerOnBotDisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            ChannelEventManager.RemoveEventHandlers();
            UserEventManager.RemoveEventHandlers();
            ErrorManager.RemoveEventHandlers();
        }

        private static void ConnectionManagerOnBotConnectedEvent(object sender, ConnectionEventArgs e)
        {
            ChannelEventManager.BindToMessageEvent();
            UserEventManager.BindToMessageEvent();
            ErrorManager.BindToMessageEvent();
        }
    }
}